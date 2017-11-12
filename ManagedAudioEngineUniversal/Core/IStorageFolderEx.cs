using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace ManagedAudioEngineUniversal.Core
{
    public interface IStorageFolderEx
    {
        string Name { get; }
        string DisplayName { get; }
        string Path { get; }

        Task<IEnumerable<IStorageFolderEx>> GetFoldersAsync();

        Task<IStorageFolderEx> GetFolderAsync(string name);

        Task<IEnumerable<IStorageFileEx>> GetFilesAsync();

        Task<IStorageFileEx> GetFileAsync(string name);

        Task<IStorageFileEx> CreateFileAsync(string projectFileName, CreationCollisionOption replaceExisting);
    }
}