using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver
{
    public static class PlatformService
    {
        public static ICryptographyService Cryptography { get; set; }

        public static IDeviceInformationService DeviceInformation { get; set; }

        public static IDispatcherService Dispatcher { get; set; }

        public static INetowrkInformationService NetworkInformation { get; set; }
    }
}
