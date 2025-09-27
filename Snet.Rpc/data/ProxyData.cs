using DotNetty.Transport.Channels;
using Snet.Rpc.unility;

namespace Snet.Rpc.data
{
    /// <summary>
    /// 代理数据
    /// </summary>
    public class ProxyData
    {
        /// <summary>
        /// 基础数据
        /// </summary>
        public class Basics
        {
            /// <summary>
            /// 控制端对象
            /// </summary>
            public object Main { get; set; }
            /// <summary>
            /// 通道
            /// </summary>
            public IChannel channel { get; set; }
            /// <summary>
            /// 接口名称
            /// </summary>
            public string iName { get; set; }
            /// <summary>
            /// 结构类型
            /// </summary>
            public System.Type type { get; set; }
            /// <summary>
            /// 等待任务
            /// </summary>
            public Await Await { get; set; }
        }
    }
}
