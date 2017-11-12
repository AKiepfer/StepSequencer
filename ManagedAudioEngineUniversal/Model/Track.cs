using System.Threading.Tasks;
using ManagedAudioEngineUniversal.Core;

namespace ManagedAudioEngineUniversal.Model
{
    public class Track
    {
        public Track(ISoundPlayerBuilder<IStorageFileEx> soundBuilder)
        {
            SoundBuilder = soundBuilder;
            ChannelVolume = SoundBuilder.Volume;
            ChannelPan = SoundBuilder.Pan;
            TimeStretch = SoundBuilder.Pitch;

            PlayAtTick = new bool[32];
        }

        public ISoundPlayerBuilder<IStorageFileEx> SoundBuilder { get; private set; }

        public double TimeStretch { get; set; }

        public double ChannelPan { get; set; }

        public double ChannelVolume { get; set; }

        public bool[] PlayAtTick { get; set; }

        public bool this[int index]
        {
            get { return PlayAtTick[index]; }
            set { PlayAtTick[index] = value; }
        }

        public string GetDescription()
        {
            return SoundBuilder.Description;
        }

        public async Task<ISoundPlayerBuilder<IStorageFileEx>> PlayWithSample(IStorageFileEx storedSample)
        {
            return await SoundBuilder
                .WithInput(storedSample)
                .BuildAsync();
        }

        public void PlayWithVolume(double volume)
        {
            SoundBuilder.WithChannelVolume(volume);
        }

        public void PlayWithPan(double pan)
        {
            SoundBuilder.WithChannelPan(pan);
        }

        public void PlayWithPitch(double pitch)
        {
            SoundBuilder.WithPitch(pitch);
        }

        public void PlayWithTicks(bool[] ticks)
        {
            PlayAtTick = ticks;
        }

        public void Stop()
        {
            SoundBuilder.Stop();
        }

        public void PlayAt(int tick)
        {
            if (IsMarkedAtTick(tick))
            {
                SoundBuilder.Play();
            }
        }

        public bool IsMarkedAtTick(int tick)
        {
            return PlayAtTick[tick];
        }
    }
}