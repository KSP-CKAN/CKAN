using System;
using System.IO;

// https://learn.microsoft.com/en-us/archive/msdn-magazine/2006/december/net-matters-deserialization-progress-and-more

namespace CKAN
{
    public class ReadProgressStream : ContainerStream
    {
        public ReadProgressStream(Stream stream, IProgress<long> progress)
            : base(stream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("stream");
            }
            this.progress = progress;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int amountRead = base.Read(buffer, offset, count);
            if (progress != null)
            {
                long newProgress = Position;
                if (newProgress > lastProgress)
                {
                    progress?.Report(newProgress);
                    lastProgress = newProgress;
                }
            }
            return amountRead;
        }

        private readonly IProgress<long> progress;
        private long lastProgress = 0;
    }

    public abstract class ContainerStream : Stream
    {
        protected ContainerStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            inner = stream;
        }

        protected Stream ContainedStream => inner;

        public override bool CanRead  => inner.CanRead;
        public override bool CanSeek  => inner.CanSeek;
        public override bool CanWrite => inner.CanWrite;
        public override long Length   => inner.Length;
        public override long Position
        {
            get => inner.Position;
            set
            {
                inner.Position = value;
            }
        }
        public override void Flush()
        {
            inner.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count)
            => inner.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count)
        {
            inner.Write(buffer, offset, count);
        }
        public override void SetLength(long length)
        {
            inner.SetLength(length);
        }
        public override long Seek(long offset, SeekOrigin origin)
            => inner.Seek(offset, origin);

        private readonly Stream inner;
    }
}
