using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace ManagedAudioEngineUniversal.Core
{
    //The CharSet must match the CharSet of the corresponding PInvoke signature
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WIN32_FIND_DATA
    {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }


    public enum FINDEX_INFO_LEVELS
    {
        FindExInfoStandard = 0,
        FindExInfoBasic = 1
    }

    public enum FINDEX_SEARCH_OPS
    {
        FindExSearchNameMatch = 0,
        FindExSearchLimitToDirectories = 1,
        FindExSearchLimitToDevices = 2
    }

    public class StorageFolderEx : IStorageFolderEx
    {
        //private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        //private IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        //[DllImport("api-ms-win-core-file-l1-2-1.dll", SetLastError = true)]
        //static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        //[DllImport("api-ms-win-core-file-l1-2-1.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        //static extern IntPtr FindFirstFileEx(
        //    string lpFileName,
        //    FINDEX_INFO_LEVELS fInfoLevelId,
        //    out WIN32_FIND_DATA lpFindFileData,
        //    FINDEX_SEARCH_OPS fSearchOp,
        //    IntPtr lpSearchFilter,
        //    int dwAdditionalFlags);

        //[DllImport("api-ms-win-core-file-l1-2-1.dll", SetLastError = true)]
        //static extern bool FindClose(IntPtr hFindFile);

        //[DllImport("api-ms-win-core-file-l1-2-1.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        //static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        private StorageFolder _storageFolder;

        public StorageFolderEx(StorageFolder storageFolder)
        {
            _storageFolder = storageFolder;
            Name = _storageFolder.Name;
            DisplayName = _storageFolder.Name;
            Path = _storageFolder.Path;
        }

        public StorageFolderEx(StorageFolder storageFolder, string name)
        {
            _storageFolder = storageFolder;
            Path = _storageFolder.Path;
            Name = _storageFolder.Name;
            DisplayName = name;
        }

        public StorageFolderEx(string path, string name)
        {
            Path = path + @"\" + name;
            Name = name;
            DisplayName = name;
        }

        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public string Path { get; private set; }

        private async Task<StorageFolder> GetStorageFolderAsync()
        {
            if (_storageFolder != null)
            {
                return _storageFolder;
            }

            _storageFolder = await StorageFolder.GetFolderFromPathAsync(Path);

            return _storageFolder;
        }

        public async Task<IEnumerable<IStorageFolderEx>> GetFoldersAsync()
        {
            try
            {
                //return await Task.Run(() =>
                //                    {
                //                        var directoryProvider = new DirectoryProvider();

                //                        var directoryData = directoryProvider.GetDirectories(Path);

                //                        var result = directoryData.Select(data => new StorageFolderEx(data.Path, data.Name)).ToList();

                //                        return result;
                //                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //exception occured, do it old fashioned
            var storageFolder = await GetStorageFolderAsync();

            IReadOnlyList<StorageFolder> storageFolerResult = await storageFolder.GetFoldersAsync();

            var backupResult = storageFolerResult.Select(folder => new StorageFolderEx(folder));

            return backupResult;
        }

        public async Task<IStorageFolderEx> GetFolderAsync(string name)
        {
            var storageFolder = await GetStorageFolderAsync();

            StorageFolder result = await storageFolder.GetFolderAsync(name);

            return new StorageFolderEx(result);
        }

        public async Task<IStorageFileEx> GetFileAsync(string name)
        {
            var storageFolder = await GetStorageFolderAsync();

            StorageFile file = await storageFolder.GetFileAsync(name);

            return new StorageFileEx(file); 
        }

        public async Task<IEnumerable<IStorageFileEx>> GetFilesAsync()
        {
            try
            {
                //return await Task.Run(() =>
                //{
                //    //var directoryProvider = new DirectoryProvider();

                //    //var fileData = directoryProvider.GetFiles(Path);

                //    //var result = fileData.Select(data => new StorageFileEx(data.Path, data.Name)).ToList();

                //    //return result;
                //});
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
            //exception occured, do it old fashioned
            var storageFolder = await GetStorageFolderAsync();

            var fallbackFiles = await storageFolder.GetFilesAsync();

            var backupResult = fallbackFiles.Select(file => new StorageFileEx(file));

            return backupResult;
        }

        public async Task<IStorageFileEx> CreateFileAsync(string projectFileName, CreationCollisionOption replaceExisting)
        {
            var storageFolder = await GetStorageFolderAsync();

            StorageFile file = await storageFolder.CreateFileAsync(projectFileName, replaceExisting);

            return new StorageFileEx(file);
        }
    }
}