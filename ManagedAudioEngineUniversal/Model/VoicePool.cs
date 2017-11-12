using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using ManagedAudioEngineUniversal.Core;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace ManagedAudioEngineUniversal.Model
{
    public class VoicePool : IVoicePool
    {
        private readonly Func<WaveFormat, SourceVoiceEx> _objectGenerator;
        private readonly ConcurrentDictionary<WaveFormat, ConcurrentBag<SourceVoiceEx>> _objects;
        private int _nextOperationId;

        public VoicePool(XAudio2 xAudio2, Voice masterVoice)
        {
            _objects = new ConcurrentDictionary<WaveFormat, ConcurrentBag<SourceVoiceEx>>();
            _objectGenerator = format =>
                               {
                                   var newVoice = new SourceVoice(xAudio2, format, VoiceFlags.None, 3.0f, true);
                                   newVoice.SetOutputVoices(new[] {new VoiceSendDescriptor(masterVoice)});

                                   var newId = Interlocked.Increment(ref _nextOperationId);

                                   var sourceVoiceEx = new SourceVoiceEx(newVoice, Guid.NewGuid(), format, newId);

                                   return sourceVoiceEx;
                               };

        }

        public SourceVoiceEx GetVoice(WaveFormat waveFormat)
        {
            ConcurrentBag<SourceVoiceEx> bag;
            
            if (_objects.TryGetValue(waveFormat, out bag) && bag != null)
            {
                SourceVoiceEx item;

                if (bag.TryTake(out item) && item != null)
                {
                    item.SourceVoice.Start();

                    return item;
                }
            }

            var newItem = _objectGenerator(waveFormat);
            newItem.SourceVoice.Start();
            return newItem;
        }

        public bool IsPooled(SourceVoiceEx voice)
        {
            var pooled = _objects.Values.SelectMany(exs => exs).Any(ex => ReferenceEquals(voice, ex));

            return pooled;
        }

        public void PutVoice(SourceVoiceEx item, WaveFormat waveFormat)
        {
            item.Id = Guid.Empty;

            item.SourceVoice.Stop(XAudio2.CommitNow);

            ConcurrentBag<SourceVoiceEx> bag;
            
            if (_objects.TryGetValue(waveFormat, out bag) && bag != null)
            {
                bag.Add(item);
                
                return;
            }
            
            bag = new ConcurrentBag<SourceVoiceEx> { item };

            _objects.TryAdd(waveFormat, bag);
        }
    }
}