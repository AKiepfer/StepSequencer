using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using ManagedAudioEngineUniversal.Core;
using PersistencyUniversal.Dto;

namespace PersistencyUniversal.Services
{
    public class StorageFileLocator
    {
        public static IStorageFileEx FromDto(SampleFileDtoV1 sampleFileDtoV1)
        {
            if (sampleFileDtoV1 == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(sampleFileDtoV1.Path))
            {
                return null;
            }

            try
            {
                if (sampleFileDtoV1.FileType == FileType.Extern)
                {
                    //IEnumerable<ExternalStorageDevice> storageAssets =
                    //    Task.Run(async () => await ExternalStorage.GetExternalStorageDevicesAsync())
                    //        .Result;

                    //ExternalStorageDevice storageDevice = storageAssets.FirstOrDefault();

                    //if (storageDevice != null)
                    //{
                    //    ExternalStorageFile file =
                    //        Task.Run(async () => await storageDevice.GetFileAsync(sampleFileDtoV1.Path))
                    //            .Result;

                    //    return new ExternalStorageFileEx(file);
                    //}
                }

                if (sampleFileDtoV1.FileType == FileType.Normal)
                {
                    StorageFile file = null;
                    try
                    {
                        file = Task.Run(async () => await StorageFile.GetFileFromPathAsync(sampleFileDtoV1.Path)).Result;
                    }
                    catch (Exception)
                    {
                        var splittedPath = sampleFileDtoV1.Path.Split('\\').ToList();

                        //special logic for new versions of app (assets folder may be corrupted)
                        if (splittedPath.Contains("Assets") && splittedPath.Contains("Samples"))
                        {
                            var list = new List<string> { Package.Current.InstalledLocation.Path };

                            list.AddRange(splittedPath.SkipWhile(s => s != "Assets").ToList());
                            
                            var newPath = Path.Combine(list.ToArray());

                            file = Task.Run(async () => await StorageFile.GetFileFromPathAsync(newPath)).Result;
                        }
                    }
                   

                    return new StorageFileEx(file);
                }
            }
            catch
            {
            }

            return null;
        }
    }
}