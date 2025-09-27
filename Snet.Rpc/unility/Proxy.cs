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
    /// 动作代理
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
        /// 单例模式
        /// </summary>
        /// <returns></returns>
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
        /// 调用方法
        /// </summary>
        /// <returns>状态</returns>
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
                //发送
                basics.channel.WriteAndFlushAsync(sendBuffer);
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
