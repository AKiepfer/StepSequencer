using System;
using System.Threading.Tasks;
using ManagedAudioEngineUniversal.Core;
using SharpDX;

namespace ManagedAudioEngineUniversal.Model
{
    public class SilenceSamplePlayer : SoundPlayerBase<IStorageFileEx>
    {
        private readonly byte[] _currentSampleBuffer = new byte[44100*1*2];
        private  DataStream _dataStream;
        private readonly PlayWorkItem _playItem = new PlayWorkItem();

        private SourceVoiceEx _overlapSourceVoice;
        private DataStream _overLapDataStream;
        
        public override Task<ISoundPlayerBuilder<IStorageFileEx>> BuildAsync()
        {
            DisposeInternally();
            
            _dataStream = new DataStream(_currentSampleBuffer.Length, true, true);
            _overLapDataStream = new DataStream(_currentSampleBuffer.Length, true, true);
           
            _dataStream.Read(_currentSampleBuffer, 0, _currentSampleBuffer.Length);
            _dataStream.Seek(0, 0);

            _overLapDataStream.Read(_currentSampleBuffer, 0, _currentSampleBuffer.Length);
            _overLapDataStream.Seek(0, 0);

            SourceVoice = VoicePool.GetVoice(WaveFormat);
            _overlapSourceVoice = VoicePool.GetVoice(WaveFormat);

            SourceVoice.PlayWith(_dataStream);
            _overlapSourceVoice.PlayWith(_overLapDataStream);

            return Task.FromResult<ISoundPlayerBuilder<IStorageFileEx>>(this);

        }

        public override void Play()
        {
            SourceVoice.SourceVoice.BufferEnd += SourceVoiceOnBufferEnd;

            SourceVoice.Play(_playItem);

            Task.Delay(1000).Wait();

            _overlapSourceVoice.SourceVoice.BufferEnd += SourceVoiceOnBufferEndOverlap;

            _overlapSourceVoice.Play(_playItem);
        }

        private void SourceVoiceOnBufferEnd(IntPtr intPtr)
        {
            SourceVoice.Play(_playItem);
        }

        private void SourceVoiceOnBufferEndOverlap(IntPtr intPtr)
        {
            _overlapSourceVoice.Play(_playItem);
        }

        public override void Stop()
        {
            if (SourceVoice != null)
            {
                SourceVoice.SourceVoice.BufferEnd -= SourceVoiceOnBufferEnd;
                SourceVoice.Stop();
            }

            if (_dataStream != null)
            {
                _dataStream.Dispose();
            }

            if (_overlapSourceVoice != null)
            {
                _overlapSourceVoice.SourceVoice.BufferEnd -= SourceVoiceOnBufferEndOverlap;
                _overlapSourceVoice.Stop();
            }

            if (_overLapDataStream != null)
            {
                _overLapDataStream.Dispose();
            }
        }

        private void DisposeInternally()
        {
            Stop();

            if (SourceVoice != null)
            {
                SourceVoice.Clear();
                VoicePool.PutVoice(SourceVoice, WaveFormat);
            }

            if (_overLapDataStream != null)
            {
                _overLapDataStream.Dispose();
            }

            if (_overlapSourceVoice != null)
            {
                _overlapSourceVoice.Clear();
                VoicePool.PutVoice(_overlapSourceVoice, WaveFormat);
            }
        }

        public void Dispose()
        {
            DisposeInternally();
        }
    }
}