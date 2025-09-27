using System.Net;

namespace Snet.Rpc.unility
{
    public class Tool
    {
        /// <summary>
        /// IP处理
        /// </summary>
        /// <param name="Ip"></param>
        /// <returns></returns>
        public static string IpHandle(EndPoint Ip)
        {
            //原始数据
            string ip = string.Empty;
            string? ip_od = Ip.ToString();
            if (!string.IsNullOrWhiteSpace(ip_od))
            {
                string[] str = ip_od.Replace("[", "").Replace("]", "").Split(':');
                ip = $"{str[3]}:{str[4]}";
            }
            return ip;
        }
    }
}
