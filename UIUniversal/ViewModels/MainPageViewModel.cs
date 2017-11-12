using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Display;
using Caliburn.Micro;
using ManagedAudioEngineUniversal.Core;
using ManagedAudioEngineUniversal.Effects;
using ManagedAudioEngineUniversal.Model;
using PersistencyUniversal.Services;
using SharpDX.XAudio2;

namespace UIUniversal.ViewModels
{

    public class MainPageViewModel : PropertyChangedBase
    {
       
        private string _currentSample;
        
        private TrackStepViewModel _currentTrackViewModels;
        private double _masterBpm = 90;
        private double _masterPan;
        private double _masterVolume = 0.8;
        
        private DisplayRequest _displayRequest;
        private bool _playIsChecked;
        private bool _playIsEnable;
        private bool _pauseIsEnable;
        private bool _stopIsEnabled;
        private bool _pauseIsChecked;
        private bool _stopIsChecked;
        private bool _isRendering;
        private bool _isNotRendering;
        private string _renderProgress;
        private string _version;
        private Transporter _transporter;

        public MainPageViewModel()
        {
            TrackViewModels = new ObservableCollection<TrackStepViewModel>();

            Tracks = new ConcurrentBag<Track>();
            TransportViewModel = new TransportViewModel();
            TransportViewModel[0] = true;

            MasterBpm = 90;
            MasterPan = 0.0;
            MasterVolume = 0.8;

            PauseIsEnabled = false;
            PauseIsChecked = false;
            PlayIsEnabled = true;
            PlayIsChecked = false;
            StopIsEnabled = false;
            StopIsEnabled = false;
            IsRendering = false;

            Version = "Beta 2.0";
            
            SetupPlayer();
            
            AudioDefines.XAudio.StartEngine();
        }

        public ObservableCollection<TrackStepViewModel> TrackViewModels { get; set; }

        private Transporter Transporter
        {
            get
            {
                if (_transporter == null)
                {
                    _transporter = new Transporter(32, PlayTracks);
                }

                return _transporter;
            } 
        }

        public double MasterBpm
        {
            get { return _masterBpm; }
            set
            {
                _masterBpm = value;

                Transporter.Bpm = _masterBpm;

                NotifyOfPropertyChange(() => MasterBpm);
            }
        }

        public double MasterVolume
        {
            get { return _masterVolume; }
            set
            {
                _masterVolume = value;

                AudioDefines.MasteringVoice.SetVolume((float)_masterVolume);

                NotifyOfPropertyChange(() => MasterVolume);
            }
        }


        public double MasterPan
        {
            get { return _masterPan; }
            set
            {
                _masterPan = value;

                float left = AudioDefines.SubmixVoice.Volume;
                float right = AudioDefines.SubmixVoice.Volume;

                if (_masterPan > 0)
                {
                    right = AudioDefines.SubmixVoice.Volume - (float)_masterPan;
                }
                else
                {
                    left = AudioDefines.SubmixVoice.Volume + (float)_masterPan;
                }

                AudioDefines.SubmixVoice.SetChannelVolumes(2, new[] { left, right });

                NotifyOfPropertyChange(() => MasterPan);
            }
        }

        public ConcurrentBag<Track> Tracks { get; set; }
        
        public TrackStepViewModel CurrentTrackViewModel
        {
            get { return _currentTrackViewModels; }
            set
            {
                if (ReferenceEquals(value, _currentTrackViewModels))
                {
                    return;
                }

                if (_currentTrackViewModels != null)
                {
                    _currentTrackViewModels.DescriptionChanged -= DescriptionChanged;
                }

                _currentTrackViewModels = value;

                if (_currentTrackViewModels != null)
                {
                    _currentTrackViewModels.DescriptionChanged += DescriptionChanged;
                    CurrentSample = _currentTrackViewModels.Description;
                }

                NotifyOfPropertyChange(() => CurrentTrackViewModel);
            }
        }


        public TransportViewModel TransportViewModel { get; set; }

        public string CurrentSample
        {
            get { return _currentSample; }
            set
            {
                _currentSample = value;

                NotifyOfPropertyChange(() => CurrentSample);
            }
        }

        private void DescriptionChanged(object sender, EventArgs eventArgs)
        {
            var vm = sender as TrackStepViewModel;

            if (vm == null)
            {
                return;
            }

            CurrentSample = vm.Description;
        }

        public bool PlayIsEnabled
        {
            get { return _playIsEnable; }
            set
            {
                if (value.Equals(_playIsEnable)) return;
                _playIsEnable = value;

                NotifyOfPropertyChange();
            }
        }

        public bool PauseIsEnabled
        {
            get { return _pauseIsEnable; }
            set
            {
                if (value.Equals(_pauseIsEnable)) return;
                _pauseIsEnable = value;

                NotifyOfPropertyChange();
            }
        }

        public bool StopIsEnabled
        {
            get { return _stopIsEnabled; }
            set
            {
                if (value.Equals(_stopIsEnabled)) return;
                _stopIsEnabled = value;

                NotifyOfPropertyChange();
            }
        }

        public bool PlayIsChecked
        {
            get { return _playIsChecked; }
            set
            {
                if (value.Equals(_playIsChecked)) return;
                _playIsChecked = value;

                if (_playIsChecked)
                {
                    ExecuteStart();

                    PauseIsEnabled = true;
                    PauseIsChecked = false;

                    StopIsEnabled = true;
                    StopIsChecked = false;

                    PlayIsEnabled = false;
                }
                
                NotifyOfPropertyChange();
            }
        }

        public bool PauseIsChecked
        {
            get { return _pauseIsChecked; }
            set
            {
                if (value.Equals(_pauseIsChecked)) return;
                _pauseIsChecked = value;

                if (_pauseIsChecked)
                {
                    ExecutePause();

                    PlayIsEnabled = true;
                    PlayIsChecked = false;

                    StopIsEnabled = true;
                    StopIsChecked = false;

                    PauseIsEnabled = false;
                }

                NotifyOfPropertyChange();
            }
        }

        public bool StopIsChecked
        {
            get { return _stopIsChecked; }
            set
            {
                if (value.Equals(_stopIsChecked)) return;
                _stopIsChecked = value;

                if (_stopIsChecked)
                {
                    ExecuteStop();

                    PauseIsEnabled = false;
                    PauseIsChecked = false;

                    PlayIsEnabled = true;
                    PlayIsChecked = false;

                    StopIsEnabled = false;
                }

                NotifyOfPropertyChange();
            }
        }

        public void ExecuteStart()
        {
            Execute.OnUIThread(() =>
                               {
                                   if (_displayRequest != null)
                                   {
                                       _displayRequest.RequestRelease();
                                       _displayRequest = null;

                                       _displayRequest = new DisplayRequest();
                                       _displayRequest.RequestActive();
                                   }
                                   else
                                   {
                                       _displayRequest = new DisplayRequest();
                                       _displayRequest.RequestActive();
                                   }
                               });

            Transporter.Start();
        }

        public void ExecutePause()
        {
            Execute.OnUIThread(() =>
                               {
                                   if (_displayRequest != null)
                                   {
                                       _displayRequest.RequestRelease();
                                       _displayRequest = null;
                                   }
                               });

            Transporter.Pause();
        }

        public void ExecuteStop()
        {
            Transporter.Stop();

            Parallel.ForEach(Tracks, track => track.Stop());

            Execute.OnUIThread(() =>
                               {
                                   if (_displayRequest != null)
                                   {
                                       _displayRequest.RequestRelease();
                                       _displayRequest = null;
                                   }
                               });

           
            CurrentTrackViewModel[0] = true;
        }

        public TrackStepViewModel AddTrack()
        {
            var player = CreateSamplePlayer();

            return AddTrack(player);
        }


        public TrackStepViewModel AddTrack(SamplePlayer samplePlayer)
        {
            var track = new Track(samplePlayer);

            Tracks.Add(track);

            var trackVm = new TrackStepViewModel(track);

            try
            {
                if (CurrentTrackViewModel != null)
                {
                    var index = TrackViewModels.IndexOf(CurrentTrackViewModel);

                    if (index != -1)
                    {

                        TrackViewModels.Insert(index + 1, trackVm);
            return trackVm;
        }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            TrackViewModels.Add(trackVm);
            return trackVm;
        }

        public static SamplePlayer CreateSamplePlayer()
        {
            var player = new SamplePlayer();

            player.WithXAudio(AudioDefines.XAudio)
                .WithWaveFormat(AudioDefines.WaveFormat)
                .WithVoicePool(AudioDefines.VoicePool);

            return player;
        }

        private void PlayTracks(int step)
        {
            try
            {
                foreach (var track in Tracks)
                {
                    track.PlayAt(step);
                }

                if (!IsRendering)
                {
                    CurrentTrackViewModel[step] = true;
                }
            }
            catch
            {
            }
        }

        private void SetupPlayer()
        {
            TrackViewModels =
              new ObservableCollection<TrackStepViewModel>(Tracks.Select(track => new TrackStepViewModel(track)));

            var first = AddTrack();

            CurrentTrackViewModel = first;
        }

        public string Version
        {
            get { return _version; }
            set
            {
                if (value == _version) return;
                _version = value;
                NotifyOfPropertyChange();
            }
        }

        public void LoadProject(IStorageFileEx projectFile)
        {
            try
            {
                Stream file = Task.Run(async () => await projectFile.OpenStreamForWriteAsync()).Result;

                using (var streamReader = new StreamReader(file, Encoding.UTF8))
                {
                    var json = streamReader.ReadToEnd();

                    ReadFromJson(json);
                }
            }
            catch
            {
            }
        }

        public void SaveProject(string projectFileName, IStorageFolderEx saveToFolder)
        {
            try
            {
                var jsonData = GetProjectAsJson();

                var data = Encoding.UTF8.GetBytes(jsonData);

                IEnumerable<IStorageFileEx> files = Task.Run(async () => await saveToFolder.GetFilesAsync()).Result;

                if (files.Any(ex => ex.Name == projectFileName))
                {
                    projectFileName = projectFileName + "_" +
                                      DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                }

                IStorageFileEx file =
                    Task.Run(
                        async () =>
                            await
                                saveToFolder.CreateFileAsync(projectFileName + ".seqproj",
                                    CreationCollisionOption.ReplaceExisting)).Result;

                using (Stream s = Task.Run(async () => await file.OpenStreamForWriteAsync()).Result)
                {
                    Task.Run(async () => await s.WriteAsync(data, 0, data.Length)).Wait();
                }
            }
            catch
            {
            }
        }

        private string GetProjectAsJson()
        {
            var saveService = new PersistencyService
            {
                MasterTrackBpm = MasterBpm,
                MasterTrackPan = MasterPan,
                MasterTrackVolume = MasterVolume,
                Tracks = Tracks.ToList()
            };


            var jsonData = saveService.CreateJsonProjectFromData();
            return jsonData;
        }

        private void ReadFromJson(string jsonData)
        {
            var saveService = new PersistencyService();

            saveService.CreateDataFromJsonProject(AudioDefines.XAudio, AudioDefines.WaveFormat, AudioDefines.VoicePool, jsonData);

            MasterPan = saveService.MasterTrackPan;
            MasterVolume = saveService.MasterTrackVolume;
            MasterBpm = saveService.MasterTrackBpm;

            Tracks = new ConcurrentBag<Track>(saveService.Tracks);
            TrackViewModels.Clear();

            foreach (var track in Tracks)
            {
                TrackViewModels.Add(new TrackStepViewModel(track));
            }

            if (TrackViewModels.Any())
            {
                CurrentTrackViewModel = TrackViewModels.First();
            }
        }

        public void SaveProject(IStorageFileEx projectFile)
        {
            try
            {
                var jsonData = GetProjectAsJson();

                var data = Encoding.UTF8.GetBytes(jsonData);

                using (Stream s = Task.Run(async () => await projectFile.OpenStreamForWriteAsync()).Result)
                {
                    Task.Run(async () => await s.WriteAsync(data, 0, data.Length)).Wait();
                }
            }
            catch
            {
            }
        }

        public bool IsNotRendering
        {
            get { return _isNotRendering; }
            set
            {
                if (value.Equals(_isNotRendering)) return;
                _isNotRendering = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsRendering
        {
            get { return _isRendering; }
            set
            {
                IsNotRendering = !value;

                if (value.Equals(_isRendering)) 
                    return;

                _isRendering = value;



                NotifyOfPropertyChange();
            }
        }

        public string RenderProgress
        {
            get { return _renderProgress; }
            set
            {
                if (value == _renderProgress) return;
                _renderProgress = value;
                NotifyOfPropertyChange();
            }
        }

        public async void RenderProject(StorageFileEx saveToFile)
        {
            //second rendering not allowed
            if (IsRendering)
            {
                return;
            }

            RenderProgress = "Initiate rendering ...";

            IsRendering = true;

            StopIsChecked = true;

            var tempWaveFileStream = await saveToFile.OpenStreamForWriteAsync();

            await Task.Run(() =>
                     {
                         ExecuteStop();

                         var waveFileWriter = new WaveFileWriterEffect();
                         var descriptor = new EffectDescriptor(waveFileWriter);

                         AudioDefines.MasteringVoice.SetEffectChain(descriptor);

                         AudioDefines.MasteringVoice.EnableEffect(0);

                         var silenceSamplePlayer = new SilenceSamplePlayer();

                         silenceSamplePlayer.WithXAudio(AudioDefines.XAudio)
                            .WithWaveFormat(AudioDefines.WaveFormat)
                            .WithVoicePool(AudioDefines.VoicePool);
                         
                         silenceSamplePlayer.BuildAsync().Wait();
                         silenceSamplePlayer.Play();

                         RenderProgress = string.Format("rendering");

                         Task.Delay(500).Wait();

                         var previousVolume = MasterVolume;
                         MasterVolume = 0.0;
                         
                         waveFileWriter.Start();

                         PlayIsChecked = true;

                         MasterVolume = previousVolume;

                         Task.Delay(30000).Wait();

                         MasterVolume = 0.0;

                         waveFileWriter.Stop(tempWaveFileStream);

                         StopIsChecked = true;

                         silenceSamplePlayer.Dispose();

                         AudioDefines.MasteringVoice.DisableEffect(0);

                         AudioDefines.MasteringVoice.SetEffectChain(null);

                         waveFileWriter.Dispose();

                         MasterVolume = previousVolume;
                     });

            tempWaveFileStream.Dispose();

            IsRendering = false;
        }
    }
}