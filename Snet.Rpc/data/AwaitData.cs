namespace Snet.Rpc.data
{
    /// <summary>
    /// 等待传递数据
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
