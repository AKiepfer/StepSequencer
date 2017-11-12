using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagedAudioEngineUniversal.Core;
using ManagedAudioEngineUniversal.Model;
using Newtonsoft.Json;
using PersistencyUniversal.Dto;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace PersistencyUniversal.Services
{
    public class PersistencyService
    {
        public List<Track> Tracks { get; set; }

        public double MasterTrackPan { get; set; }
        public double MasterTrackPitch { get; set; }
        public double MasterTrackVolume { get; set; }
        public int MasterTrackTransportPosition { get; set; }
        public bool MasterTrackPlaying { get; set; }
        public double MasterTrackBpm { get; set; }

        public void CreateDataFromJsonProject(XAudio2 xaudio, WaveFormat waveFormat, IVoicePool voicePool, string jsonData)
        {
            var version = (ProjectVersionDto) JsonConvert.DeserializeObject(jsonData, typeof (ProjectVersionDto));

            switch (version.Version)
            {
                case 1:
                    ParseFromV1(xaudio, waveFormat, voicePool, jsonData);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void ParseFromV1(XAudio2 xaudio, WaveFormat waveFormat, IVoicePool voicePool, string jsonData)
        {
            var project = (ProjectDtoV1) JsonConvert.DeserializeObject(jsonData, typeof (ProjectDtoV1));

            Tracks = project.SampleTracks.ToList()
                .Select(v1 =>
                        {
                            
                            var samplePlayer =
                                new SamplePlayer();

                            var buildSamplePlayer = Task.Run(async () => await samplePlayer.WithInput(StorageFileLocator.FromDto(v1.File))
                                                                .WithXAudio(xaudio)
                                                                .WithWaveFormat(waveFormat)
                                                                .WithVoicePool(voicePool)
                                                                .WithPitch(v1.Pitch)
                                                                .WithChannelPan(v1.Pan)
                                                                .WithChannelVolume(v1.Volume)
                                                                .BuildAsync()).Result;
                                                     
                            var newTrack = new Track(buildSamplePlayer);
                            newTrack.PlayWithTicks(v1.PlayAt);
                            return newTrack;
                        })
                .ToList();

            MasterTrackPan = project.MasterTrack.Pan;
            MasterTrackPitch = project.MasterTrack.Pitch;
            MasterTrackVolume = project.MasterTrack.Volume;
            MasterTrackTransportPosition = project.MasterTrack.TransportPosition;
            MasterTrackPlaying = project.MasterTrack.Playing;
            MasterTrackBpm = project.MasterTrack.BPM;
        }

        public string CreateJsonProjectFromData()
        {
            var project = new ProjectDtoV1();

            project.SampleTracks = Tracks.Select(track =>
                                                 {
                                                     var fileDto = new SampleFileDtoV1();

                                                     if (track.SoundBuilder.Input != null)
                                                     {
                                                         fileDto.Checksum = track.SoundBuilder.Input.Checksum;
                                                         fileDto.FileType = track.SoundBuilder.Input.FileType;
                                                         fileDto.Path = track.SoundBuilder.Input.Path;
                                                     }

                                                     var trackDto = new SampleTrackDtoV1();

                                                     trackDto.File = fileDto;
                                                     trackDto.Pan = track.SoundBuilder.Pan;
                                                     trackDto.Volume = track.SoundBuilder.Volume;
                                                     trackDto.Pitch = track.SoundBuilder.Pitch;
                                                     trackDto.PlayAt = track.PlayAtTick;

                                                     return trackDto;
                                                 })
                .ToList();

            project.MasterTrack = new MasterTrackDtoV1();

            project.MasterTrack.Pan = MasterTrackPan;
            project.MasterTrack.Pitch = MasterTrackPitch;
            project.MasterTrack.Volume = MasterTrackVolume;
            project.MasterTrack.TransportPosition = MasterTrackTransportPosition;
            project.MasterTrack.Playing = MasterTrackPlaying;
            project.MasterTrack.BPM = MasterTrackBpm;

            string projectAsJson = JsonConvert.SerializeObject(project);

            return projectAsJson;
        }
    }
}