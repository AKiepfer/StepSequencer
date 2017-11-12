using System.Collections.Generic;

namespace PersistencyUniversal.Dto
{
    public class ProjectDtoV1
    {
        public ProjectDtoV1()
        {
            Version = 1;
        }

        public int Version { get; set; }
        public MasterTrackDtoV1 MasterTrack { get; set; }
        public IEnumerable<SampleTrackDtoV1> SampleTracks { get; set; }
    }
}