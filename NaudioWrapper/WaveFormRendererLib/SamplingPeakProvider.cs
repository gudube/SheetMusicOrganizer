using System;

namespace NAudioWrapper.WaveFormRendererLib
{
    public class SamplingPeakProvider : PeakProvider
    {
        private readonly int _sampleInterval;

        public SamplingPeakProvider(int sampleInterval) 
        {
            this._sampleInterval = sampleInterval;
        }

        public override PeakInfo GetNextPeak()
        {
            if(Provider == null || ReadBuffer == null)
                return new PeakInfo(0,0);

            var samplesRead = Provider.Read(ReadBuffer,0,ReadBuffer.Length);
            var max = 0.0f;
            var min = 0.0f;
            for (int x = 0; x < samplesRead; x += _sampleInterval)
            {
                max = Math.Max(max, ReadBuffer[x]);
                min = Math.Min(min, ReadBuffer[x]);
            }

            return new PeakInfo(min,max);
        }
    }
}