using System.Text.Json.Serialization;

namespace Snet.Rpc.data
{
    /// <summary>
    /// RPC 消息类型基类，所有 RPC 数据模型（请求、响应、认证、消息）均包含 <see cref="Types"/> TAG 标识，用于反序列化时区分消息类型。
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
