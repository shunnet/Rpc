namespace Snet.Rpc.data
{
    /// <summary>
    /// RPC 身份认证数据模型，客户端连接时发送给服务端进行身份验证。
    /// <para>包含用户名、密码、唯一标识 (ISn) 和可提供的接口列表 (INs)。</para>
    /// </summary>
    public class Authentication
    {
        /// <summary>
        /// 标识
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Types TAG = Types.authentication;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 唯一标识
        /// </summary>
        public string ISn { get; set; }

        /// <summary>
        /// 接口名称集合
        /// </summary>
        public List<Details> INs { get; set; }
    }
}
