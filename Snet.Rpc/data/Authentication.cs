namespace Snet.Rpc.data
{
    /// <summary>
    /// 身份认证
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
