namespace PersistencyUniversal.Dto
{
    public class MasterTrackDtoV1
    {
        public MasterTrackDtoV1()
        {
            Pan = 0.0;
            Pitch = 1.0;
            Volume = 0.8;
            Playing = false;
            BPM = 90.0;
        }

        public double Pan { get; set; }
        public double Pitch { get; set; }
        public double Volume { get; set; }
        public TrackTypeDtoV1 TrackType { get; set; }
        public int TransportPosition { get; set; }
        public bool Playing { get; set; }
        public double BPM { get; set; }
    }
}