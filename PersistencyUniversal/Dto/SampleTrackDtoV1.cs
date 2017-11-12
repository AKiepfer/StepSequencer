namespace PersistencyUniversal.Dto
{
    public class SampleTrackDtoV1
    {
        public double Pan { get; set; }
        public double Pitch { get; set; }
        public double Volume { get; set; }
        public TrackTypeDtoV1 TrackType { get; set; }
        public bool[] PlayAt { get; set; }
        public SampleFileDtoV1 File { get; set; }
    }
}