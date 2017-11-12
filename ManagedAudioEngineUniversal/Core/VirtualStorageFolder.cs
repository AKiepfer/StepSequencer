using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace ManagedAudioEngineUniversal.Core
{
    public class VirtualStorageFolder : IStorageFolderEx
    {
        private readonly List<IStorageFolderEx> m_Folders = new List<IStorageFolderEx>();

        public string Name
        {
            get { return "Root"; }
        }

        public string DisplayName { get; private set; }

        public string Path
        {
            get { return "Root"; }
        }

        public Task<IStorageFileEx> GetFileAsync(string name)
        {
            return Task.FromResult((IStorageFileEx)null);
        }

        public Task<IEnumerable<IStorageFolderEx>> GetFoldersAsync()
        {
            return Task.FromResult(m_Folders.Select(ex => ex));
        }

        public Task<IStorageFolderEx> GetFolderAsync(string name)
        {
            return Task.FromResult(m_Folders.FirstOrDefault(ex => ex.Name == name));
        }

        public Task<IEnumerable<IStorageFileEx>> GetFilesAsync()
        {
            return Task.FromResult(new List<IStorageFileEx>().Select(ex => ex));
        }

        public Task<IStorageFileEx> CreateFileAsync(string projectFileName, CreationCollisionOption replaceExisting)
        {
            return null;
        }

        public void AddFolder(IStorageFolderEx folder)
        {
            m_Folders.Add(folder);
        }
    }
}