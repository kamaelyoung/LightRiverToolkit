using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace LightRiver
{
    public class NetowrkInformationService : INetowrkInformationService
    {
        public string GetIpAddress()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null)
                return string.Empty;

            if (connectionProfile.NetworkAdapter == null)
                return string.Empty;

            string ipAddress = string.Empty;
            var hostNames = NetworkInformation.GetHostNames();
            foreach (var hostName in hostNames) {
                if (hostName.IPInformation == null)
                    continue;

                if (hostName.Type != HostNameType.Ipv4)
                    continue;

                ipAddress = hostName.CanonicalName;
                break;
            }

            return ipAddress;
        }
    }
}
