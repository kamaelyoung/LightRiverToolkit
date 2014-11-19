using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver
{
    public class PlatformFile : IPlatformFile
    {
        public Task<bool> ExistsAsync(string path)
        {
            return StorageHelper.FileExistsInAppDataAsync(path);
        }

        public Task<Stream> OpenReadStreamAsync(string path)
        {
            return StorageHelper.OpenReadStreamInAppDataAsync(path);
        }

        public Task<Stream> OpenWriteStreamAsync(string path)
        {
            return StorageHelper.OpenWriteStreamInAppDataAsync(path);
        }

        public async Task MoveAndReplaceAsync(string sourcePath, string destinationPath)
        {
            var newFile = await StorageHelper.GetFileInAppDataAsync(destinationPath);
            var oldFile = await StorageHelper.GetFileInAppDataAsync(sourcePath);
            await newFile.MoveAndReplaceAsync(oldFile);
        }
    }
}
