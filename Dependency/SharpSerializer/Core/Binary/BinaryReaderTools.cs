#region Copyright � 2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

//  ***********************************************************************
//  Project: sharpSerializer
//  Web: http://www.sharpserializer.com
//  
//  This software is provided 'as-is', without any express or implied warranty.
//  In no event will the author(s) be held liable for any damages arising from
//  the use of this software.
//  
//  Permission is granted to anyone to use this software for any purpose,
//  including commercial applications, and to alter it and redistribute it
//  freely, subject to the following restrictions:
//  
//      1. The origin of this software must not be misrepresented; you must not
//        claim that you wrote the original software. If you use this software
//        in a product, an acknowledgment in the product documentation would be
//        appreciated but is not required.
//  
//      2. Altered source versions must be plainly marked as such, and must not
//        be misrepresented as being the original software.
//  
//      3. This notice may not be removed or altered from any source distribution.
//  
//  ***********************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace Iveely.Dependency.Polenter.Serialization.Core.Binary
{
    /// <summary>
    ///   Some methods which are used by IBinaryReader
    /// </summary>
    public static class BinaryReaderTools
    {
        ///<summary>
        ///</summary>
        ///<param name = "reader"></param>
        ///<returns></returns>
        public static string ReadString(BinaryReader reader)
        {
            if (!reader.ReadBoolean()) return null;
            return reader.ReadString();
        }

        ///<summary>
        ///</summary>
        ///<param name = "reader"></param>
        ///<returns></returns>
        public static int ReadNumber(BinaryReader reader)
        {
            // Size
            byte size = reader.ReadByte();

            // Number
            switch (size)
            {
                case NumberSize.Zero:
                    return 0;
                case NumberSize.B1:
                    return reader.ReadByte();
                case NumberSize.B2:
                    return reader.ReadInt16();
                default:
                    return reader.ReadInt32();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>empty array if there are no indexes</returns>
        public static int[] ReadNumbers(BinaryReader reader)
        {
            // Count
            int count = ReadNumber(reader);

            if (count == 0) return new int[0];

            // Items
            var result = new List<int>();
            for (int i = 0; i < count; i++)
            {
                result.Add(ReadNumber(reader));
            }
            return result.ToArray();
        }

        ///<summary>
        ///</summary>
        ///<param name = "expectedType"></param>
        ///<param name = "reader"></param>
        ///<returns></returns>
        public static object ReadValue(Type expectedType, BinaryReader reader)
        {
            if (!reader.ReadBoolean()) return null;
            return readValueCore(expectedType, reader);
        }

        private static object readValueCore(Type type, BinaryReader reader)
        {
            try
            {
                if (type == typeof (byte[])) return readArrayOfByte(reader);
                if (type == typeof (string)) return reader.ReadString();
                if (type == typeof (Boolean)) return reader.ReadBoolean();
                if (type == typeof (Byte)) return reader.ReadByte();
                if (type == typeof (Char)) return reader.ReadChar();
                if (type == typeof (DateTime)) return new DateTime(reader.ReadInt64());
                if (type == typeof(Guid)) return new Guid(reader.ReadBytes(16));
#if DEBUG || Smartphone || SILVERLIGHT
                if (type == typeof(decimal)) return readDecimal(reader);                
#else
                if (type == typeof (Decimal)) return reader.ReadDecimal();
#endif
                if (type == typeof (Double)) return reader.ReadDouble();
                if (type == typeof (Int16)) return reader.ReadInt16();
                if (type == typeof (Int32)) return reader.ReadInt32();
                if (type == typeof (Int64)) return reader.ReadInt64();
                if (type == typeof (SByte)) return reader.ReadSByte();
                if (type == typeof (Single)) return reader.ReadSingle();
                if (type == typeof (UInt16)) return reader.ReadUInt16();
                if (type == typeof (UInt32)) return reader.ReadUInt32();
                if (type == typeof (UInt64)) return reader.ReadUInt64();

                if (type == typeof (TimeSpan)) return new TimeSpan(reader.ReadInt64());

                // Enumeration
                if (type.IsEnum) return readEnumeration(type, reader);

                // Type
                if (isType(type))
                {
                    var typeName = reader.ReadString();
                    return Type.GetType(typeName, true);
                }

                throw new InvalidOperationException(string.Format("Unknown simple type: {0}", type.FullName));
            }
            catch (Exception ex)
            {
                throw new SimpleValueParsingException(
                    string.Format("Invalid type: {0}. See details in the inner exception.", type), ex);
            }
        }

        private static object readDecimal(BinaryReader reader)
        {
            var bits = new int[4];
            bits[0] = reader.ReadInt32();
            bits[1] = reader.ReadInt32();
            bits[2] = reader.ReadInt32();
            bits[3] = reader.ReadInt32();
            return new decimal(bits);
        }

        private static bool isType(Type type)
        {
            return type == typeof (Type) || type.IsSubclassOf(typeof (Type));
        }

        private static object readEnumeration(Type expectedType, BinaryReader reader)
        {
            // read the enum as int
            int value = reader.ReadInt32();
            object result = Enum.ToObject(expectedType, value);
            return result;
        }

        private static byte[] readArrayOfByte(BinaryReader reader)
        {
            int length = ReadNumber(reader);
            if (length == 0) return null;

            return reader.ReadBytes(length);
        }
    }
}