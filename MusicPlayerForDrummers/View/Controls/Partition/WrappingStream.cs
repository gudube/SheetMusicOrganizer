using System;
using System.IO;
using System.Threading.Tasks;

namespace MusicPlayerForDrummers.View.Controls.Partition
{
    public class WrappingStream: Stream
    {
        private Stream? _streamBase;

        public WrappingStream (Stream streamBase)
        {
            if (streamBase == null )
            {
                throw new ArgumentNullException (nameof(streamBase));
            }
            this._streamBase = streamBase; // Keep the passed Stream as an internal stream
        }

        public override bool CanRead => _streamBase?.CanRead ?? false;

        public override bool CanSeek => _streamBase?.CanSeek ?? false;

        public override bool CanWrite => _streamBase?.CanWrite ?? false;

        public override long Length => _streamBase?.Length ?? 0;

        public override long Position { get => _streamBase?.Position ?? 0;
            set {
                if (_streamBase != null)
                    _streamBase.Position = value;
            }
        }

        public override void Flush()
        {
            _streamBase?.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _streamBase?.Read(buffer, offset, count) ?? 0;
        }

        // Override the method of Stream class and just call the same method of internal stream as it is 
        public override Task < int > ReadAsync ( byte [] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            if (_streamBase == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            return _streamBase.ReadAsync(buffer, offset, count, cancellationToken);
        }
        public new Task < int > ReadAsync ( byte [] buffer, int offset, int count)
        {
            if (_streamBase == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            return _streamBase.ReadAsync(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _streamBase?.Seek(offset, origin) ?? 0L;
        }

        public override void SetLength(long value)
        {
            _streamBase?.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _streamBase?.Write(buffer, offset, count);
        }

        //... (Omitted) ...

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _streamBase?.Dispose ();
                _streamBase = null;   // After dispose, make the internal stream null and remove the reference
            }
            base.Dispose(disposing);
        }
    }
}
