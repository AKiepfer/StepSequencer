using System.IO;
using SharpDX;
using SharpDX.XAPO;

namespace ManagedAudioEngineUniversal.Effects
{
    public class WaveFileWriterEffect : AudioProcessorBase<WavWriterEffect>, AudioProcessor
    {
        private readonly DataStream _dataStream = new DataStream(50000000, true, true);

        public WaveFileWriterEffect()
        {
            RegistrationProperties = new RegistrationProperties
                                     {
                                         Clsid = Utilities.GetGuidFromType(typeof (WaveFileWriterEffect)),
                                         CopyrightInfo = "9cubes, Andreas Kiepfer",
                                         FriendlyName = "WavWriter",
                                         MaxInputBufferCount = 1,
                                         MaxOutputBufferCount = 1,
                                         MinInputBufferCount = 1,
                                         MinOutputBufferCount = 1,
                                         Flags = PropertyFlags.Default
                                     };
        }

        public bool IsRecording { get; set; }

        public override void Process(BufferParameters[] inputProcessParameters,
            BufferParameters[] outputProcessParameters, bool isEnabled)
        {
            if (inputProcessParameters[0].BufferFlags == BufferFlags.None || !isEnabled || _dataStream == null ||
                !IsRecording)
                return;

            int size = inputProcessParameters[0].ValidFrameCount*InputFormatLocked.BlockAlign;
                
            _dataStream.Write(inputProcessParameters[0].Buffer, 0, size);
        }

        public void Stop(Stream destinationFile)
        {
            IsRecording = false;

            //var wavWriter = new WavWriter(destinationFile);

            //wavWriter.Begin(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));

            //wavWriter.AppendData(new DataPointer(_dataStream.DataPointer, (int)_dataStream.Position));

            //wavWriter.End();

            _dataStream.Dispose();
        }

        public void Start()
        {
            IsRecording = true;
        }
    }
}