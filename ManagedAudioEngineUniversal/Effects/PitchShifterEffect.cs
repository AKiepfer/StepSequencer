using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.XAPO;

namespace ManagedAudioEngineUniversal.Effects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ModulatorParam
    {
    }

    public class PitchShifterEffect : AudioProcessorBase<ModulatorParam>, AudioProcessor
    {
        private readonly SmbPitchShift leftPitcher = new SmbPitchShift();
        private SmbPitchShift _rightPitcher = new SmbPitchShift();
        private float _stretching;

        public PitchShifterEffect()
        {
            RegistrationProperties = new RegistrationProperties
                                     {
                                         Clsid = Utilities.GetGuidFromType(typeof (PitchShifterEffect)),
                                         CopyrightInfo = "Copyright",
                                         FriendlyName = "Modulator",
                                         MaxInputBufferCount = 1,
                                         MaxOutputBufferCount = 1,
                                         MinInputBufferCount = 1,
                                         MinOutputBufferCount = 1,
                                         Flags = PropertyFlags.Default
                                     };

            _stretching = 1.0f;
        }

        public float Stretching
        {
            get { return _stretching; }
            set
            {
                lock (this)
                {
                    _stretching = value;
                }
            }
        }

        public override void Process(BufferParameters[] inputProcessParameters,
            BufferParameters[] outputProcessParameters, bool isEnabled)
        {
            if (inputProcessParameters[0].BufferFlags != BufferFlags.Valid || !isEnabled || _stretching == 1.0f)
                return;

            int frameCount = inputProcessParameters[0].ValidFrameCount;
            var input = new DataStream(inputProcessParameters[0].Buffer,
                frameCount*InputFormatLocked.BlockAlign,
                true,
                true);
            var output = new DataStream(outputProcessParameters[0].Buffer,
                frameCount*InputFormatLocked.BlockAlign,
                true,
                true);

            var left = new List<float>();
            var right = new List<float>();

            for (int i = 0;
                i < frameCount;
                i++)
            {
                left.Add(input.Read<float>());
                right.Add(input.Read<float>());
            }

            float[] leftArray = left.ToArray();
            float[] rightArray = right.ToArray();

            var leftOut = new float[frameCount];
            var rightOut = new float[frameCount];

            lock (this)
            {
                leftPitcher.smbPitchShift(_stretching,
                    frameCount,
                    1024,
                    32,
                    InputFormatLocked.SampleRate,
                    leftArray,
                    leftOut);


                for (int i = 0;
                    i < frameCount;
                    i++)
                {
                    output.Write(leftOut[i]);
                    output.Write(leftOut[i]);
                }
            }

            ////Console.WriteLine("Process is called every: " + timer.ElapsedMilliseconds);
            //timer.Reset(); timer.Start();

            // Use a linear ramp on intensity in order to avoir too much glitches

            //float nextIntensity = Intensity;
            //for (int i = 0; i < frameCount; i++, _counter++)
            //{
            //    float left = input.Read<float>();
            //    float right = input.Read<float>();
            //    float intensity = (nextIntensity - lastIntensity) * (float)i / frameCount + lastIntensity;
            //    double vibrato = Math.Cos(2 * Math.PI * intensity * 400 * _counter / InputFormatLocked.SampleRate);
            //    output.Write((float)vibrato * left);
            //    output.Write((float)vibrato * right);
            //}
            //lastIntensity = nextIntensity;
        }
    }
}