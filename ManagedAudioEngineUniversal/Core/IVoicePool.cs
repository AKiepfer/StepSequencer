using ManagedAudioEngineUniversal.Model;
using SharpDX.Multimedia;

namespace ManagedAudioEngineUniversal.Core
{
    public interface IVoicePool
    {
        SourceVoiceEx GetVoice(WaveFormat waveFormat);

        bool IsPooled(SourceVoiceEx voice);

        void PutVoice(SourceVoiceEx item, WaveFormat waveFormat);
    }
}