using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerForDrummers.View.Controls.Partition
{
    /// < summary > 
    /// Wrapper class of stream 
    /// </ summary > 
    /// < remarks > 
    /// Dereference internal stream during Dispose 
    /// </ remarks > 
    public class WrappingStream: Stream
    {
        Stream m_streamBase;

        public WrappingStream (Stream streamBase)
        {
            if (streamBase == null )
            {
                throw  new ArgumentNullException ( "streamBase" );
            }
            m_streamBase = streamBase; // Keep the passed Stream as an internal stream
        }

        public override bool CanRead => m_streamBase.CanRead;

        public override bool CanSeek => m_streamBase.CanSeek;

        public override bool CanWrite => m_streamBase.CanWrite;

        public override long Length => m_streamBase.Length;

        public override long Position { get => m_streamBase.Position; set => m_streamBase.Position = value; }

        public override void Flush()
        {
            m_streamBase.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_streamBase.Read(buffer, offset, count);
        }

        // Override the method of Stream class and just call the same method of internal stream as it is 
        public  override Task < int > ReadAsync ( byte [] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            ThrowIfDisposed ();
            return m_streamBase.ReadAsync (buffer, offset, count, cancellationToken);
        }
        public  new Task < int > ReadAsync ( byte [] buffer, int offset, int count)
        {
            ThrowIfDisposed ();
            return m_streamBase.ReadAsync (buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_streamBase.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            m_streamBase.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            m_streamBase.Write(buffer, offset, count);
        }

        //... (Omitted) ...

        protected  override  void Dispose ( bool disposing)
        {
            if (disposing)
            {
                m_streamBase.Dispose ();
                m_streamBase = null ;   // After dispose, make the internal stream null and remove the reference
            }
            base .Dispose (disposing);
        }
        private  void ThrowIfDisposed ()
        {
            if (m_streamBase == null )
            {
                throw  new ObjectDisposedException (GetType (). Name);
            }
        }
    }
}
