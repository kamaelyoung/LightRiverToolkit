using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver
{
    public interface IDeviceInformationService
    {
        string DeviceUniqueId { get; }
    }
}
