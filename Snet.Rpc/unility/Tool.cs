using System.Net;

namespace Snet.Rpc.unility
{
    /// <summary>
    /// RPC 工具类，提供网络地址解析等辅助方法。
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// 从 <see cref="EndPoint"/> 中提取 IPv4 地址和端口号。
        /// <para>将 IPv6 映射格式（如 [::ffff:127.0.0.1]:8080）解析为 IPv4:Port 格式。</para>
        /// </summary>
        /// <param name="Ip">网络端点对象</param>
        /// <returns>解析后的 IP:Port 字符串，解析失败时返回空字符串</returns>
        public static string IpHandle(EndPoint Ip)
        {
            //原始数据
            string ip = string.Empty;
            string? ip_od = Ip.ToString();
            if (!string.IsNullOrWhiteSpace(ip_od))
            {
                string[] str = ip_od.Replace("[", "").Replace("]", "").Split(':');
                if (str.Length >= 5)
                {
                    ip = $"{str[3]}:{str[4]}";
                }
            }
            return ip;
        }
    }
}
