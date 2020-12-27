using NAudio.Wave;
using SoundTouch;
using SoundTouch.Net.NAudioSupport;

namespace NAudioWrapper
{
    class CustomStream : SoundTouchWaveStream
    {
        readonly WaveStream _sourceStream;

        public CustomStream(WaveStream sourceStream, bool enableLooping, double loopStart, double loopEnd, SoundTouchProcessor? processor = null) : base(sourceStream, processor)
        {
            this._sourceStream = sourceStream;
            this.EnableLooping = enableLooping;

            _loopStart = 0;
            _loopEnd = sourceStream.Length;
        }

        /*public CustomStream(SoundTouchWaveStream source)
        {
            sourceStream = source;
        }*/

        public bool EnableLooping = false;
        private long _loopStart;
        private long _loopEnd;

        public void SetLoopStart(double time)
        {
            _loopStart = (long) (_sourceStream.Length * time / _sourceStream.TotalTime.TotalSeconds);
        }
        public void SetLoopEnd(double time)
        {
            _loopEnd = (long) (_sourceStream.Length * time / _sourceStream.TotalTime.TotalSeconds);
        }

        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

        public override long Length => long.MaxValue / 32;

        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        public override bool HasData(int count)
        {
            return true; // infinite loop
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            while (read < count)
            {
                int required = count - read;
                int readThisTime = _sourceStream.Read(buffer, offset + read, required);
                if (readThisTime < required && EnableLooping)
                {
                    _sourceStream.Position = _loopStart;
                }

                if (_sourceStream.Position >= _loopEnd && EnableLooping)
                {
                    _sourceStream.Position = _loopStart;
                }

                if (_sourceStream.Position < _loopStart)
                {
                    _sourceStream.Position = _loopStart;
                }

                read += readThisTime;
            }
            return read;
        }

        protected override void Dispose(bool disposing)
        {
            _sourceStream.Dispose();
            base.Dispose(disposing);
        }

        
    }
}
