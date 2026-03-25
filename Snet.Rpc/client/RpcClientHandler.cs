using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Snet.Rpc.client
{
    /// <summary>
    /// RPC 客户端通道处理器，继承自 <see cref="ChannelHandlerAdapter"/>，负责处理 DotNetty 通道的读取、异常等事件。
    /// <para>接收到数据时调用 <see cref="RpcClient.Response"/> 处理响应，异常时调用 <see cref="RpcClient.Exception"/> 通知上层。</para>
    /// </summary>
    public class RpcClientHandler : ChannelHandlerAdapter
    {
        /// <summary>
        /// 关联的 RPC 客户端实例
        /// </summary>
        private readonly RpcClient rpcClient;

        /// <summary>
        /// 初始化 RPC 客户端通道处理器。
        /// </summary>
        /// <param name="rpcClient">关联的 RPC 客户端实例</param>
        public RpcClientHandler(RpcClient rpcClient)
        {
            this.rpcClient = rpcClient;
        }

        /// <summary>
        /// 通道读取事件，将接收到的字节缓冲区转发给 <see cref="RpcClient.Response"/> 进行处理。
        /// </summary>
        /// <param name="context">通道上下文</param>
        /// <param name="message">接收到的消息（<see cref="IByteBuffer"/>）</param>
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            rpcClient.Response(message as IByteBuffer, context.Channel);
        }

        /// <summary>
        /// 通道读取完成事件，刷新输出缓冲区。
        /// </summary>
        /// <param name="context">通道上下文</param>
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        /// <summary>
        /// 通道异常捕获事件，通知 RPC 客户端并关闭通道。
        /// </summary>
        /// <param name="context">通道上下文</param>
        /// <param name="exception">捕获到的异常</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            rpcClient.Exception(exception);
            context.CloseAsync();
        }
    }
}
