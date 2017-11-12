using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ManagedAudioEngineUniversal.Core
{
    public class StorageFileEx : IStorageFileEx
    {
        private StorageFile _storageFile;
        private string _checksum = null;
        

        public StorageFileEx(StorageFile storageFile)
        {
            _storageFile = storageFile;
            FileType = FileType.Normal;
            Name = _storageFile.Name;
            Path = _storageFile.Path;
        }

        public StorageFileEx(string path, string fileName)
        {
            FileType = FileType.Normal;
            Name = fileName;
            Path = path + @"\" + Name;
        }

        public StorageFolder Root { get; set; }
        public string Name { get; set; }

        public string Path { get; set; }

        public FileType FileType { get; set; }

        public async Task<StorageFile> GetStorageFileAsync()
        {
            if (_storageFile != null)
            {
                return _storageFile;
            }

            _storageFile = await StorageFile.GetFileFromPathAsync(Path);

            return _storageFile;
        }

        public string Checksum
        {
            get
            {
                if (_checksum != null)
                {
                    return _checksum;
                }

                var stream = Task.Run(async () => await OpenReadAsync()).Result;

                if (stream == null)
                {
                    _checksum = "";
                    return _checksum;
                }

                using (var nativeStream = stream.AsStreamForRead())
                {
                    var memArray = new byte[nativeStream.Length];

                    nativeStream.Read(memArray, 0, (int)nativeStream.Length);

                    var alg = HashAlgorithmProvider.OpenAlgorithm("MD5");
                    IBuffer buff = CryptographicBuffer.CreateFromByteArray(memArray);
                    var hashed = alg.HashData(buff);
                    var md5Hash = CryptographicBuffer.EncodeToHexString(hashed);

                    _checksum = md5Hash;
                }

                return _checksum;
            }
            set {  }
        }

        public async Task<Stream> OpenStreamForWriteAsync()
        {
            var file = await GetStorageFileAsync();

            var stream = await file.OpenStreamForWriteAsync();

           return stream;
        }

        public async Task<IRandomAccessStreamEx> OpenReadAsync()
        {
            var file = await GetStorageFileAsync();

            IRandomAccessStream awaited = await file.OpenAsync(FileAccessMode.Read);

            return new RandomAccessStreamEx(awaited);
        }
    }
    
    public class RandomAccessStreamEx : IRandomAccessStreamEx
    {
        private readonly IRandomAccessStream m_Stream;

        public RandomAccessStreamEx(IRandomAccessStream stream)
        {
            m_Stream = stream;
        }


        public Stream AsStreamForRead()
        {
            return m_Stream.AsStreamForRead();
        }

        public void Dispose()
        {
            m_Stream.Dispose();
        }
    }

    public class ExternalRandomAccessStreamEx : IRandomAccessStreamEx
    {
        private readonly Stream m_Stream;

        public ExternalRandomAccessStreamEx(Stream stream)
        {
            m_Stream = stream;
        }


        public Stream AsStreamForRead()
        {
            return m_Stream;
        }

        public void Dispose()
        {
            m_Stream.Dispose();
        }
    }
}