using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
namespace DiskAPIBase
{
    public class IEProxyInfo
    {
        const string regexp = @"(/w+)=([/w/.]+):(/d+);?";
        bool _enabled;
        string _override;
        string _server;
        internal IEProxyInfo()
        {
            _enabled = false;
            _override = null;
            _server = null;
        }
        // 注册表位置:
        //HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings
        // 关心的子键:
        // 键              数据类型     取值和示例
        //ProxyEnable,   REG_DWORD, 1 开启, 0 关闭
        //ProxyOverride  REG_SZ,    不用代理的站点, 如: 192.168.0.*;<local>
        //ProxyServer    REG_SZ,    各类代理服务器, 如: ftp=192.168.0.1:80;http=192.168.0.1:80;https=192.168.0.1:80;socks=192.168.0.1:1080
        public static IEProxyInfo GetIEProxyInfo()
        {
            object v = null;
            int dummy = 0;
            IEProxyInfo proxyInfo = new IEProxyInfo();
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            v = key.GetValue("ProxyEnable");
            if (v != null && int.TryParse(v.ToString(), out dummy))
                proxyInfo.Enabled = (dummy == 1);
            v = key.GetValue("ProxyOverride");
            if (v != null && v is string)
                proxyInfo.Override = v.ToString();
            v = key.GetValue("ProxyServer");
            if (v != null && v is string)
                proxyInfo.Server = v.ToString();
            return proxyInfo;
        }
        public bool Enabled
        {
            get { return _enabled; }
            internal set { _enabled = value; }
        }
        public string Override
        {
            get { return _override; }
            internal set { _override = value; }
        }
        public string Server
        {
            get { return _server; }
            internal set { _server = value; }
        }
        /// <summary>
        /// 返回指定类型代理
        /// </summary>
        /// <param name="protocol">表示代理类型的字符串, 支持 ftp, http, https, socks, 大小写无关</param>
        /// <returns>返回表示代理服务器的IPEndPoint, 如果找不到返回 null</returns>
        /// <exception cref="ArgumentException">如果传入的 protocol 不在支持行列</exception>
        public IPEndPoint this[string protocol]
        {
            get
            {
                if (string.IsNullOrEmpty(_server))
                    return null;
                //ftp=192.168.0.1:80;http=192.168.0.1:80;https=192.168.0.1:80;socks=192.168.0.1:1080
                bool requestValid = false;
                switch (protocol.ToUpper())
                {
                    case "FTP":
                    case "HTTP":
                    case "HTTPS":
                    case "SOCKS":
                        requestValid = true;
                        break;
                    default:
                        break;
                }
                if (!requestValid)
                    throw new ArgumentException("无效索引");
                Regex reg = new Regex(regexp, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection mc = reg.Matches(_server);
                if (mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        if (m.Groups[1].Captures[0].Value.ToUpper() == protocol.ToUpper())
                        {
                            string addr = m.Groups[2].Captures[0].Value;
                            string port = m.Groups[3].Captures[0].Value;
                            IPAddress ipAddr;
                            if (IPAddress.TryParse(addr, out ipAddr))
                            {
                                int portNum = 0;
                                if (int.TryParse(port, out portNum))
                                {
                                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, portNum);
                                    return ipEndPoint;
                                }
                            }
                            break;
                        }
                    }
                }
                return null;
            }
        }
    }
} 
