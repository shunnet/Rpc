using Snet.Utility;

namespace Snet.Rpc.data
{
    /// <summary>
    /// 响应
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
