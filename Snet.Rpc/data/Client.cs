using System.ComponentModel;

namespace Snet.Rpc.data
{
    public class Client
    {
        /// <summary>
        /// 基础数据
        /// </summary>
        public class Basics
        {
            /// <summary>
            /// Ip地址
            /// </summary>
            [Category("基础数据")]
            [Description("Ip地址")]
            public string IpAddress { get; set; } = "127.0.0.1";
            /// <summary>
            /// 端口
            /// </summary>
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
            public string UserName { get; set; } = "ysai";
            /// <summary>
            /// 密码
            /// </summary>
            [Description("密码")]
            public string Password { get; set; } = "ysai";
            /// <summary>
            /// 唯一标识
            /// </summary>
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
