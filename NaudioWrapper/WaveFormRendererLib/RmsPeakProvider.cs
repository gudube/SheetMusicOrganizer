using System;

namespace NAudioWrapper.WaveFormRendererLib
{
    public class RmsPeakProvider : PeakProvider
    {
        private readonly int _blockSize;

        public RmsPeakProvider(int blockSize)
        {
            this._blockSize = blockSize;
        }

        public override PeakInfo GetNextPeak()
        {
            if (Provider == null || ReadBuffer == null)
                return new PeakInfo(0,0);

            var samplesRead = Provider.Read(ReadBuffer, 0, ReadBuffer.Length);

            var max = 0.0f;
            for (int x = 0; x < samplesRead; x += _blockSize)
            {
                double total = 0.0;
                for (int y = 0; y < _blockSize && x + y < samplesRead; y++)
                {
                    total += ReadBuffer[x + y] * ReadBuffer[x + y];
                }
                var rms = (float) Math.Sqrt(total/_blockSize);

                max = Math.Max(max, rms);
            }

            return new PeakInfo(0 -max, max);
        }
    }
}