#region Copyright ?2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

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
using System.Text;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Core.Binary;

namespace Polenter.Serialization.Advanced
{
    /// <summary>
    ///   Stores data in a binary format. All types and property names which describe an object are stored together with the object.
    ///   If there are more objects to store, their types are multiple stored, what increases the file size. 
    ///   This format is simple and has small overhead.
    /// </summary>
    public sealed class BurstBinaryWriter : IBinaryWriter
    {
        private readonly Encoding _encoding;
        private readonly ITypeNameConverter _typeNameConverter;
        private BinaryWriter _writer;

        ///<summary>
        ///</summary>
        ///<param name = "typeNameConverter"></param>
        ///<param name = "encoding"></param>
        ///<exception cref = "ArgumentNullException"></exception>
        public BurstBinaryWriter(ITypeNameConverter typeNameConverter, Encoding encoding)
        {
            if (typeNameConverter == null) throw new ArgumentNullException("typeNameConverter");
            if (encoding == null) throw new ArgumentNullException("encoding");
            _encoding = encoding;
            _typeNameConverter = typeNameConverter;
        }

        #region IBinaryWriter Members

        /// <summary>
        ///   Writes Element Id
        /// </summary>
        /// <param name = "id"></param>
        public void WriteElementId(byte id)
        {
            _writer.Write(id);
        }

        /// <summary>
        ///   Writes an integer. It saves the number with the least required bytes
        /// </summary>
        /// <param name = "number"></param>
        public void WriteNumber(int number)
        {
            BinaryWriterTools.WriteNumber(number, _writer);
        }

        /// <summary>
        ///   Writes an array of numbers. It saves numbers with the least required bytes
        /// </summary>
        /// <param name = "numbers"></param>
        public void WriteNumbers(int[] numbers)
        {
            BinaryWriterTools.WriteNumbers(numbers, _writer);
        }

        /// <summary>
        ///   Writes type
        /// </summary>
        /// <param name = "type"></param>
        public void WriteType(Type type)
        {
            if (type == null)
            {
                _writer.Write(false);
            }
            else
            {
                _writer.Write(true);
                _writer.Write(_typeNameConverter.ConvertToTypeName(type));
            }
        }

        /// <summary>
        ///   Writes property name
        /// </summary>
        /// <param name = "name"></param>
        public void WriteName(string name)
        {
            BinaryWriterTools.WriteString(name, _writer);
        }

        /// <summary>
        ///   Writes a simple value (value of a simple property)
        /// </summary>
        /// <param name = "value"></param>
        public void WriteValue(object value)
        {
            BinaryWriterTools.WriteValue(value, _writer);
        }

        /// <summary>
        ///   Opens the stream for writing
        /// </summary>
        /// <param name = "stream"></param>
        public void Open(Stream stream)
        {
            _writer = new BinaryWriter(stream, _encoding);
        }

        /// <summary>
        ///   Saves the data to the stream, the stream is not closed and can be further used
        /// </summary>
        public void Close()
        {
            _writer.Flush();
        }

        #endregion
    }
}