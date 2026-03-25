using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Snet.Rpc.service
{
    /// <summary>
    /// RPC 服务端通道处理器，继承自 <see cref="ChannelHandlerAdapter"/>，负责处理 DotNetty 通道的读取、异常等事件。
    /// <para>接收到数据时调用 <see cref="RpcService.Response"/> 处理请求，异常时调用 <see cref="RpcService.Exception"/> 通知上层。</para>
    /// </summary>
    public class RpcServiceHandler : ChannelHandlerAdapter
    {
        /// <summary>
        /// 初始化 RPC 服务端通道处理器。
        /// </summary>
        /// <param name="rPCServer">关联的 RPC 服务端实例</param>
        public RpcServiceHandler(RpcService rPCServer)
        {
            rpcService = rPCServer;
        }

        /// <summary>
        /// 关联的 RPC 服务端实例
        /// </summary>
        private RpcService rpcService { get; }

        /// <summary>
        /// 通道读取事件，将接收到的字节缓冲区转发给 <see cref="RpcService.Response"/> 进行处理。
        /// </summary>
        /// <param name="context">通道上下文</param>
        /// <param name="message">接收到的消息（<see cref="IByteBuffer"/>）</param>
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            rpcService.Response(message as IByteBuffer, context.Channel);
        }

        /// <summary>
        /// 通道读取完成事件，刷新输出缓冲区。
        /// </summary>
        /// <param name="context">通道上下文</param>
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        /// <summary>
        /// 通道异常捕获事件，通知 RPC 服务端并关闭通道。
        /// </summary>
        /// <param name="context">通道上下文</param>
        /// <param name="exception">捕获到的异常</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            rpcService.Exception(exception);
            context.CloseAsync();
        }
    }
}
