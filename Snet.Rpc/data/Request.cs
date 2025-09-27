namespace Snet.Rpc.data
{
    /// <summary>
    /// 请求的数据类型
    /// </summary>
    public class Request
    {
        /// <summary>
        /// 标识
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Types TAG = Types.request;
        /// <summary>
        /// 接口名称
        /// </summary>
        public string IName { get; set; }
        /// <summary>
        /// 方法名称
        /// </summary>
        public string MName { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public List<object> Params { get; set; }
    }
}
