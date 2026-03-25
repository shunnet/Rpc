using Snet.Utility;

namespace Snet.Rpc.data
{
    /// <summary>
    /// RPC 响应数据模型，封装远程方法调用的返回结果（状态、信息、数据及时间戳）。
    /// <para>由远端处理请求后构建，通过 DotNetty 通道返回给调用方。</para>
    /// </summary>
    public class Response
    {
        public Response() { }
        public Response(string info, bool status = false, object? data = null)
        {
            this.info = info;
            this.status = status;
            this.data = data;
        }
        /// <summary>
        /// 标识符
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Types TAG = Types.response;

        /// <summary>
        /// info
        /// </summary>
        public string info { get; set; }

        /// <summary>
        /// 成功还是失败
        /// </summary>
        public bool status { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object? data { get; set; }

        /// <summary>
        /// 发送端时间
        /// </summary>
        public string time { get; set; } = DateTime.Now.ToDateTimeString();
    }
}
