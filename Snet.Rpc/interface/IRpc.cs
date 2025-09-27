using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Snet.Model.data;

namespace Snet.Rpc.@interface
{
    /// <summary>
    /// RPC接口
    /// </summary>
    public interface IRpc : IDisposable
    {
        /// <summary>
        /// 注册
        /// </summary>
        /// <typeparam name="I">接口</typeparam>
        /// <typeparam name="O">对象</typeparam>
        /// <returns></returns>
        OperateResult Register<I, O>();

        /// <summary>
        /// 创建
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <returns></returns>
        T Create<T>() where T : class;

        /// <summary>
        /// 打开
        /// </summary>
        /// <returns>统一返回</returns>
        OperateResult Open();

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns>统一返回</returns>
        OperateResult Close(bool HardClose = false);

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="data">字节数据</param>
        /// <param name="channel">通道</param>
        void Response(IByteBuffer data, IChannel channel);

        /// <summary>
        /// 异常信息
        /// </summary>
        /// <param name="ex">异常</param>
        void Exception(Exception ex);
    }
}
