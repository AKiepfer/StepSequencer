using ManagedAudioEngineUniversal.Core;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace ManagedAudioEngineUniversal.Model
{
    public class AudioDefines
    {
        public static MasteringVoice MasteringVoice;
        public static SubmixVoice SubmixVoice;
        public static XAudio2 XAudio;
        public static IVoicePool VoicePool;
        public static WaveFormat WaveFormat;

        static AudioDefines()
        {
            WaveFormat = new WaveFormat();
            XAudio = new XAudio2();
            MasteringVoice = new MasteringVoice(XAudio);

            SubmixVoice = new SubmixVoice(XAudio);
            SubmixVoice.SetOutputVoices(new[] { new VoiceSendDescriptor(MasteringVoice) });

            VoicePool = new VoicePool(XAudio, SubmixVoice);
        }
    }
}