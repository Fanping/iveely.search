using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.Storage
{
    public class AtomicHeader
    {
        private const string TITLE = "STSdb 4.0";
        /// <summary>
        /// http://en.wikipedia.org/wiki/Advanced_Format
        /// http://www.idema.org
        /// </summary>
        public const int SIZE = 4 * 1024;
        public const int MAX_TAG_DATA = 256;

        private byte[] tag;
        public int Version;
        public bool UseCompression;

        /// <summary>
        /// System data location.
        /// </summary>
        public Ptr SystemData;

        public void Serialize(Stream stream)
        {
            byte[] buffer = new byte[SIZE];

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(TITLE);

                writer.Write(Version);
                writer.Write(UseCompression);

                //last flush location
                SystemData.Serialize(writer);

                //tag
                if (Tag == null)
                    writer.Write((int)-1);
                else
                {
                    writer.Write(Tag.Length);
                    writer.Write(Tag);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(buffer, 0, buffer.Length);
        }

        public static AtomicHeader Deserialize(Stream stream)
        {
            AtomicHeader header = new AtomicHeader();

            stream.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[SIZE];
            if (stream.Read(buffer, 0, SIZE) != SIZE)
                throw new Exception(String.Format("Invalid {0} header.", TITLE));

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryReader reader = new BinaryReader(ms);

                string title = reader.ReadString();
                if (title != TITLE)
                    throw new Exception(String.Format("Invalid {0} header.", TITLE));

                header.Version = reader.ReadInt32();
                header.UseCompression = reader.ReadBoolean();

                //last flush location
                header.SystemData = Ptr.Deserialize(reader);

                //tag
                int tagLength = reader.ReadInt32();
                header.Tag = tagLength >= 0 ? reader.ReadBytes(tagLength) : null;
            }

            return header;
        }

        public byte[] Tag
        {
            get { return tag; }
            set
            {
                if (value != null && value.Length > MAX_TAG_DATA)
                    throw new ArgumentException("Tag");

                tag = value;
            }
        }
    }
}
