using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace ManagedAudioEngineUniversal.Model
{
    public class PlayWorkItem
    {
        public double Volume { get; set; }
        public double Pan { get; set; }
        public double Pitch { get; set; }
    }

    public class DataStreamItem
    {
        public DataStream Stream { get; set; }
        public int Length { get; set; }
    }

    public class SourceVoiceEx
    {
        public SourceVoiceEx(SourceVoice sourceVoice, Guid id, WaveFormat waveFormat, int operationId)
        {
            SourceVoice = sourceVoice;
            Id = id;
            
            WaveFormat = waveFormat;
            _bufferSize = WaveFormat.ConvertLatencyToByteSize(8);

            OperationId = operationId;

            PreviousWorkItem = new PlayWorkItem{Pan = Double.MinValue, Pitch = Double.MinValue, Volume = Double.MinValue};
        }

        private void SourceVoiceOnBufferEnd(IntPtr intPtr)
        {
            var queued = SourceVoice.State.BuffersQueued;

            if (queued > 2)
            {
                return;
            }

            PlayNextBuffer();
        }

        private void PlayNextBuffer()
        {
            if (_currentBuffer == _maxBuffers)
                return;

            var newCurrentBuffer = Interlocked.Increment(ref _currentBuffer);

            AudioBuffer buffer;

            if (!_audioBuffers.TryGetValue(newCurrentBuffer, out buffer) || buffer == null)
            {
                return;
            }

            if (PreviousWorkItem.Volume != CurrentPlayWorkItem.Volume)
            {
                SourceVoice.SetVolume((float) CurrentPlayWorkItem.Volume, OperationId);
                PreviousWorkItem.Volume = CurrentPlayWorkItem.Volume;
            }

            if (PreviousWorkItem.Pan != CurrentPlayWorkItem.Pan)
            {
                SourceVoice.SetPan(CurrentPlayWorkItem.Pan, OperationId);
                PreviousWorkItem.Pan = CurrentPlayWorkItem.Pan;
            }

            if (PreviousWorkItem.Pitch != CurrentPlayWorkItem.Pitch)
            {
                SourceVoice.SetFrequencyRatio((float) CurrentPlayWorkItem.Pitch, OperationId);
                PreviousWorkItem.Pitch = CurrentPlayWorkItem.Pitch;
            }

            SourceVoice.SubmitSourceBuffer(buffer, null);
            
            AudioDefines.XAudio.CommitChanges(OperationId);
        
        }

        public SourceVoice SourceVoice { get; set; }
        public Guid Id { get; set; }
        public WaveFormat WaveFormat { get; set; }
        public int OperationId { get; set; }
       // public DataStream DataStream { get; set; }
        public PlayWorkItem CurrentPlayWorkItem { get; set; }
        public PlayWorkItem PreviousWorkItem { get; set; }

        private readonly object _dataStreamLock = new object();
        private readonly int _bufferSize;

        private readonly Dictionary<int, AudioBuffer> _audioBuffers = new Dictionary<int, AudioBuffer>();

        public void PlayWith(DataStream dataStream)
        {
            _currentBuffer = 0;
            _maxBuffers = 0;

            var readBytes = new byte[_bufferSize];

            while (true)
            {
                int readLength;

                if (dataStream.RemainingLength == 0)
                    return;

                lock (_dataStreamLock)
                {
                    readLength = dataStream.Read(readBytes, 0, _bufferSize);
                }

                var stream = new DataStream(readLength, true, true);
                stream.Write(readBytes, 0, readLength);
                stream.Position = 0;

                var buffer = new AudioBuffer
                             {
                                 Stream = stream,
                                 AudioBytes = (int) readLength,
                                 Flags = BufferFlags.EndOfStream
                             };

                _audioBuffers.Add(_maxBuffers, buffer);

                _maxBuffers++;
            }
        }

        private int _currentBuffer;
        private int _maxBuffers;

        public void Play(PlayWorkItem item)
        {
            SourceVoice.BufferEnd -= SourceVoiceOnBufferEnd;
            
            //current play meta data
            CurrentPlayWorkItem = item;
            
            Interlocked.Exchange(ref _currentBuffer, 0);

            SourceVoice.BufferEnd += SourceVoiceOnBufferEnd;

            PlayNextBuffer();
        }

        public void Stop()
        {
            SourceVoice.BufferEnd -= SourceVoiceOnBufferEnd;

            Interlocked.Exchange(ref _currentBuffer, 0);
        }

        public void Clear()
        {
            //flush buffers
            SourceVoice.FlushSourceBuffers();

            while (SourceVoice.State.BuffersQueued > 0)
            {
                SourceVoice.FlushSourceBuffers();
            }

            SourceVoice.FlushSourceBuffers();

            //stop sound
            SourceVoice.Stop(XAudio2.CommitNow);

            var buffers = _audioBuffers.ToArray();
            _audioBuffers.Clear();

            //dispose buffers
            foreach (var buffer in buffers)
            {
                buffer.Value.Stream.Dispose();
            }

            //flush buffers
            SourceVoice.FlushSourceBuffers();

            while (SourceVoice.State.BuffersQueued > 0)
            {
                SourceVoice.FlushSourceBuffers();
            }
        }
    }
}