using System;
using System.Linq;

namespace NAudioWrapper.WaveFormRendererLib
{
    public class AveragePeakProvider : PeakProvider
    {
        private readonly float _scale;

        public AveragePeakProvider(float scale)
        {
            this._scale = scale;
        }

        public override PeakInfo GetNextPeak()
        {
            if (Provider == null || ReadBuffer == null)
                return new PeakInfo(0,0);

            var samplesRead = Provider.Read(ReadBuffer, 0, ReadBuffer.Length);
            var sum = (samplesRead == 0) ? 0 : ReadBuffer.Take(samplesRead).Select(Math.Abs).Sum();
            var average = sum/samplesRead;
            
            return new PeakInfo(average * (0 - _scale), average * _scale);
        }
    }
}