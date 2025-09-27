using System.ComponentModel;

namespace Snet.Rpc.data
{
    public class Service
    {
        /// <summary>
        /// 基础数据
        /// </summary>
        public class Basics
        {
            /// <summary>
            /// 端口
            /// </summary>
            [Category("基础数据")]
            [Description("端口")]
            public int Port { get; set; } = 6688;

            /// <summary>
            /// 超时时间
            /// </summary>
            [Description("超时时间")]
            public int TimeOut { get; set; } = 1000;

            /// <summary>
            /// 用户名
            /// </summary>
            [Description("用户名")]
            public string UserName { get; set; } = "snet";
            /// <summary>
            /// 密码
            /// </summary>
            [Description("密码")]
            public string Password { get; set; } = "snet";

            /// <summary>
            /// 信息
            /// </summary>
            [Description("信息")]
            public List<Info> Infos { get; set; } = new List<Info>();
        }
        /// <summary>
        /// 用户信息
        /// </summary>
        public class Info
        {
            /// <summary>
            /// 唯一标识
            /// </summary>
            [Category("基础数据")]
            [Description("唯一标识")]
            public string ISn { get; set; } = "888888";
            /// <summary>
            /// 接口名称集合
            /// </summary>
            [Description("接口名称集合")]
            public List<Details> INs { get; set; } = new List<Details>();
        }
    }
}
