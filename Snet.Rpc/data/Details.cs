using System.ComponentModel;

namespace Snet.Rpc.data
{
    /// <summary>
    /// 详情
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
