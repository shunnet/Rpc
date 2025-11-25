using System.Text.Json.Serialization;

namespace Snet.Rpc.data
{
    /// <summary>
    /// 类型
    /// </summary>
    public class Type
    {
        /// <summary>
        /// 标识
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Types TAG { get; set; }
    }
    /// <summary>
    /// 类型枚举
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Types
    {
        /// <summary>
        /// 请求
        /// </summary>
        request,
        /// <summary>
        /// 响应
        /// </summary>
        response,
        /// <summary>
        /// 身份认证
        /// </summary>
        authentication,
        /// <summary>
        /// 消息
        /// </summary>
        message
    }
}
