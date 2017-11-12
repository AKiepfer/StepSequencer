using System.IO;
using System.Threading.Tasks;

namespace ManagedAudioEngineUniversal.Core
{
    public enum FileType
    {
        Normal,
        Extern,
    }

    public interface IStorageFileEx
    {
        Task<IRandomAccessStreamEx> OpenReadAsync();

        string Name { get; set; }

        string Path { get; set; }

        FileType FileType { get; set; }

        string Checksum { get; set; }

        Task<Stream> OpenStreamForWriteAsync();
    }

    public class StorageFileExDto : IStorageFileEx
    {
        public Task<IRandomAccessStreamEx> OpenReadAsync()
        {
            throw new System.NotImplementedException();
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FileType FileType { get; set; }
        public string Checksum { get; set; }

        public Task<Stream> OpenStreamForWriteAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}