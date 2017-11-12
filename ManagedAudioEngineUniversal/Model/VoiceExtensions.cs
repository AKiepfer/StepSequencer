using System.Linq;
using SharpDX.XAudio2;

namespace ManagedAudioEngineUniversal.Model
{
    public static class VoiceExtensions
    {
        public static void SetPan(this Voice voice, double pan, int operationId = XAudio2.CommitNow)
        {
            try
            {
                if (voice == null)
                {
                    return;
                }

                // pan of -1.0 indicates all right speaker, 
                // 1.0 is all left speaker, 0.0 is split between left and right
                float left = 0.5f + (float) pan/2;
                float right = 0.5f - (float) pan/2;


                float[] outputMatrix = Enumerable.Range(0, voice.VoiceDetails.InputChannelCount * 2)
                    .Select(i => 0.0f)
                    .ToArray();


                //hard coded... always stereo

                switch (outputMatrix.Length)
                {
                    case 1:
                        outputMatrix[0] = 1.0f;
                        break;
                    case 2:
                        outputMatrix[0] = left;
                        outputMatrix[1] = right;
                        break;
                    case 4:
                        outputMatrix[0] = outputMatrix[1] = left;
                        outputMatrix[2] = outputMatrix[3] = right;
                        break;
                }

                voice.SetOutputMatrix(voice.VoiceDetails.InputChannelCount, 2, outputMatrix, operationId);
            }
            catch
            {
            }
        }
    }
}