using DotNetty.Buffers;
using Newtonsoft.Json;
using Snet.Rpc.client;
using Snet.Rpc.data;
using Snet.Rpc.service;
using Snet.Utility;
using System.Dynamic;
using System.Text;

namespace Snet.Rpc.unility
{

    /// <summary>
    /// RPC 动态代理类，继承自 <see cref="DynamicObject"/>，用于拦截接口方法调用并通过 DotNetty 通道发送 RPC 请求。
    /// <para>采用单例模式管理代理实例，每个 <see cref="ProxyData.Basics"/> 对应一个代理对象。</para>
    /// <para>调用流程：拦截方法调用 → 序列化为 JSON 请求 → 通过 DotNetty 通道发送 → 等待响应 → 反序列化返回值。</para>
    /// </summary>
    public class Proxy : DynamicObject
    {
        /// <summary>
        /// 锁
        /// </summary>
        private static readonly object Lock = new object();

        /// <summary>
        /// 自身对象集合
        /// </summary>
        private static List<Proxy> ThisObjList = new List<Proxy>();

        /// <summary>
        /// 获取或创建代理实例（线程安全的双重检查锁定单例模式）。
        /// <para>根据 <see cref="ProxyData.Basics"/> 的比较结果复用已有实例，避免重复创建。</para>
        /// </summary>
        /// <param name="basics">代理所需的基础数据（通道、接口名称、类型等）</param>
        /// <returns>匹配的代理实例</returns>
        public static Proxy Instance(ProxyData.Basics basics)
        {
            Proxy? exp = ThisObjList.FirstOrDefault(c => c.basics.Comparer(basics).result);
            if (exp == null)
            {
                lock (Lock)
                {
                    if (ThisObjList.Count(c => c.basics.Comparer(basics).result) > 0)
                    {
                        return ThisObjList.First(c => c.basics.Comparer(basics).result);
                    }
                    else
                    {
                        Proxy exp2 = new Proxy(basics);
                        ThisObjList.Add(exp2);
                        return exp2;
                    }
                }
            }
            return exp;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="basics">基础数据</param>
        public Proxy(ProxyData.Basics basics)
        {
            this.basics = basics;
        }

        /// <summary>
        /// 基础数据
        /// </summary>
        private ProxyData.Basics basics;

        /// <summary>
        /// 拦截动态方法调用，将其序列化为 RPC 请求并通过 DotNetty 通道发送到远端。
        /// <para>流程：构建请求 → JSON 序列化 → 发送 → 阻塞等待响应 → 反序列化返回值。</para>
        /// <para>超时或异常时通过 <see cref="RpcClient.Exception"/> 或 <see cref="RpcService.Exception"/> 回调通知。</para>
        /// </summary>
        /// <param name="binder">方法调用绑定器，包含方法名称等信息</param>
        /// <param name="args">方法调用参数</param>
        /// <param name="result">方法返回值（反序列化后的对象）</param>
        /// <returns>调用成功返回 true，失败或异常返回 false</returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            try
            {
                //标识
                string tag = basics.channel.Id.AsShortText();
                //启动等待
                basics.Await.Start(tag);

                //设置参数
                data.Request request = new data.Request
                {
                    IName = basics.iName,
                    MName = binder.Name,
                    Params = args.ToList()
                };

                //转换成字节
                IByteBuffer sendBuffer = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));
                //发送（同步等待完成，确保数据发送后再等待响应）
                basics.channel.WriteAndFlushAsync(sendBuffer).GetAwaiter().GetResult();
                //响应的字符串
                string res = basics.Await.Wait(tag).resultData;
                //转换
                Response response = JsonConvert.DeserializeObject<Response>(res);
                if (response == null)
                {
                    if (basics.Main is RpcClient rpcClient)
                    {
                        rpcClient?.Exception(new Exception("超时未响应"));
                    }
                    else if (basics.Main is RpcService rpcService)
                    {
                        rpcService?.Exception(new Exception("超时未响应"));
                    }
                }
                else if (response.status)
                {
                    System.Type? returnType = basics.type?.GetMethod(binder.Name)?.ReturnType;
                    if (returnType != typeof(void))
                    {
                        result = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(response.data), returnType);
                    }
                    return true;
                }
                else
                {
                    if (basics.Main is RpcClient rpcClient)
                    {
                        rpcClient?.Exception(new Exception($"异常错误消息：{response.info}"));
                    }
                    else if (basics.Main is RpcService rpcService)
                    {
                        rpcService?.Exception(new Exception($"异常错误消息：{response.info}"));
                    }
                }
            }
            catch (Exception ex)
            {
                if (basics.Main is RpcClient rpcClient)
                {
                    rpcClient?.Exception(ex);
                }
                else if (basics.Main is RpcService rpcService)
                {
                    rpcService?.Exception(ex);
                }
            }
            return false;
        }
    }
}
