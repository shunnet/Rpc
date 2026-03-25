using Snet.Rpc.data;
using System.Collections.Concurrent;

namespace Snet.Rpc.unility
{
    /// <summary>
    /// RPC 异步等待管理器，基于 <see cref="ConcurrentDictionary{TKey, TValue}"/> 和 <see cref="AutoResetEvent"/> 实现请求-响应的同步等待机制。
    /// <para>流程：Start(注册等待) → 发送请求 → Wait(阻塞等待) → Set(远端响应到达后唤醒)。</para>
    /// </summary>
    public class Await
    {
        /// <summary>
        /// 容器
        /// </summary>
        private ConcurrentDictionary<string, AwaitData> WaitIoc { get; set; } = new ConcurrentDictionary<string, AwaitData>();
        /// <summary>
        /// 注册一个新的等待标识，若该标识尚不存在则创建 <see cref="AwaitData"/> 实例并添加到容器中。
        /// </summary>
        /// <param name="tag">等待标识（通常为通道 ID）</param>
        public void Start(string tag)
        {
            if (!WaitIoc.ContainsKey(tag))
            {
                AwaitData awaitData = new AwaitData();
                WaitIoc.AddOrUpdate(tag, awaitData, (k, v) => awaitData);
            }
        }
        /// <summary>
        /// 设置指定标识的等待结果并唤醒等待线程。
        /// <para>当远端响应到达后调用此方法，将结果数据写入 <see cref="AwaitData.resultData"/> 并通过 <see cref="AutoResetEvent.Set"/> 唤醒等待线程。</para>
        /// </summary>
        /// <param name="tag">等待标识（通常为通道 ID）</param>
        /// <param name="rData">远端返回的 JSON 响应数据</param>
        /// <exception cref="Exception">当指定标识不存在时抛出</exception>
        public void Set(string tag, string rData)
        {
            if (WaitIoc.TryGetValue(tag, out AwaitData? awaitData))
            {
                awaitData.resultData = rData;
                awaitData.WaitHandler.Set();
            }
            else
            {
                throw new Exception($"TAG({tag})不存在");
            }
        }
        /// <summary>
        /// 阻塞等待指定标识的响应数据，等待完成后自动从容器中移除该标识。
        /// <para>通过 <see cref="AutoResetEvent.WaitOne()"/> 阻塞当前线程，直到 <see cref="Set"/> 方法被调用。</para>
        /// </summary>
        /// <param name="tag">等待标识（通常为通道 ID）</param>
        /// <returns>包含响应数据的 <see cref="AwaitData"/> 对象</returns>
        /// <exception cref="Exception">当指定标识不存在时抛出</exception>
        public AwaitData Wait(string tag)
        {
            if (WaitIoc.TryGetValue(tag, out AwaitData? awaitData))
            {
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
