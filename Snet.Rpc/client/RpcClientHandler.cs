using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Snet.Rpc.client
{
    public class RpcClientHandler : ChannelHandlerAdapter
    {
        RpcClient rpcClient;
        public RpcClientHandler(RpcClient rpcClient)
        {
            this.rpcClient = rpcClient;
        }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            rpcClient.Response(message as IByteBuffer, context.Channel);
        }
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            rpcClient.Exception(exception);
            context.CloseAsync();
        }
    }
}
