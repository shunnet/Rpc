using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using ImpromptuInterface;
using Newtonsoft.Json;
using Snet.Core.extend;
using Snet.Model.data;
using Snet.Rpc.data;
using Snet.Rpc.@interface;
using Snet.Rpc.unility;
using Snet.Utility;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using static Snet.Rpc.data.Client;
namespace Snet.Rpc.client
{
    /// <summary>
    /// RPC 客户端<br/>
    /// <br/>
    /// 功能：<br/>
    /// - 基于 DotNetty 实现高性能 TCP RPC 通信<br/>
    /// - 支持接口代理透明调用远程服务<br/>
    /// - 支持身份认证和接口注册<br/>
    /// - 支持同步等待异步响应
    /// </summary>
    public class RpcClient : CoreUnify<RpcClient, Basics>, IRpc
    {
        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="basics">基础数据（包含服务器地址、端口、账号密码等）</param>
        public RpcClient(Basics basics) : base(basics) { }

        /// <summary>
        /// DotNetty Bootstrap 客户端实例
        /// </summary>
        private Bootstrap? client;

        /// <summary>
        /// 客户端事件循环组（管理 I/O 线程）
        /// </summary>
        private MultithreadEventLoopGroup? ClientGroup;

        /// <summary>
        /// 与服务端的通信通道
        /// </summary>
        private IChannel? Channel;

        /// <summary>
        /// 已创建的接口代理缓存（按接口名索引）
        /// </summary>
        private ConcurrentDictionary<string, object> creates { get; } = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 接口注册表（接口名称 -> 实现类型）
        /// </summary>
        private Dictionary<string, System.Type> iRegister { get; set; } = new Dictionary<string, System.Type>();

        /// <summary>
        /// 异步等待管理器（管理请求/响应的同步等待）
        /// </summary>
        private Await Await = new Await();

        /// <summary>
        /// 身份认证标识（用于匹配认证响应）
        /// </summary>
        private string AuthenticationTag = Guid.NewGuid().ToUpperNString();

        /// <summary>
        /// 打开 RPC 客户端连接<br/>
        /// 创建 TCP 连接并发送身份认证请求，等待服务端认证响应
        /// </summary>
        /// <returns>操作结果</returns>
        public OperateResult Open()
        {
            BegOperate();
            try
            {
                if (client != null)
                {
                    return EndOperate(false, "已打开");
                }
                client = new Bootstrap()
               .Group(ClientGroup = new MultithreadEventLoopGroup())
               .Channel<TcpSocketChannel>()
               .Option(ChannelOption.TcpNodelay, true)
               .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
               {
                   IChannelPipeline pipeline = channel.Pipeline;
                   pipeline.AddLast("framing-enc", new LengthFieldPrepender(8));
                   pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 8, 0, 8));
                   pipeline.AddLast(new RpcClientHandler(this));
               }));
                Channel = client.ConnectAsync(new IPEndPoint(IPAddress.Parse(basics.IpAddress), basics.Port)).ConfigureAwait(false).GetAwaiter().GetResult();
                //发送身份认证
                Await.Start(AuthenticationTag);

                Channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Authentication()
                {
                    UserName = basics.UserName,
                    Password = basics.Password,
                    ISn = basics.ISn,
                    INs = basics.INs
                }))));
                //消息转换转换
                Message? response = JsonConvert.DeserializeObject<Message>(Await.Wait(AuthenticationTag).resultData);
                if (response == null)
                {
                    Close(true);
                    return EndOperate(false, "服务器未响应认证信息");
                }
                return EndOperate(response.State, response.Info);
            }
            catch (Exception ex)
            {
                Close(true);
                return EndOperate(false, ex.Message, ex);
            }
        }
        /// <summary>
        /// 关闭 RPC 客户端连接<br/>
        /// 优雅关闭事件循环组并释放通道资源
        /// </summary>
        /// <param name="HardClose">是否强制关闭（跳过状态检查）</param>
        /// <returns>操作结果</returns>
        public OperateResult Close(bool HardClose = false)
        {
            BegOperate();
            try
            {
                if (!HardClose)
                {
                    if (client == null)
                    {
                        return EndOperate(false, "未打开");
                    }
                }
                ClientGroup?.ShutdownGracefullyAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                Channel?.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                Channel?.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                client = null;

                return EndOperate(true);
            }
            catch (Exception ex)
            {
                return EndOperate(false, ex.Message, ex);
            }
        }
        /// <summary>
        /// 创建接口代理对象<br/>
        /// 通过动态代理透明调用远程服务方法，自动缓存已创建的代理
        /// </summary>
        /// <typeparam name="T">目标接口类型</typeparam>
        /// <returns>接口代理实例</returns>
        public T Create<T>() where T : class
        {
            T cre = null;
            //接口名称
            string iname = typeof(T).Name;

            //不存在则添加
            if (creates.ContainsKey(iname))
            {
                cre = (T)creates[iname];
            }
            else
            {
                //代理
                Proxy proxy = Proxy.Instance(new ProxyData.Basics
                {
                    Main = this,
                    Await = Await,
                    channel = Channel,
                    iName = iname,
                    type = typeof(T)
                });
                cre = proxy.ActLike<T>();
                //添加
                creates.AddOrUpdate(iname, cre, (k, v) => cre);
            }
            return cre;
        }
        /// <summary>
        /// 注册接口与实现类的映射关系<br/>
        /// 用于服务端请求时的反射调用
        /// </summary>
        /// <typeparam name="I">接口类型</typeparam>
        /// <typeparam name="O">实现类型</typeparam>
        /// <returns>操作结果</returns>
        public OperateResult Register<I, O>()
        {
            BegOperate();
            try
            {
                iRegister.Add(typeof(I).Name, typeof(O));
                return EndOperate(true);
            }
            catch (Exception ex)
            {
                return EndOperate(false, ex.Message, ex);
            }
        }
        /// <summary>
        /// 处理服务端响应数据<br/>
        /// 根据消息类型分发：请求、响应、认证、消息
        /// </summary>
        /// <param name="data">接收到的字节缓冲区</param>
        /// <param name="channel">通信通道</param>
        public void Response(IByteBuffer data, IChannel channel)
        {
            //实例化返回信息
            Response message = new Response();

            try
            {
                Types TAG = data.ToString(Encoding.UTF8).ToJsonEntity<Snet.Rpc.data.Type>().TAG;
                switch (TAG)
                {
                    case Types.request:
                        //地址
                        string address = Tool.IpHandle(channel.RemoteAddress);
                        //请求的数据
                        Request? request = data.ToString(Encoding.UTF8).ToJsonEntity<Request>();
                        //判断接口名是否在字典中
                        if (!iRegister.ContainsKey(request.IName))
                        {
                            message.info = "接口不存在";
                            //发送消息
                            channel.WriteAndFlushAsync(message);
                            //跳出
                            return;
                        }
                        //取出这个程序集
                        System.Type iType = iRegister[request.IName];
                        //创建实例
                        object? instance = Activator.CreateInstance(iType);
                        //获取方法
                        MethodInfo? method = iType.GetMethod(request.MName);
                        //判断方法是否为空
                        if (method == null)
                        {
                            message.info = "未找到该方法";
                            channel.WriteAndFlushAsync(message);
                            //跳出
                            return;
                        }
                        //参数信息
                        object[] paramters = request.Params.ToArray();
                        //获取方法参数
                        var methodParamters = method.GetParameters();
                        //参数转换赋值
                        for (int i = 0; i < methodParamters.Length; i++)
                        {
                            if (paramters[i].GetType() != methodParamters[i].ParameterType)
                            {
                                paramters[i] = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(paramters[i]), methodParamters[i].ParameterType);
                            }
                        }
                        //执行此方法
                        object? res = method.Invoke(instance, paramters);

                        //返回响应信息
                        message.info = "请求成功";
                        message.status = true;
                        message.data = res;
                        //发送消息
                        channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message))));
                        break;
                    case Types.response:
                        Await.Set(channel.Id.AsShortText(), data.ToString(Encoding.UTF8));
                        break;
                    case Types.authentication:
                        break;
                    case Types.message:
                        Await.Set(AuthenticationTag, data.ToString(Encoding.UTF8));
                        break;
                }
            }
            catch (Exception ex)
            {
                //返回响应信息
                message.info = ex.Message;
                //发送消息
                channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message))));
            }
        }
        /// <summary>
        /// 处理异常信息，招出事件并释放资源
        /// </summary>
        /// <param name="ex">异常对象</param>
        public void Exception(Exception ex)
        {
            OnInfoEventHandler(this, new EventInfoResult(false, ex.Message));
            Dispose();
        }
        /// <inheritdoc/>
        public override void Dispose()
        {
            Close();
            base.Dispose();
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            Close();
            await base.DisposeAsync();
        }
    }
}
