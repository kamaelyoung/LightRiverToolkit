using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver
{
    public interface ICryptographyService
    {
        string HashMD5(string source);

        byte[] HashMD5(byte[] source);

        string EncryptTripleDES(string value, string key);

        string DecryptTripleDES(string value, string key);
    }
}
