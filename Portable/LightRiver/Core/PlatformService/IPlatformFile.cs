using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver
{
    public interface IPlatformFile
    {
        Task<bool> ExistsAsync(string path);

        Task<Stream> OpenReadStreamAsync(string path);

        Task<Stream> OpenWriteStreamAsync(string path);

        Task MoveAndReplaceAsync(string sourcePath, string destinationPath);
    }
}
