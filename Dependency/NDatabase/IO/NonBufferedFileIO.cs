using System.IO;
using NDatabase.Exceptions;
using NDatabase.Tool;

namespace NDatabase.IO
{
    internal sealed class NonBufferedFileIO : INonBufferedFileIO
    {
        private readonly string _wholeFileName;

        private IOdbStream _odbWriter;

        internal NonBufferedFileIO(string fileName)
        {
            CurrentPositionForDirectWrite = -1;

            _wholeFileName = fileName;
            _odbWriter = new OdbFileStream(_wholeFileName);
        }

        internal NonBufferedFileIO()
        {
            CurrentPositionForDirectWrite = -1;

            _odbWriter = new OdbMemoryStream();
        }

        #region INonBufferedFileIO Members

        public long Length
        {
            get { return _odbWriter.Length; }
        }

        /// <summary>
        ///   Current position for direct write to IO
        /// </summary>
        public long CurrentPositionForDirectWrite { get; private set; }

        public void Dispose()
        {
            CloseIO();
        }

        public void SetCurrentPosition(long currentPosition)
        {
            CurrentPositionForDirectWrite = currentPosition;
            GoToPosition(currentPosition);
        }

        private void GoToPosition(long position)
        {
            _odbWriter.SetPosition(position);
        }

        public void WriteByte(byte b)
        {
            GoToPosition(CurrentPositionForDirectWrite);
            _odbWriter.Write(b);
            CurrentPositionForDirectWrite++;
        }

        public byte[] ReadBytes(int size)
        {
            GoToPosition(CurrentPositionForDirectWrite);

            var bytes = new byte[size];
            var realSize = _odbWriter.Read(bytes, size);

            CurrentPositionForDirectWrite += realSize;
            return bytes;
        }

        public byte ReadByte()
        {
            GoToPosition(CurrentPositionForDirectWrite);

            var b = (byte) _odbWriter.Read();
            CurrentPositionForDirectWrite++;

            return b;
        }

        public void WriteBytes(byte[] bytes, int length)
        {
            GoToPosition(CurrentPositionForDirectWrite);
            _odbWriter.Write(bytes, length);
            CurrentPositionForDirectWrite += length;
        }

        public long Read(long position, byte[] buffer, int size)
        {
            GoToPosition(position);
            return _odbWriter.Read(buffer, size);
        }

        #endregion

        private void CloseIO()
        {
            try
            {
                _odbWriter.Dispose();
            }
            catch (IOException e)
            {
                DLogger.Error("NonBufferedFileIO" + e);
                throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter(e.Message), e);
            }

            _odbWriter = null;
        }
    }
}
