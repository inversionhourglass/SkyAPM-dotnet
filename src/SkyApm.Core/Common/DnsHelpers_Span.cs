using System.Linq;

namespace SkyApm.Common
{
    partial class DnsHelpers
    {
        private static string _IpV4OrHostName;

        public static string GetIpV4OrHostName()
        {
            if (_IpV4OrHostName == null)
            {
                try
                {
                    _IpV4OrHostName = GetIpV4s().FirstOrDefault() ?? GetHostName();
                }
                catch
                {
                    _IpV4OrHostName = "UNKNOW";
                }
            }
            return _IpV4OrHostName;
        }
    }
}
