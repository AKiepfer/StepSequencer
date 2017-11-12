using System.Threading.Tasks;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace ManagedAudioEngineUniversal.Core
{
    public interface IPlayablePlayer
    {
        void Play();

        void Stop();
    }

    public interface ISoundPlayerMetaData<out TSoundPlayerInput>
    {
        string Description { get; }

        TSoundPlayerInput Input { get; }

        double Pan { get; }

        double Volume { get; }

        double Pitch { get; }
    }

    public interface ISoundPlayerBuilder<TSoundPlayerInput> : IPlayablePlayer, ISoundPlayerMetaData<TSoundPlayerInput>
    {
        ISoundPlayerBuilder<TSoundPlayerInput> WithXAudio(XAudio2 xaudio);

        ISoundPlayerBuilder<TSoundPlayerInput> WithWaveFormat(WaveFormat waveFormat);

        ISoundPlayerBuilder<TSoundPlayerInput> WithInput(TSoundPlayerInput input);

        ISoundPlayerBuilder<TSoundPlayerInput> WithVoicePool(IVoicePool voicePool);

        ISoundPlayerBuilder<TSoundPlayerInput> WithChannelVolume(double volume);

        ISoundPlayerBuilder<TSoundPlayerInput> WithChannelPan(double pan);

        ISoundPlayerBuilder<TSoundPlayerInput> WithPitch(double pitch);

        Task<ISoundPlayerBuilder<TSoundPlayerInput>> BuildAsync();
    }
}