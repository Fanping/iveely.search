using System;
using System.Collections.Generic;
using System.Text;
using NDatabase.Api;
using NDatabase.Core;
using NDatabase.Core.Engine;
using NDatabase.Exceptions;
using NDatabase.Meta;
using NDatabase.Tool;

namespace NDatabase.Transaction
{
    /// <summary>
    ///   The WriteAction class is the description of a Write operation that will be applied to the main database file when committing.
    /// </summary>
    /// <remarks>
    ///   The WriteAction class is the description of a Write operation that will be applied to the main database file when committing.
    ///   All operations(writes) that can not be written to the database file before committing, 
    ///   pointers (for example) are stored in WriteAction objects. The transaction keeps track of all these WriteActions. 
    ///   When committing, the transaction apply each WriteAction to the engine database file.
    /// </remarks>
    internal sealed class WriteAction
    {
        private readonly long _position;
        private IList<byte[]> _listOfBytes;

        private int _size;

        internal WriteAction(long position, byte[] bytes = null)
        {
            _position = position;
            _listOfBytes = new List<byte[]>(20);

            if (bytes == null)
                return;

            _listOfBytes.Add(bytes);
            _size = bytes.Length;
        }

        internal void AddBytes(byte[] bytes)
        {
            _listOfBytes.Add(bytes);
            _size += bytes.Length;
        }

        internal void PersistMeTo(IFileSystemInterface fsi)
        {
            var sizeOfLong = OdbType.Long.Size;
            var sizeOfInt = OdbType.Integer.Size;

            // build the full byte array to write once
            var bytes = new byte[sizeOfLong + sizeOfInt + _size];

            var bytesOfPosition = ByteArrayConverter.LongToByteArray(_position);
            var bytesOfSize = ByteArrayConverter.IntToByteArray(_size);
            for (var i = 0; i < sizeOfLong; i++)
                bytes[i] = bytesOfPosition[i];

            var offset = sizeOfLong;
            for (var i = 0; i < sizeOfInt; i++)
            {
                bytes[offset] = bytesOfSize[i];
                offset++;
            }

            foreach (var tmp in _listOfBytes)
            {
                Buffer.BlockCopy(tmp, 0, bytes, offset, tmp.Length);
                offset += tmp.Length;
            }

            fsi.WriteBytes(bytes, false);
        }

        internal void ApplyTo(IFileSystemInterface fsi)
        {
            fsi.SetWritePosition(_position, false);
            
            foreach (var bytes in _listOfBytes)
                fsi.WriteBytes(bytes, false);
        }

        internal bool IsEmpty()
        {
            return _listOfBytes == null || _listOfBytes.Count == 0;
        }

        internal void Clear()
        {
            _listOfBytes.Clear();
            _listOfBytes = null;
        }

        internal static WriteAction Read(IFileSystemInterface fsi)
        {
            try
            {
                var position = fsi.ReadLong();
                var size = fsi.ReadInt();
                var bytes = fsi.ReadBytes(size);
                var writeAction = new WriteAction(position, bytes);

                if (OdbConfiguration.IsLoggingEnabled())
                    DLogger.Debug(string.Format("Transaction WriteAction: Loading Write Action at {0} => {1}", fsi.GetPosition(), writeAction));

                return writeAction;
            }
            catch (OdbRuntimeException)
            {
                DLogger.Error(string.Format("Transaction WriteAction: error reading write action at position {0}", fsi.GetPosition()));
                throw;
            }
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("position=").Append(_position);
            var bytes = new StringBuilder();

            if (_listOfBytes != null)
            {
                foreach (var bytesToWrite in _listOfBytes)
                    bytes.Append(ByteArrayToString(bytesToWrite));

                buffer.Append(" | bytes=[").Append(bytes).Append("] & size=" + _size);
            }
            else
                buffer.Append(" | bytes=null & size=").Append(_size);

            return buffer.ToString();
        }

        private static string ByteArrayToString(IEnumerable<byte> bytes)
        {
            var buffer = new StringBuilder();
            foreach (var value in bytes)
                buffer.Append((int)value).Append(" ");

            return buffer.ToString();
        }
    }
}
