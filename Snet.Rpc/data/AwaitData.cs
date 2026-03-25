namespace Snet.Rpc.data
{
    /// <summary>
    /// RPC 等待传递数据模型，封装 <see cref="AutoResetEvent"/> 同步信号和响应结果字符串，用于请求-响应的同步等待机制。
    /// </summary>
    public class AwaitData
    {
        /// <summary>
        /// 等待处理
        /// </summary>
        public AutoResetEvent WaitHandler { get; set; } = new AutoResetEvent(false);
        /// <summary>
        /// 返回的数据
        /// </summary>
        public string resultData { get; set; }
    }
}
