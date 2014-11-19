using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;

namespace LightRiver
{
    public enum ApplicationDataLocation
    {
        Local,
        Roaming,
        Temp,
    };

    public static class StorageHelper
    {
        public enum FileOpenMode
        {
            Read,
            Write,
        };

        public static Task<bool> CopyFileFromInstalledLocation(string filePath, ApplicationDataLocation location = ApplicationDataLocation.Local, bool overwrite = true)
        {
            return CopyFileFromInstalledLocation(filePath, filePath, location, overwrite);
        }

        /// <summary>
        /// 將檔案從安裝目錄複製到ApplicationData的Local目錄下
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public static async Task<bool> CopyFileFromInstalledLocation(string sourcePath, string destinationPath, ApplicationDataLocation location = ApplicationDataLocation.Local, bool overwrite = true)
        {
            StorageFile destinationFile = null;

            try {
                var sourceStorage = Package.Current.InstalledLocation;
                var sourceFile = await sourceStorage.CreateFileAsync(sourcePath, CreationCollisionOption.OpenIfExists);

                StorageFolder destinationStorage = GetApplicationDataFolder(location);
                destinationStorage = await destinationStorage.CreateFolder2Async(destinationPath);
                var fileName = Path.GetFileName(destinationPath);

                if (!overwrite)
                    destinationFile = await sourceFile.CopyAsync(destinationStorage, fileName, NameCollisionOption.FailIfExists);
                else
                    destinationFile = await sourceFile.CopyAsync(destinationStorage, fileName, NameCollisionOption.ReplaceExisting);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                destinationFile = null;
            }

            return (destinationFile != null);
        }

        public static async Task CopyFolderFromInstalledLocation(string path, ApplicationDataLocation location = ApplicationDataLocation.Local, bool overwrite = true)
        {
            IStorageFolder destination = GetApplicationDataFolder(location);
            IStorageFolder root = Windows.ApplicationModel.Package.Current.InstalledLocation;

            if (!await FolderExistsInAppDataAsync(path)) {
                await destination.CreateFolderAsync(path);
            }

            destination = await destination.GetFolderAsync(path);
            root = await root.GetFolderAsync(path);

            IReadOnlyList<IStorageItem> items = await root.GetItemsAsync();
            foreach (IStorageItem item in items) {
                if (item.GetType() == typeof(StorageFile)) {
                    IStorageFile presFile = await root.CreateFileAsync(item.Name, CreationCollisionOption.OpenIfExists);

                    if (!overwrite)
                        await presFile.CopyAsync(destination, presFile.Name, NameCollisionOption.FailIfExists);
                    else
                        await presFile.CopyAsync(destination, presFile.Name, NameCollisionOption.ReplaceExisting);
                }
                else {
                    // If folder doesn't exist, than create new one on destination folder
                    if (!await FolderExistsInAppDataAsync(path + "\\" + item.Name))
                        await destination.CreateFolderAsync(item.Name);

                    // Do recursive copy for every items inside
                    await CopyFolderFromInstalledLocation(path + "\\" + item.Name);
                }
            }
        }

        /// <summary>
        /// 開啟ApplicationData Folder下的檔案
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static async Task<IInputStream> OpenSequentialReadInAppDataAsync(string filePath, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));
            StorageFolder folder = GetApplicationDataFolder(location);
            var file = await folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
            if (file == null)
                return null;

            return await file.OpenSequentialReadAsync();
        }

        /// <summary>
        /// 開啟Install Folder下的檔案
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<IInputStream> OpenSequentialReadInInstallAsync(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));
            var folder = Package.Current.InstalledLocation;
            var file = await folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
            if (file == null)
                return null;

            return await file.OpenSequentialReadAsync();
        }

        public static async Task<Stream> OpenReadStreamInAppDataAsync(string filePath, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            StorageFolder folder = GetApplicationDataFolder(location);
            var file = await folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
            if (file == null)
                return null;

            return await file.OpenStreamForReadAsync();
        }

        public static async Task<Stream> OpenWriteStreamInAppDataAsync(string filePath, bool overwrite = true, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            StorageFolder folder = GetApplicationDataFolder(location);
            folder = await folder.CreateFolder2Async(filePath);
            var fileName = Path.GetFileName(filePath);
            CreationCollisionOption createOption = (overwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync(fileName, createOption);
            if (file == null)
                return null;

            return await file.OpenStreamForWriteAsync();
        }

        public static async Task<Stream> OpenReadStreamInInstallAsync(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));
            var folder = Package.Current.InstalledLocation;
            var file = await folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
            if (file == null)
                return null;

            return await file.OpenStreamForReadAsync();
        }

        private static StorageFolder GetApplicationDataFolder(ApplicationDataLocation location)
        {
            StorageFolder folder = null;
            switch (location) {
                case ApplicationDataLocation.Local:
                    folder = ApplicationData.Current.LocalFolder;
                    break;
                case ApplicationDataLocation.Roaming:
                    folder = ApplicationData.Current.RoamingFolder;
                    break;
                case ApplicationDataLocation.Temp:
                    folder = ApplicationData.Current.TemporaryFolder;
                    break;
            }

            return folder;
        }

        /// <summary>
        /// 建立完整的子路徑
        /// </summary>
        /// <param name="storage">要建立路徑的跟目錄, EX: ApplicationData.LocalFolder</param>
        /// <param name="filePath">完整檔案名稱</param>
        /// <returns>最後一個建立的路徑的Handle</returns>
        public static async Task<StorageFolder> CreateFolder2Async(this StorageFolder storage, string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryName))
                return storage;

            const char directorySeparatorChar = '\\';
            string[] subDirectoies = directoryName.Split(new char[] { directorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            string path = string.Empty;
            StorageFolder parentFolder = storage;
            StorageFolder childFolder = null;

            for (int i = 0; i < subDirectoies.Length; i++) {
                path = subDirectoies[i];
                try {
                    childFolder = await parentFolder.GetFolderAsync(path);
                }
                catch (FileNotFoundException) {
                    childFolder = null;
                }

                if (null == childFolder)
                    parentFolder = await parentFolder.CreateFolderAsync(path);
                else
                    parentFolder = childFolder;
            }

            return parentFolder;
        }

        /// <summary>
        /// 取得檔案在ApplicationData Folder下的完整路徑
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static async Task<string> GetFilePathInApplicationDataAsync(string filePath, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            var storage = GetApplicationDataFolder(location);
            var file = await storage.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
            return file.Path;
        }

        /// <summary>
        /// 取得檔案在Install Folder下的完整路徑
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<string> GetFilePathInInstallFolderAsync(string filePath)
        {
            var storage = Package.Current.InstalledLocation;
            var file = await storage.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
            return file.Path;
        }

        /// <summary>
        /// 檢查目錄是否存在
        /// </summary>
        /// <param name="foldername"></param>
        /// <returns></returns>
        private static async Task<bool> FolderExistsInAppDataAsync(string foldername, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            var folder = GetApplicationDataFolder(location);

            try {
                await folder.GetFolderAsync(foldername);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// 檢查檔案是否存在
        /// </summary>
        /// <param name="fileName">相對路徑之檔案名稱</param>
        /// <param name="location">AppData的位置</param>
        /// <returns></returns>
        public static async Task<bool> FileExistsInAppDataAsync(string fileName, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            try {
                var file = await GetFileInAppDataAsync(fileName, location);
                return (file != null);
            }
            catch (IOException) {
                return false;
            }
        }

        /// <summary>
        /// Delete a file asynchronously
        /// </summary>
        /// <param name="fileName">相對路徑之檔案名稱</param>
        /// <param name="location">AppData的位置</param>
        /// <returns></returns>
        public static async Task<bool> DeleteFileInAppDataAsync(string fileName, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            try {
                var file = await GetFileInAppDataAsync(fileName, location);
                if (file == null)
                    return false;

                await file.DeleteAsync();
                return true;
            }
            catch (IOException) {
                return false;
            }
        }

        /// <summary>
        /// At the moment the only way to check if a file exists to catch an exception... :/
        /// </summary>
        /// <param name="fileName">相對路徑之檔案名稱</param>
        /// <param name="location">AppData的位置</param>
        /// <returns></returns>
        public static async Task<StorageFile> GetFileInAppDataAsync(string fileName, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            try {
                StorageFolder storageFolder = GetApplicationDataFolder(location);
                return await storageFolder.GetFileAsync(fileName);
            }
            catch (IOException){
                return null;
            }
        }

        public static async Task<BinaryReader> OpenBinaryReaderInAppDataAsync(string filePath, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            var fileStream = await StorageHelper.OpenReadStreamInAppDataAsync(filePath, location);
            if (fileStream == null)
                return null;
            return new BinaryReader(fileStream);
        }

        public static async Task<BinaryWriter> OpenBinaryWriterInAppDataAsync(string filePath, bool overWrite = true, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            var fileStream = await StorageHelper.OpenWriteStreamInAppDataAsync(filePath, overWrite, location);
            if (fileStream == null)
                return null;
            return new BinaryWriter(fileStream);
        }

        public static async Task<StreamReader> OpenStreamReaderInAppDataAsync(string filePath, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            var fileStream = await StorageHelper.OpenReadStreamInAppDataAsync(filePath, location);
            if (fileStream == null)
                return null;
            return new StreamReader(fileStream);
        }

        public static async Task<StreamWriter> OpenStreamWriterInAppDataAsync(string filePath, bool overWrite = true, ApplicationDataLocation location = ApplicationDataLocation.Local)
        {
            var fileStream = await StorageHelper.OpenWriteStreamInAppDataAsync(filePath, overWrite, location);
            if (fileStream == null)
                return null;
            return new StreamWriter(fileStream);
        }
    }
}
