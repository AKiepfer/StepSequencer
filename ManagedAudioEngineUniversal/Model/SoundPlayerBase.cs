using System.Threading.Tasks;
using ManagedAudioEngineUniversal.Core;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace ManagedAudioEngineUniversal.Model
{
    public abstract class SoundPlayerBase<T> : ISoundPlayerBuilder<T>
    {
        protected XAudio2 XAudio { get; set; }
        protected WaveFormat WaveFormat { get; set; }
        protected IVoicePool VoicePool { get; set; }

        protected SourceVoiceEx SourceVoice { get; set; }

        protected SoundPlayerBase()
        {
            Volume = 0.8;
            Pan = 0.0;
            Pitch = 1.0;
            Description = ". . .";
        }

        public ISoundPlayerBuilder<T> WithXAudio(XAudio2 xaudio)
        {
            XAudio = xaudio;

            return this;
        }


        public ISoundPlayerBuilder<T> WithWaveFormat(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;

            return this;
        }

        public ISoundPlayerBuilder<T> WithInput(T input)
        {
            Input = input;

            return this;
        }

        public ISoundPlayerBuilder<T> WithVoicePool(IVoicePool voicePool)
        {
            VoicePool = voicePool;

            return this;
        }

        public ISoundPlayerBuilder<T> WithChannelVolume(double volume)
        {
            Volume = volume;

            //if (_SourceVoice != null)
            //    _SourceVoice.SetVolume((float) volume, XAudio2.CommitNow);

            return this;
        }

        public ISoundPlayerBuilder<T> WithChannelPan(double pan)
        {
            Pan = pan;

            //if (_SourceVoice != null)
            //    _SourceVoice.SetPan(pan);

            return this;
        }

        public ISoundPlayerBuilder<T> WithPitch(double pitch)
        {
            Pitch = pitch;

            //if (_SourceVoice != null)
            //    _SourceVoice.SetFrequencyRatio((float) pitch);

            return this;
        }

        public abstract Task<ISoundPlayerBuilder<T>> BuildAsync();

        public abstract void Play();

        public abstract void Stop();

        public string Description { get; set; }
        public T Input { get; private set; }
        public double Pan { get; private set; }
        public double Volume { get; private set; }
        public double Pitch { get; private set; }
    }
}