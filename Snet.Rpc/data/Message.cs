using Snet.Utility;

namespace Snet.Rpc.data
{
    public class Message
    {
        /// <summary>
        /// 标识
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Types TAG = Types.message;
        /// <summary>
        /// 信息
        /// </summary>
        public string Info { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; } = false;
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { get; set; } = DateTime.Now.ToDateTimeString();
    }
}
