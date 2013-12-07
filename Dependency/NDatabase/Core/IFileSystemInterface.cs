using System;
using NDatabase.IO;
using NDatabase.Meta;

namespace NDatabase.Core
{
    internal interface IFileSystemInterface : IDisposable
    {
        void Flush();

        long GetPosition();

        long GetLength();

        /// <summary>
        ///   Does the same thing than setWritePosition, but do not control write position
        /// </summary>
        /// <param name="position"> </param>
        /// <param name="writeInTransacation"> </param>
        void SetWritePositionNoVerification(long position, bool writeInTransacation);

        void SetWritePosition(long position, bool writeInTransacation);

        void SetReadPosition(long position);

        long GetAvailablePosition();

        void EnsureSpaceFor(OdbType type);

        void WriteByte(byte i, bool writeInTransaction);

        byte ReadByte();

        void WriteBytes(byte[] bytes, bool writeInTransaction);

        byte[] ReadBytes(int length);

        void WriteChar(char c, bool writeInTransaction);

        char ReadChar();

        void WriteShort(short s, bool writeInTransaction);

        short ReadShort();

        void WriteInt(int i, bool writeInTransaction);

        int ReadInt();

        void WriteLong(long i, bool writeInTransaction);

        long ReadLong();

        void WriteFloat(float f, bool writeInTransaction);

        float ReadFloat();

        void WriteDouble(double d, bool writeInTransaction);

        double ReadDouble();

        void WriteBigDecimal(Decimal d, bool writeInTransaction);

        Decimal ReadBigDecimal();

        void WriteDate(DateTime d, bool writeInTransaction);

        DateTime ReadDate();

        void WriteString(string s, bool writeInTransaction);

        void WriteString(string s, bool writeInTransaction, int totalSpace);

        string ReadString();

        void WriteBoolean(bool b, bool writeInTransaction);

        bool ReadBoolean();

        void Close();

        /// <returns> Returns the parameters. </returns>
        IDbIdentification GetFileIdentification();

        void WriteUShort(ushort s, bool writeInTransaction);
        
        ushort ReadUShort();
        
        void WriteUInt(uint i, bool writeInTransaction);
        
        uint ReadUInt();
        
        void WriteULong(ulong i, bool writeInTransaction);
        
        ulong ReadULong();
        
        void WriteSByte(sbyte i, bool writeInTransaction);
        
        sbyte ReadSByte();
    }
}
