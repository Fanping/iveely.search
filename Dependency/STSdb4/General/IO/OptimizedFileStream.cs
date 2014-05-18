using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Iveely.General.IO
{
    /// <summary>
    /// An optimized FileStram - optimizes calls to Seek & Size methods
    /// The requirement is if the file is opened for writing, it is an exclusive.
    /// </summary>
    public class OptimizedFileStream : FileStream
    {
        private long length = long.MinValue;

        public OptimizedFileStream(string fileName, FileMode mode)
            : base(fileName, mode)
        {
        }

        public override long Position
        {
            get { return base.Position; }
            set
            {
                if (base.Position != value)
                    base.Position = value;
            }
        }

        public override void Write(byte[] array, int offset, int count)
        {
            try
            {
                base.Write(array, offset, count);

                if (Position > Length)
                    length = Position;
            }
            catch (Exception exc)
            {
                length = long.MinValue;
                throw exc;
            }
        }

        public override void WriteByte(byte value)
        {
            Write(new byte[] { value }, 0, 1);
        }

        public override long Length
        {
            get
            {
                if (length == long.MinValue)
                    length = base.Length;

                return length;
            }
        }

        public override void SetLength(long value)
        {
            try
            {
                base.SetLength(value);

                length = value;
            }
            catch (Exception exc)
            {
                length = long.MinValue;

                throw exc;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        if (offset != Position)
                            return base.Seek(offset, origin);
                    }
                    break;
                case SeekOrigin.Current:
                    {
                        if (offset != 0)
                            return base.Seek(offset, origin);
                    }
                    break;
                case SeekOrigin.End:
                    {
                        if (offset != Length - Position)
                            return base.Seek(offset, origin);
                    }
                    break;
            }

            return Position;
        }
    }
}
