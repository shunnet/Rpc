namespace Snet.Rpc.data
{
    /// <summary>
    /// RPC 请求数据模型，封装远程方法调用的接口名称、方法名称和参数列表。
    /// <para>由客户端动态代理 <see cref="Snet.Rpc.unility.Proxy"/> 构建，通过 DotNetty 通道发送到远端。</para>
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
