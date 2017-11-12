using ManagedAudioEngineUniversal.Core;

namespace PersistencyUniversal.Dto
{
    public class SampleFileDtoV1
    {
        public string Checksum { get; set; }
        public string Path { get; set; }
        public FileType FileType { get; set; }

        public static SampleFileDtoV1 FromStorageFileEx(IStorageFileEx storageFileEx)
        {
            var dto = new SampleFileDtoV1
                      {
                          Checksum = storageFileEx.Checksum,
                          FileType = storageFileEx.FileType,
                          Path = storageFileEx.Path
                      };

            return dto;
        }
    }
}