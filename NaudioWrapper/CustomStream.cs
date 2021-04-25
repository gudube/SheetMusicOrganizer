using NAudio.Wave;
using SoundTouch;
using SoundTouch.Net.NAudioSupport;
using System;

namespace NAudioWrapper
{
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

        /*public CustomStream(SoundTouchWaveStream source)
        {
            sourceStream = source;
        }*/

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
        public override WaveFormat WaveFormat => _sourceStream?.WaveFormat ?? new WaveFormat();

        public override long Length => EnableLooping ? long.MaxValue / 32 : (_sourceStream?.Length ?? 0);

        public override long Position
        {
            get => _sourceStream?.Position ?? 0L;
            set {
                if (_sourceStream != null)
                    _sourceStream.Position = value;
            }
        }

        public override bool HasData(int count)
        {
            // infinite data when looping
            return EnableLooping || (_sourceStream?.HasData(count) ?? false);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                int read = 0;
                while (read < count && _sourceStream != null)
                {
                    int required = count - read;
                    int readThisTime = _sourceStream.Read(buffer, offset + read, required);

                    if (EnableLooping)
                    {
                        if (readThisTime < required || _sourceStream.Position >= _loopEnd ||
                            _sourceStream.Position < _loopStart)
                        {
                            _sourceStream.Position = _loopStart;
                        }
                    }

                    read += readThisTime;
                }
                return read;
            }
            catch(Exception ex)
            {
                // if(ex is NullReferenceException || ex is ObjectDisposedException)
                // {
                    //nothing to do, will be disposed
                    return 0;
                // }
                // throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _sourceStream?.Flush();
            _sourceStream?.Dispose();
            _sourceStream = null;
            base.Dispose(disposing);
        }

        
    }
}
