using System;
using System.IO;
using Iveely.STSdb4.General.Extensions;

namespace Iveely.STSdb4.General.Persist
{
    public class BooleanIndexerPersist : IIndexerPersist<Boolean>
    {
        public const byte VERSION = 40;

        public void Store(BinaryWriter writer, Func<int, bool> values, int count)
        {
            writer.Write(VERSION);
            
            byte[] buffer = new byte[(int)Math.Ceiling(count / 8.0)];

            for (int i = 0; i < count; i++)
                buffer.SetBit(i, values(i) ? 1 : 0);

            writer.Write(buffer);
        }

        public void Load(BinaryReader reader, Action<int, bool> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid BooleanIndexerPersist version.");

            byte[] buffer = reader.ReadBytes((int)Math.Ceiling(count / 8.0));

            for (int i = 0; i < count; i++)
                values(i, buffer.GetBit(i) == 0 ? false : true);
        }
    }
}
