using System.IO;

namespace ManagedAudioEngineUniversal.Core
{
    public interface IRandomAccessStreamEx
    {
        void Dispose();

        Stream AsStreamForRead();
    }
}