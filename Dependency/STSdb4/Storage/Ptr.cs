using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Iveely.STSdb4.Storage
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Ptr : IEquatable<Ptr>, IComparable<Ptr>
    {
        public static readonly Ptr NULL = new Ptr(0, 0);

        public long Position;
        public long Size;

        public const int SIZE = sizeof(long) + sizeof(long);

        [DebuggerStepThrough]
        public Ptr(long position, long size)
        {
            Position = position;
            Size = size;
        }

        #region IEquatable<Ptr> Members

        public bool Equals(Ptr other)
        {
            return Position == other.Position &&
                Size == other.Size;
        }

        #endregion

        #region IComparable<Ptr> Members

        public int CompareTo(Ptr other)
        {
            return Position.CompareTo(other.Position);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj is Ptr)
                return this.Equals((Ptr)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Size.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", Position, Size);
        }

        public static bool operator ==(Ptr ptr1, Ptr ptr2)
        {
            return ptr1.Equals(ptr2);
        }

        public static bool operator !=(Ptr ptr1, Ptr ptr2)
        {
            return !(ptr1 == ptr2);
        }

        public static Ptr operator +(Ptr ptr, long offset)
        {
            return new Ptr(ptr.Position + offset, ptr.Size);
        }

        /// <summary>
        /// Checking whether the pointer is invalid.
        /// </summary>
        public bool IsNull
        {
            get { return this.Equals(NULL); }
        }

        /// <summary>
        /// Returns index of the block after fragment.
        /// </summary>
        public long PositionPlusSize
        {
            get { return checked(Position + Size); }
        }

        #region Serialize/Deserialize

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Position);
            writer.Write(Size);
        }

        public static Ptr Deserialize(BinaryReader reader)
        {
            long position = reader.ReadInt64();
            long size = reader.ReadInt64();

            return new Ptr(position, size);
        }

        #endregion

        public bool Contains(long position)
        {
            return Position <= position && position < PositionPlusSize;
        }
    }
}
