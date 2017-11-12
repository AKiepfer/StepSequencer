using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Caliburn.Micro;
using ManagedAudioEngineUniversal.Core;
using ManagedAudioEngineUniversal.Model;

namespace UIUniversal.ViewModels
{
    public class TrackStepViewModel : PropertyChangedBase
    {
        private readonly Track _track;
        private double _pan;
        private double _channelVolume;
        private string _description;
        private int _trackStepCount;
        private double _timeStretch;

        public EventHandler TracksChanged;
        public EventHandler DescriptionChanged;

        public TrackStepViewModel(Track track)
        {
            _track = track;
            Volume = _track.ChannelVolume;
            Pan = _track.ChannelPan;
            Pitch = _track.TimeStretch;
            Description = _track.GetDescription();

            _trackStepCount = _track.PlayAtTick.Length;
            TransportIsAtTick = new BitArray(32);
        }


        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);

                if (DescriptionChanged != null)
                {
                    DescriptionChanged(this, EventArgs.Empty);
                }
            }
        }

        public double Volume
        {
            get { return _channelVolume; }
            set
            {
                _channelVolume = value;
                NotifyOfPropertyChange(() => Volume);

                _track.PlayWithVolume(Volume);
            }
        }


        public double Pitch
        {
            get { return _timeStretch; }
            set
            {
                _timeStretch = value;
                NotifyOfPropertyChange(() => Pitch);

                _track.PlayWithPitch(Pitch);
            }
        }

        public double Pan
        {
            get { return _pan; }
            set
            {
                _pan = value;
                NotifyOfPropertyChange(() => Pan);

                _track.PlayWithPan(Pan);
            }
        }


        public int TrackStepCount
        {
            get { return _trackStepCount; }
            set
            {
                _trackStepCount = value;
                NotifyOfPropertyChange(() => TrackStepCount);
            }
        }

        public void SetAt(int index, bool value)
        {
            _track[index] = value;
        }

        public bool GetAt(int index)
        {
            return _track[index];
        }

        public BitArray TransportIsAtTick { get; set; }

        private volatile bool bStarted;

        public bool this[int index]
        {
            get { return TransportIsAtTick[index]; }
            set
            {
                TransportIsAtTick.SetAll(false);
                TransportIsAtTick.Set(index, true);

                if (TracksChanged != null)
                {
                    TracksChanged(index, EventArgs.Empty);
                }
            }
        }

        public  async Task SelectSampleAsync(IStorageFileEx file)
        {
            try
            {
                await _track.PlayWithSample(file);

                Description = _track.GetDescription();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to load new sample", ex.Message);
            }
          
        }
    }

}