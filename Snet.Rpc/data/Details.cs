using System.ComponentModel;

namespace Snet.Rpc.data
{
    /// <summary>
    /// RPC 接口详情数据模型，用于身份认证时描述客户端可提供的接口信息。
    /// </summary>
    public class Details
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        [Category("基础数据")]
        [Description("接口名称")]
        public string INames { get; set; } = "Interface Name";
    }
}
