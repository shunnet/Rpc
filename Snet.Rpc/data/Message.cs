using Snet.Utility;

namespace Snet.Rpc.data
{
    /// <summary>
    /// RPC 消息数据模型，用于在客户端与服务端之间传递通知性消息（如连接状态变更、认证结果等）。
    /// </summary>
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
