using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagedAudioEngineUniversal.Core;
using SharpDX;
using SharpDX.Multimedia;

namespace ManagedAudioEngineUniversal.Model
{
    public class SamplePlayer : SoundPlayerBase<IStorageFileEx>, IDisposable
    {
        private DataStream _dataStream = null;
        private readonly PlayWorkItem _playItem = new PlayWorkItem();
        
        public override async Task<ISoundPlayerBuilder<IStorageFileEx>> BuildAsync()
        {
            if (Input == null)
                return this;

            DisposeInternally();

            IRandomAccessStreamEx streamOpenFile = await Input.OpenReadAsync();

            using (Stream nativeStream = streamOpenFile.AsStreamForRead())
            {
                using (var soundStream = new SoundStream(nativeStream))
                {
                    Description = Input.Name;

                    WaveFormat = soundStream.Format;

                    _dataStream = soundStream.ToDataStream();

                    SourceVoice = VoicePool.GetVoice(WaveFormat);

                    SourceVoice.PlayWith(_dataStream);
                }
            }

            return this;
            
        }

        public override void Play()
        {
            if (SourceVoice != null)
            {
                _playItem.Pan = Pan;
                _playItem.Pitch = Pitch;
                _playItem.Volume = Volume;

                SourceVoice.Play(_playItem);
            }
        }

        public override void Stop()
        {
            if (SourceVoice != null)
            {
                SourceVoice.Stop();
            }
        }

        private void DisposeInternally()
        {
            Stop();
            
            if (SourceVoice != null)
            {
                SourceVoice.Clear();

                VoicePool.PutVoice(SourceVoice, WaveFormat);
                SourceVoice = null;
            }

            if (_dataStream != null)
            {
                _dataStream.Dispose();
                _dataStream = null;
            }

        }

        public void Dispose()
        {
            DisposeInternally();
        }
    }
}