using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Snet.Rpc.service
{
    public class RpcServiceHandler : ChannelHandlerAdapter
    {
        public RpcServiceHandler(RpcService rPCServer)
        {
            rpcService = rPCServer;
        }

        RpcService rpcService { get; }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            rpcService.Response(message as IByteBuffer, context.Channel);
        }
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            rpcService.Exception(exception);
            context.CloseAsync();
        }
    }
}
