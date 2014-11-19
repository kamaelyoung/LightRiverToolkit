using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace LightRiver
{
    public class DeviceInformationService : IDeviceInformationService
    {
        public string DeviceUniqueId
        {
            get
            {
                var packageSpecificToken = HardwareIdentification.GetPackageSpecificToken(null);
                var hardwareId = packageSpecificToken.Id;

                HashAlgorithmProvider hasher = HashAlgorithmProvider.OpenAlgorithm("MD5");
                var hashedHardwareId = hasher.HashData(hardwareId);

                return CryptographicBuffer.EncodeToHexString(hashedHardwareId);
            }
        }
    }
}
