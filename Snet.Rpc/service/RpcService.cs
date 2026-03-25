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
using System.Reflection;
using System.Text;
using static Snet.Rpc.data.Service;
namespace Snet.Rpc.service
{
    /// <summary>
    /// RPC 服务端<br/>
    /// <br/>
    /// 功能：<br/>
    /// - 基于 DotNetty 实现高性能 TCP RPC 服务<br/>
    /// - 支持客户端身份认证（账号密码 + 唯一标识 + 接口校验）<br/>
    /// - 支持接口注册、反射调用和代理创建<br/>
    /// - 多客户端并发管理
    /// </summary>
    public class RpcService : CoreUnify<RpcService, Basics>, IRpc
    {
        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="basics">基础数据（包含端口、账号密码、客户端信息等）</param>
        public RpcService(Basics basics) : base(basics) { }

        /// <summary>
        /// DotNetty ServerBootstrap 服务端实例
        /// </summary>
        private ServerBootstrap? server;

        /// <summary>
        /// 连接接受事件循环组（Boss Group）
        /// </summary>
        private MultithreadEventLoopGroup? ClientGroup;

        /// <summary>
        /// 消息处理事件循环组（Worker Group）
        /// </summary>
        private MultithreadEventLoopGroup? ClientMessageGroup;

        /// <summary>
        /// 服务端监听通道
        /// </summary>
        private IChannel? Channel;

        /// <summary>
        /// 接口注册表（接口名称 -> 实现类型）
        /// </summary>
        private Dictionary<string, System.Type> iRegister { get; set; } = new Dictionary<string, System.Type>();

        /// <summary>
        /// 已创建的接口代理缓存（按接口名索引）
        /// </summary>
        private ConcurrentDictionary<string, object> creates { get; } = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 已认证的客户端信息（接口名称列表 -> 通道）
        /// </summary>
        public ConcurrentDictionary<List<Details>, IChannel> clients = new ConcurrentDictionary<List<Details>, IChannel>();

        /// <summary>
        /// 异步等待管理器（管理请求/响应的同步等待）
        /// </summary>
        private Await Await = new Await();
        /// <summary>
        /// 打开 RPC 服务端<br/>
        /// 创建 ServerBootstrap 并绑定监听端口
        /// </summary>
        /// <returns>操作结果</returns>
        public OperateResult Open()
        {
            BegOperate();
            try
            {
                if (server != null)
                {
                    return EndOperate(false, "已打开");
                }

                //实例化服务
                server = new ServerBootstrap()
                    .Group(ClientGroup = new MultithreadEventLoopGroup(), ClientMessageGroup = new MultithreadEventLoopGroup())
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(8));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 8, 0, 8));
                        pipeline.AddLast(new RpcServiceHandler(this));
                    }));

                Channel = server.BindAsync(basics.Port).ConfigureAwait(false).GetAwaiter().GetResult();

                return EndOperate(true);

            }
            catch (Exception ex)
            {
                Close(true);
                return EndOperate(false, ex.Message, ex);
            }
        }
        /// <summary>
        /// 关闭 RPC 服务端<br/>
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
                    if (server == null)
                    {
                        return EndOperate(false, "未打开");
                    }
                }

                ClientGroup?.ShutdownGracefullyAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                ClientMessageGroup?.ShutdownGracefullyAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                Channel?.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                Channel?.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                server = null;

                return EndOperate(true);
            }
            catch (Exception ex)
            {
                return EndOperate(false, ex.Message, ex);
            }
        }
        /// <summary>
        /// 创建接口代理对象<br/>
        /// 通过客户端认证信息检索对应通道，创建动态代理
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
                //通过接口名检索到使用哪个通道
                IChannel channel = clients.FirstOrDefault(c => c.Key.Contains(c.Key.FirstOrDefault(x => x.INames.Equals(typeof(T).Name)))).Value;

                Proxy proxy = Proxy.Instance(new ProxyData.Basics
                {
                    Main = this,
                    Await = Await,
                    channel = channel,
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
        /// 用于客户端请求时的反射调用
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
        /// 处理客户端请求数据<br/>
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
                        Authentication? authentication = data.ToString(Encoding.UTF8).ToJsonEntity<Authentication>();
                        if (basics.UserName == authentication.UserName && basics.Password == authentication.Password)
                        {
                            if (basics.Infos.Select(c => c.ISn == authentication.ISn).First())
                            {
                                if (basics.Infos.Select(c => c.INs.Comparer(authentication.INs).result).First())
                                {

                                    //认证成功
                                    channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message()
                                    {
                                        Info = "认证成功",
                                        State = true,
                                    }) ?? string.Empty)));

                                    //添加客户端
                                    clients.AddOrUpdate(basics.Infos.FirstOrDefault(c => c.ISn == authentication.ISn && c.INs.Comparer(authentication.INs).result).INs, channel, (k, v) => channel);
                                }
                                else
                                {
                                    //认证失败
                                    channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message()
                                    {
                                        Info = "认证失败，接口信息不一致",
                                        State = false,
                                    }))));
                                    //关闭客户端连接
                                    channel.CloseAsync();
                                }
                            }
                            else
                            {
                                //认证失败
                                channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message()
                                {
                                    Info = "认证失败，此唯一标识不存在",
                                    State = false,
                                }))));
                                //关闭客户端连接
                                channel.CloseAsync();
                            }
                        }
                        else
                        {
                            //认证失败
                            channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message()
                            {
                                Info = "认证失败，账号密码错误",
                                State = false,
                            }))));
                            //关闭客户端连接
                            channel.CloseAsync();
                        }
                        break;
                    case Types.message:

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
        /// 处理异常信息，招出异常事件
        /// </summary>
        /// <param name="ex">异常对象</param>
        public void Exception(Exception ex)
        {
            OnInfoEventHandler(this, new EventInfoResult(false, ex.Message));
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
