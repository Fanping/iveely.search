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
using System.IO;

namespace Polenter.Serialization.Core.Binary
{
    /// <summary>
    ///   Some methods which are used by IBinaryWriter
    /// </summary>
    public static class BinaryWriterTools
    {
        ///<summary>
        ///</summary>
        ///<param name = "number"></param>
        ///<param name = "writer"></param>
        public static void WriteNumber(int number, BinaryWriter writer)
        {
            // Write size
            byte size = NumberSize.GetNumberSize(number);
            writer.Write(size);

            // Write number
            if (size > NumberSize.Zero)
            {
                switch (size)
                {
                    case NumberSize.B1:
                        writer.Write((byte) number);
                        break;
                    case NumberSize.B2:
                        writer.Write((Int16) number);
                        break;
                    default:
                        writer.Write(number);
                        break;
                }
            }
        }

        ///<summary>
        ///</summary>
        ///<param name = "numbers"></param>
        ///<param name = "writer"></param>
        public static void WriteNumbers(int[] numbers, BinaryWriter writer)
        {
            // Length
            WriteNumber(numbers.Length, writer);

            // Numbers
            foreach (int number in numbers)
            {
                WriteNumber(number, writer);
            }
        }


        ///<summary>
        ///</summary>
        ///<param name = "value"></param>
        ///<param name = "writer"></param>
        public static void WriteValue(object value, BinaryWriter writer)
        {
            if (value == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writeValueCore(value, writer);
            }
        }

        /// <summary>
        ///   BinaryWriter.Write(string...) can not be used as it produces exception if the text is null.
        /// </summary>
        /// <param name = "text"></param>
        /// <param name = "writer"></param>
        public static void WriteString(string text, BinaryWriter writer)
        {
            if (string.IsNullOrEmpty(text))
            {
                // no exception if the text is null
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(text);
            }
        }

        private static void writeValueCore(object value, BinaryWriter writer)
        {
            if (value == null) throw new ArgumentNullException("value", "Written data can not be null.");

            // Write argument data
            Type type = value.GetType();

            if (type == typeof (byte[]))
            {
                writeArrayOfByte((byte[]) value, writer);
                return;
            }
            if (type == typeof (string))
            {
                writer.Write((string) value);
                return;
            }
            if (type == typeof (Boolean))
            {
                writer.Write((bool) value);
                return;
            }
            if (type == typeof (Byte))
            {
                writer.Write((byte) value);
                return;
            }
            if (type == typeof (Char))
            {
                writer.Write((Char) value);
                return;
            }
            if (type == typeof (DateTime))
            {
                writer.Write(((DateTime) value).Ticks);
                return;
            }
            if (type == typeof(Guid))
            {
                writer.Write(((Guid)value).ToByteArray());
                return;
            }
#if DEBUG || Smartphone || SILVERLIGHT
            if (type == typeof(decimal))
            {
                writeDecimal((decimal)value, writer);
                return;
            }
#else
            if (type == typeof (Decimal))
            {
                writer.Write((Decimal) value);
                return;
            }
#endif
            if (type == typeof (Double))
            {
                writer.Write((Double) value);
                return;
            }
            if (type == typeof (Int16))
            {
                writer.Write((Int16) value);
                return;
            }
            if (type == typeof (Int32))
            {
                writer.Write((Int32) value);
                return;
            }
            if (type == typeof (Int64))
            {
                writer.Write((Int64) value);
                return;
            }
            if (type == typeof (SByte))
            {
                writer.Write((SByte) value);
                return;
            }
            if (type == typeof (Single))
            {
                writer.Write((Single) value);
                return;
            }
            if (type == typeof (UInt16))
            {
                writer.Write((UInt16) value);
                return;
            }
            if (type == typeof (UInt32))
            {
                writer.Write((UInt32) value);
                return;
            }
            if (type == typeof (UInt64))
            {
                writer.Write((UInt64) value);
                return;
            }

            if (type == typeof (TimeSpan))
            {
                writer.Write(((TimeSpan) value).Ticks);
                return;
            }

            // Enumeration
            if (type.IsEnum)
            {
                writer.Write(Convert.ToInt32(value));
                return;
            }

            // Type
            if (isType(type))
            {
                writer.Write(((Type)value).AssemblyQualifiedName);
                return;
            }

            throw new InvalidOperationException(string.Format("Unknown simple type: {0}", type.FullName));
        }

        private static void writeDecimal(decimal value, BinaryWriter writer)
        {
            var bits = decimal.GetBits(value);
            writer.Write(bits[0]);
            writer.Write(bits[1]);
            writer.Write(bits[2]);
            writer.Write(bits[3]);
        }

        private static bool isType(Type type)
        {
            return type == typeof(Type) || type.IsSubclassOf(typeof(Type));
        }

        private static void writeArrayOfByte(byte[] data, BinaryWriter writer)
        {
            WriteNumber(data.Length, writer);
            writer.Write(data);
        }
    }
}