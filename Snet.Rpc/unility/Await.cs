using Snet.Rpc.data;
using System.Collections.Concurrent;

namespace Snet.Rpc.unility
{
    /// <summary>
    /// 等待
    /// </summary>
    public class Await
    {
        /// <summary>
        /// 容器
        /// </summary>
        private ConcurrentDictionary<string, AwaitData> WaitIoc { get; set; } = new ConcurrentDictionary<string, AwaitData>();
        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="tag">标识</param>
        public void Start(string tag)
        {
            if (!WaitIoc.ContainsKey(tag))
            {
                AwaitData awaitData = new AwaitData();
                WaitIoc.AddOrUpdate(tag, awaitData, (k, v) => awaitData);
            }
        }
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="rData">返回的数据</param>
        /// <exception cref="Exception"></exception>
        public void Set(string tag, string rData)
        {
            if (WaitIoc.ContainsKey(tag))
            {
                AwaitData awaitData = WaitIoc[tag];
                awaitData.resultData = rData;
                awaitData.WaitHandler.Set();
            }
            else
            {
                throw new Exception($"TAG({tag})不存在");
            }
        }
        /// <summary>
        /// 等待
        /// </summary>
        /// <param name="tag">标识</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AwaitData Wait(string tag)
        {
            if (WaitIoc.ContainsKey(tag))
            {
                AwaitData awaitData = WaitIoc[tag];
                awaitData.WaitHandler.WaitOne();
                WaitIoc.TryRemove(tag, out _);
                return awaitData;
            }
            else
            {
                throw new Exception($"TAG({tag})不存在");
            }
        }
    }
}
