using System;
using NDatabase.Core.Session;
using NDatabase.Exceptions;
using NDatabase.IO;
using NDatabase.Meta;

namespace NDatabase.Core.Engine
{
    /// <summary>
    ///   Class that knows how to read/write all language native types : byte, char, String, int, long,....
    /// </summary>
    internal sealed class FileSystemInterface : IFileSystemInterface
    {
        private const byte ReservedSpace = 128;

        private static readonly int IntSizeX2 = OdbType.Integer.Size * 2;

        private readonly IDbIdentification _fileIdentification;
        private readonly ISession _session;

        private IMultiBufferedFileIO _io;

        public FileSystemInterface(IDbIdentification fileIdentification, int bufferSize, ISession session)
        {
            fileIdentification.EnsureDirectories();
            
            _fileIdentification = fileIdentification;
            _io = fileIdentification.GetIO(bufferSize);
            _session = session;
        }

        #region IFileSystemInterface Members

        public void Flush()
        {
            _io.FlushAll();
        }

        public long GetPosition()
        {
            return _io.CurrentPosition;
        }

        public long GetLength()
        {
            return _io.Length;
        }

        public void SetWritePositionNoVerification(long position, bool writeInTransacation)
        {
            _io.SetCurrentWritePosition(position);

            if (writeInTransacation)
                _session.GetTransaction().SetWritePosition(position);
        }

        public void SetWritePosition(long position, bool writeInTransacation)
        {
            if (position < StorageEngineConstant.DatabaseHeaderProtectedZoneSize)
            {
                if (IsWritingInWrongPlace(position))
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.InternalError.AddParameter(
                            string.Concat("Trying to write in Protected area at position ", position.ToString())));
                }
            }

            _io.SetCurrentWritePosition(position);
            if (writeInTransacation)
                _session.GetTransaction().SetWritePosition(position);
        }

        public void SetReadPosition(long position)
        {
            _io.SetCurrentReadPosition(position);
        }

        public long GetAvailablePosition()
        {
            return _io.Length;
        }

        public void EnsureSpaceFor(OdbType type)
        {
            EnsureSpaceFor(1, type);
        }

        public void WriteByte(byte i, bool writeInTransaction)
        {
            var bytes = new[] { i };

            if (!writeInTransaction)
            {
                _io.WriteByte(i);
            }
            else
            {
                _session.GetTransaction().ManageWriteAction(_io.CurrentPosition, bytes);
                EnsureSpaceFor(OdbType.Byte);
            }
        }

        public byte ReadByte()
        {
            return _io.ReadByte();
        }

        public void WriteSByte(sbyte i, bool writeInTransaction)
        {
            var asByte = unchecked((byte)i);
            var bytes = new[] { asByte };

            if (!writeInTransaction)
            {
                _io.WriteByte(asByte);
            }
            else
            {
                _session.GetTransaction().ManageWriteAction(_io.CurrentPosition, bytes);
                EnsureSpaceFor(OdbType.SByte);
            }
        }

        public sbyte ReadSByte()
        {
            var i = _io.ReadByte();

            var asSByte = unchecked((sbyte)i);

            return asSByte;
        }

        public void WriteBytes(byte[] bytes, bool writeInTransaction)
        {
            if (!writeInTransaction)
            {
                _io.WriteBytes(bytes);
            }
            else
            {
                _session.GetTransaction().ManageWriteAction(_io.CurrentPosition, bytes);
                EnsureSpaceFor(bytes.Length, OdbType.Byte);
            }
        }

        public byte[] ReadBytes(int length)
        {
            var currentPosition = _io.CurrentPosition;
            var bytes = _io.ReadBytes(length);
            var byteCount = bytes.Length;

            if (byteCount != length)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.FileInterfaceReadError.AddParameter(length).AddParameter(currentPosition).AddParameter(
                        byteCount));
            }

            return bytes;
        }

        public void WriteChar(char c, bool writeInTransaction)
        {
            WriteValue(c, writeInTransaction, ByteArrayConverter.CharToByteArray, OdbType.Character);
        }

        private byte[] ReadCharBytes()
        {
            return _io.ReadBytes(OdbType.Character.Size);
        }

        public char ReadChar()
        {
            return ByteArrayConverter.ByteArrayToChar(ReadCharBytes());
        }

        public void WriteShort(short s, bool writeInTransaction)
        {
            WriteValue(s, writeInTransaction, ByteArrayConverter.ShortToByteArray, OdbType.Short);
        }

        private byte[] ReadShortBytes()
        {
            return _io.ReadBytes(OdbType.Short.Size);
        }

        public short ReadShort()
        {
            return ByteArrayConverter.ByteArrayToShort(ReadShortBytes());
        }

        public void WriteUShort(ushort s, bool writeInTransaction)
        {
            WriteValue(s, writeInTransaction, ByteArrayConverter.UShortToByteArray, OdbType.UShort);
        }

        private byte[] ReadUShortBytes()
        {
            return _io.ReadBytes(OdbType.UShort.Size);
        }

        public ushort ReadUShort()
        {
            return ByteArrayConverter.ByteArrayToUShort(ReadUShortBytes());
        }

        public void WriteInt(int i, bool writeInTransaction)
        {
            WriteValue(i, writeInTransaction, ByteArrayConverter.IntToByteArray, OdbType.Integer);
        }

        private byte[] ReadIntBytes()
        {
            return _io.ReadBytes(OdbType.Integer.Size);
        }

        public int ReadInt()
        {
            return ByteArrayConverter.ByteArrayToInt(ReadIntBytes());
        }

        public void WriteUInt(uint i, bool writeInTransaction)
        {
            WriteValue(i, writeInTransaction, ByteArrayConverter.UIntToByteArray, OdbType.UInteger);
        }

        private byte[] ReadUIntBytes()
        {
            return _io.ReadBytes(OdbType.UInteger.Size);
        }

        public uint ReadUInt()
        {
            return ByteArrayConverter.ByteArrayToUInt(ReadUIntBytes());
        }

        public void WriteLong(long i, bool writeInTransaction)
        {
            WriteValue(i, writeInTransaction, ByteArrayConverter.LongToByteArray, OdbType.Long);
        }

        private byte[] ReadLongBytes()
        {
            return _io.ReadBytes(OdbType.Long.Size);
        }

        public long ReadLong()
        {
            return ByteArrayConverter.ByteArrayToLong(ReadLongBytes());
        }

        public void WriteULong(ulong i, bool writeInTransaction)
        {
            WriteValue(i, writeInTransaction, ByteArrayConverter.ULongToByteArray, OdbType.ULong);
        }

        private byte[] ReadULongBytes()
        {
            return _io.ReadBytes(OdbType.ULong.Size);
        }

        public ulong ReadULong()
        {
            return ByteArrayConverter.ByteArrayToULong(ReadULongBytes());
        }

        public void WriteFloat(float f, bool writeInTransaction)
        {
            WriteValue(f, writeInTransaction, ByteArrayConverter.FloatToByteArray, OdbType.Float);
        }

        private byte[] ReadFloatBytes()
        {
            return _io.ReadBytes(OdbType.Float.Size);
        }

        public float ReadFloat()
        {
            return ByteArrayConverter.ByteArrayToFloat(ReadFloatBytes());
        }

        public void WriteDouble(double d, bool writeInTransaction)
        {
            WriteValue(d, writeInTransaction, ByteArrayConverter.DoubleToByteArray, OdbType.Double);
        }

        private byte[] ReadDoubleBytes()
        {
            return _io.ReadBytes(OdbType.Double.Size);
        }

        public double ReadDouble()
        {
            return ByteArrayConverter.ByteArrayToDouble(ReadDoubleBytes());
        }

        public void WriteBigDecimal(Decimal d, bool writeInTransaction)
        {
            var bytes = ByteArrayConverter.DecimalToByteArray(d);

            if (!writeInTransaction)
            {
                _io.WriteBytes(bytes);
            }
            else
            {
                _session.GetTransaction().ManageWriteAction(_io.CurrentPosition, bytes);
                EnsureSpaceFor(bytes.Length, OdbType.Decimal);
            }
        }

        private byte[] ReadBigDecimalBytes()
        {
            return _io.ReadBytes(OdbType.Decimal.Size);
        }

        public Decimal ReadBigDecimal()
        {
            return ByteArrayConverter.ByteArrayToDecimal(ReadBigDecimalBytes());
        }

        public void WriteDate(DateTime d, bool writeInTransaction)
        {
            WriteValue(d, writeInTransaction, ByteArrayConverter.DateToByteArray, OdbType.Date);
        }

        private byte[] ReadDateBytes()
        {
            return _io.ReadBytes(OdbType.Date.Size);
        }

        public DateTime ReadDate()
        {
            return ByteArrayConverter.ByteArrayToDate(ReadDateBytes());
        }

        public void WriteString(string s, bool writeInTransaction)
        {
            WriteString(s, writeInTransaction, -1);
        }

        public void WriteString(string s, bool writeInTransaction, int totalSpace)
        {
            var bytes = ByteArrayConverter.StringToByteArray(s, totalSpace);
            
            if (!writeInTransaction)
            {
                _io.WriteBytes(bytes);
            }
            else
            {
                _session.GetTransaction().ManageWriteAction(_io.CurrentPosition, bytes);
                EnsureSpaceFor(bytes.Length, OdbType.String);
            }
        }

        private byte[] ReadStringBytes()
        {
            var sizeBytes = _io.ReadBytes(IntSizeX2);
            var totalSize = ByteArrayConverter.ByteArrayToInt(sizeBytes);

            // Use offset of int size to read real size
            var stringSize = ByteArrayConverter.ByteArrayToInt(sizeBytes, OdbType.Integer.Size);
            var bytes = ReadBytes(stringSize);

            // Reads extra bytes
            ReadBytes(totalSize - stringSize);

            var bytes2 = new byte[stringSize + IntSizeX2];

            for (var i = 0; i < IntSizeX2; i++)
                bytes2[i] = sizeBytes[i];

            for (var i = 0; i < bytes.Length; i++)
                bytes2[i + 8] = bytes[i];

            return bytes2;
        }

        public string ReadString()
        {
            return ByteArrayConverter.ByteArrayToString(ReadStringBytes());
        }

        public void WriteBoolean(bool b, bool writeInTransaction)
        {
            WriteValue(b, writeInTransaction, ByteArrayConverter.BooleanToByteArray, OdbType.Boolean);
        }

        private byte[] ReadBooleanBytes()
        {
            return _io.ReadBytes(OdbType.Boolean.Size);
        }

        public bool ReadBoolean()
        {
            return ByteArrayConverter.ByteArrayToBoolean(ReadBooleanBytes());
        }

        public void Close()
        {
            _io.Close();
            _io = null;
        }

        public IDbIdentification GetFileIdentification()
        {
            return _fileIdentification;
        }

        #endregion

        /// <summary>
        ///   Writing at position &lt; DATABASE_HEADER_PROTECTED_ZONE_SIZE is writing in ODB Header place.
        /// </summary>
        /// <remarks>
        ///   Writing at position &lt; DATABASE_HEADER_PROTECTED_ZONE_SIZE is writing in ODB Header place. 
        ///   Here we check the positions where the writing is done. 
        ///   Search for 'page format' in ODB wiki to understand the positions
        /// </remarks>
        /// <param name="position"> </param>
        /// <returns> </returns>
        private static bool IsWritingInWrongPlace(long position)
        {
            if (position < StorageEngineConstant.DatabaseHeaderProtectedZoneSize)
            {
                var size = StorageEngineConstant.DatabaseHeaderPositions.Length;
                for (var i = 0; i < size; i++)
                {
                    if (position == StorageEngineConstant.DatabaseHeaderPositions[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        private bool PointerAtTheEndOfTheFile()
        {
            return _io.CurrentPosition == _io.Length;
        }

        /// <summary>
        ///   Reserve space in the file when it is at the end of the file Used in transaction mode where real write will happen later
        /// </summary>
        /// <param name="quantity"> The number of object to reserve space for </param>
        /// <param name="type"> The type of the object to reserve space for </param>
        private void EnsureSpaceFor(long quantity, OdbType type)
        {
            var space = type.Size * quantity;

            // We are in transaction mode - do not write just reserve space if
            // necessary
            // ensure space will be available when applying transaction
            if (PointerAtTheEndOfTheFile())
            {
                if (space != 1)
                    _io.SetCurrentWritePosition(_io.CurrentPosition + space - 1);

                _io.WriteByte(ReservedSpace);
            }
            else
            {
                // DLogger.debug("Reserving " + space + " bytes (" + quantity +
                // " " + type.getName() + ")");
                // We must simulate the move
                _io.SetCurrentWritePosition(_io.CurrentPosition + space);
            }
        }

        private void WriteValue<TValue>(TValue value, bool writeInTransaction, Func<TValue, Byte[]> convert, OdbType odbType)
        {
            var bytes = convert(value);

            if (!writeInTransaction)
            {
                _io.WriteBytes(bytes);
            }
            else
            {
                _session.GetTransaction().ManageWriteAction(_io.CurrentPosition, bytes);
                EnsureSpaceFor(odbType);
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
