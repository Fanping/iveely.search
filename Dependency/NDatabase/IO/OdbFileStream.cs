using System;
using System.IO;
using NDatabase.Exceptions;

namespace NDatabase.IO
{
    internal sealed class OdbFileStream : IOdbStream
    {
        private readonly object _lockObject = new object();
        private bool _disposed;

        private FileStream _fileAccess;
        private long _position;

        internal OdbFileStream(string wholeFileName)
        {
            try
            {
                _fileAccess = OdbFileManager.GetStream(wholeFileName);
                _disposed = false;
            }
            catch (OdbRuntimeException)
            {
                throw;
            }
            catch (IOException e)
            {
                throw new OdbRuntimeException(NDatabaseError.FileNotFoundOrItIsAlreadyUsed.AddParameter(wholeFileName), e);
            }
            catch (Exception ex)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.InternalError.AddParameter("Error during opening FileStream"), ex);
            }
        }

        #region IO Members

        /// <summary>
        ///     Gets the length in bytes of the stream
        /// </summary>
        public long Length
        {
            get { return _fileAccess.Length; }
        }

        /// <summary>
        ///     Sets the current position of this stream to the given value
        /// </summary>
        /// <param name="position">offset</param>
        public void SetPosition(long position)
        {
            if (position < 0)
                throw new OdbRuntimeException(NDatabaseError.NegativePosition.AddParameter(position));

            _position = position;
        }

        public void Write(byte b)
        {
            try
            {
                Seek(_position);
                _fileAccess.WriteByte(b);
                _position = _fileAccess.Position;
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
                _fileAccess.Write(buffer, 0, size);
                _position = _fileAccess.Position;
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
                var data = _fileAccess.ReadByte();
                if (data == -1)
                    throw new IOException("End of file");

                _position = _fileAccess.Position;

                return (byte) data;
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
                var read = _fileAccess.Read(buffer, 0, size);
                _position = _fileAccess.Position;
                return read;
            }
            catch (IOException e)
            {
                throw new OdbRuntimeException(e, "Error while reading an array of byte");
            }
        }

        private void Seek(long position)
        {
            try
            {
                if (position < 0)
                    throw new OdbRuntimeException(NDatabaseError.NegativePosition.AddParameter(position));

                _fileAccess.Seek(position, SeekOrigin.Begin);
            }
            catch (IOException e)
            {
                long l = -1;
                try
                {
                    l = _fileAccess.Length;
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

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (_lockObject)
            {
                if (_disposed)
                    return;

                if (disposing && _fileAccess != null)
                    _fileAccess.Dispose();

                _fileAccess = null;
                _disposed = true;
            }
        }

        ~OdbFileStream()
        {
            Dispose(false);
        }
    }
}