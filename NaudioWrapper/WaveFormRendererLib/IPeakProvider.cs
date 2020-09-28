using NAudio.Wave;

namespace NAudioWrapper.WaveFormRendererLib
{
    public interface IPeakProvider
    {
        void Init(ISampleProvider reader, int samplesPerPixel);
        PeakInfo GetNextPeak();
    }
}