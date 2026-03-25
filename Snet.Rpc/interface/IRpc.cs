using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Snet.Model.data;

namespace Snet.Rpc.@interface
{
    /// <summary>
    /// RPC 通信接口<br/>
    /// 定义客户端和服务端的公共操作契约
    /// </summary>
    public interface IRpc : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// 注册接口与实现类的映射关系
        /// </summary>
        /// <typeparam name="I">接口类型</typeparam>
        /// <typeparam name="O">实现类型</typeparam>
        /// <returns>操作结果</returns>
        OperateResult Register<I, O>();

        /// <summary>
        /// 创建接口代理对象，用于透明调用远程服务方法
        /// </summary>
        /// <typeparam name="T">目标接口类型</typeparam>
        /// <returns>接口代理实例</returns>
        T Create<T>() where T : class;

        /// <summary>
        /// 打开 RPC 连接
        /// </summary>
        /// <returns>操作结果</returns>
        OperateResult Open();

        /// <summary>
        /// 关闭 RPC 连接
        /// </summary>
        /// <param name="HardClose">是否强制关闭</param>
        /// <returns>操作结果</returns>
        OperateResult Close(bool HardClose = false);

        /// <summary>
        /// 处理接收到的数据响应
        /// </summary>
        /// <param name="data">字节缓冲区数据</param>
        /// <param name="channel">通信通道</param>
        void Response(IByteBuffer data, IChannel channel);

        /// <summary>
        /// 处理异常信息
        /// </summary>
        /// <param name="ex">异常对象</param>
        void Exception(Exception ex);
    }
}
