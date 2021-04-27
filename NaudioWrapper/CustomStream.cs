using NAudio.Wave;
using SoundTouch;
using SoundTouch.Net.NAudioSupport;
using System;
using System.IO;

namespace NAudioWrapper
{
    /*
     * SoundTouchWaveStream with loop support
     */
    class CustomStream : SoundTouchWaveStream
    {
        private WaveStream? _sourceStream;

        public CustomStream(WaveStream sourceStream, bool enableLooping, double loopStart, double loopEnd, SoundTouchProcessor? processor = null) : base(sourceStream, processor)
        {
            this._sourceStream = sourceStream;
            this.EnableLooping = enableLooping;

            _loopStart = 0;
            _loopEnd = sourceStream.Length;
        }

        public bool EnableLooping = false;
        private long _loopStart;
        private long _loopEnd;

        public void SetLoopStart(double time)
        {
            if(_sourceStream != null)
            {
                _loopStart = (long)(_sourceStream.Length * time / _sourceStream.TotalTime.TotalSeconds);
            }
        }
        public void SetLoopEnd(double time)
        {
            if (_sourceStream != null)
            {
                _loopEnd = (long)(_sourceStream.Length * time / _sourceStream.TotalTime.TotalSeconds);
            }
        }

        public override long Length => EnableLooping ? long.MaxValue / 32 : base.Length;

        public override bool HasData(int count)
        {
            // infinite data when looping
            return EnableLooping || base.HasData(count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (EnableLooping && (_sourceStream?.Position < _loopStart || _sourceStream?.Position >= _loopEnd))
            {
                _sourceStream.Position = _loopStart;
            }
            return base.Read(buffer, offset, count);
        }
    }
}
