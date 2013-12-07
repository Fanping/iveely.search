using System;
using System.IO;
using NDatabase.Exceptions;

namespace NDatabase.IO
{
    internal sealed class OdbMemoryStream : IOdbStream
    {
        private MemoryStream _memoryStream;
        private long _position;

        internal OdbMemoryStream()
        {
            try
            {
                _memoryStream = new MemoryStream();
            }
            catch(Exception ex)
            {
                throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter("Error during opening MemoryStream"), ex);
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream
        /// </summary>
        public long Length
        {
            get { return _memoryStream.Length; }
        }

        /// <summary>
        ///  Sets the current position of this stream to the given value
        /// </summary>
        /// <param name="position">offset</param>
        public void SetPosition(long position)
        {
            if (position < 0)
                throw new OdbRuntimeException(NDatabaseError.NegativePosition.AddParameter(position));

            _position = position;
        }

        private void Seek(long position)
        {
            try
            {
                if (position < 0)
                    throw new OdbRuntimeException(NDatabaseError.NegativePosition.AddParameter(position));

                _memoryStream.Seek(position, SeekOrigin.Begin);
            }
            catch (IOException e)
            {
                long l = -1;
                try
                {
                    l = _memoryStream.Length;
                }
                catch (IOException)
                {
                }

                throw new OdbRuntimeException(NDatabaseError.GoToPosition.AddParameter(position).AddParameter(l), e);
            }
            catch (Exception ex)
            {
                var parameter = string.Concat("Error during seek operation, position: ", position.ToString());
                throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter(parameter), ex);
            }
        }

        public void Write(byte b)
        {
            try
            {
                Seek(_position);
                _memoryStream.WriteByte(b);
                _position = _memoryStream.Position;
            }
            catch (IOException e)
            {
                throw new OdbRuntimeException(e, "Error while writing a byte");
            }
        }

        public void Write(byte[] buffer, int size)
        {
            try
            {
                Seek(_position);
                _memoryStream.Write(buffer, 0, size);
                _position = _memoryStream.Position;
            }
            catch (IOException e)
            {
                throw new OdbRuntimeException(e, "Error while writing an array of byte");
            }
        }

        public int Read()
        {
            try
            {
                Seek(_position);
                var data = _memoryStream.ReadByte();
                if (data == -1)
                    throw new IOException("End of file");

                _position = _memoryStream.Position;

                return (byte)data;
            }
            catch (IOException e)
            {
                throw new OdbRuntimeException(e, "Error while reading a byte");
            }
        }

        public int Read(byte[] buffer, int size)
        {
            try
            {
                Seek(_position);
                var read = _memoryStream.Read(buffer, 0, size);
                _position = _memoryStream.Position;
                return read;
            }
            catch (IOException e)
            {
                throw new OdbRuntimeException(e, "Error while reading an array of byte");
            }
        }

        public void Dispose()
        {
            _memoryStream.Close();
            _memoryStream = null;
        }
    }
}