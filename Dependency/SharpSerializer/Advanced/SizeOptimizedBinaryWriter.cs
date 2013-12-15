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
using System.Text;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Core.Binary;

namespace Polenter.Serialization.Advanced
{
    /// <summary>
    ///   Stores data in a binary format. Data is stored in two steps. At first are all objects stored in a cache and all types are analysed. 
    ///   Then all types and property names are sorted and placed in a list. Duplicates are removed. Serialized objects contain references
    ///   to these types and property names. It decreases file size, especially for serialization of collection (many items of the same type)
    ///   SizeOptimizedBinaryWriter has bigger overhead than BurstBinaryWriter
    /// </summary>
    public sealed class SizeOptimizedBinaryWriter : IBinaryWriter
    {
        private readonly Encoding _encoding;
        private readonly ITypeNameConverter _typeNameConverter;
        private List<WriteCommand> _cache;
        private IndexGenerator<string> _names;
        private Stream _stream;
        private IndexGenerator<Type> _types;


        ///<summary>
        ///</summary>
        ///<param name = "typeNameConverter"></param>
        ///<param name = "encoding"></param>
        ///<exception cref = "ArgumentNullException"></exception>
        public SizeOptimizedBinaryWriter(ITypeNameConverter typeNameConverter, Encoding encoding)
        {
            if (typeNameConverter == null) throw new ArgumentNullException("typeNameConverter");
            if (encoding == null) throw new ArgumentNullException("encoding");
            _encoding = encoding;
            _typeNameConverter = typeNameConverter;
        }

        #region IBinaryWriter Members

        /// <summary>
        ///   Writes Property Id
        /// </summary>
        /// <param name = "id"></param>
        public void WriteElementId(byte id)
        {
            _cache.Add(new ByteWriteCommand(id));
        }

        /// <summary>
        ///   Writes type
        /// </summary>
        /// <param name = "type"></param>
        public void WriteType(Type type)
        {
            int typeIndex = _types.GetIndexOfItem(type);
            _cache.Add(new NumberWriteCommand(typeIndex));
        }

        /// <summary>
        ///   Writes property name
        /// </summary>
        /// <param name = "name"></param>
        public void WriteName(string name)
        {
            int nameIndex = _names.GetIndexOfItem(name);
            _cache.Add(new NumberWriteCommand(nameIndex));
        }

        /// <summary>
        ///   Writes a simple value (value of a simple property)
        /// </summary>
        /// <param name = "value"></param>
        public void WriteValue(object value)
        {
            _cache.Add(new ValueWriteCommand(value));
        }

        /// <summary>
        ///   Writes an integer. It saves the number with the least required bytes
        /// </summary>
        /// <param name = "number"></param>
        public void WriteNumber(int number)
        {
            _cache.Add(new NumberWriteCommand(number));
        }

        /// <summary>
        ///   Writes an array of numbers. It saves numbers with the least required bytes
        /// </summary>
        /// <param name = "numbers"></param>
        public void WriteNumbers(int[] numbers)
        {
            _cache.Add(new NumbersWriteCommand(numbers));
        }

        /// <summary>
        ///   Opens the stream for writing
        /// </summary>
        /// <param name = "stream"></param>
        public void Open(Stream stream)
        {
            _stream = stream;
            _cache = new List<WriteCommand>();
            _types = new IndexGenerator<Type>();
            _names = new IndexGenerator<string>();
        }


        /// <summary>
        ///   Saves the data to the stream, the stream is not closed and can be further used
        /// </summary>
        public void Close()
        {
            var writer = new BinaryWriter(_stream, _encoding);

            // Write Names
            writeNamesHeader(writer);

            // Write Types
            writeTypesHeader(writer);

            // Write Data
            writeCache(_cache, writer);

            writer.Flush();
        }

        #endregion

        private static void writeCache(List<WriteCommand> cache, BinaryWriter writer)
        {
            foreach (WriteCommand command in cache)
            {
                command.Write(writer);
            }
        }

        private void writeNamesHeader(BinaryWriter writer)
        {
            // count
            BinaryWriterTools.WriteNumber(_names.Items.Count, writer);

            // Items
            foreach (string name in _names.Items)
            {
                BinaryWriterTools.WriteString(name, writer);
            }
        }

        private void writeTypesHeader(BinaryWriter writer)
        {
            // count
            BinaryWriterTools.WriteNumber(_types.Items.Count, writer);

            // Items
            foreach (Type type in _types.Items)
            {
                string typeName = _typeNameConverter.ConvertToTypeName(type);
                BinaryWriterTools.WriteString(typeName, writer);
            }
        }

        #region Nested type: ByteWriteCommand

        private sealed class ByteWriteCommand : WriteCommand
        {
            public ByteWriteCommand(byte data)
            {
                Data = data;
            }

            public byte Data { get; set; }

            public override void Write(BinaryWriter writer)
            {
                writer.Write(Data);
            }
        }

        #endregion

        #region Nested type: NumberWriteCommand

        private sealed class NumberWriteCommand : WriteCommand
        {
            public NumberWriteCommand(int data)
            {
                Data = data;
            }

            public int Data { get; set; }

            public override void Write(BinaryWriter writer)
            {
                BinaryWriterTools.WriteNumber(Data, writer);
            }
        }

        #endregion

        #region Nested type: NumbersWriteCommand

        private sealed class NumbersWriteCommand : WriteCommand
        {
            public NumbersWriteCommand(int[] data)
            {
                Data = data;
            }

            public int[] Data { get; set; }

            public override void Write(BinaryWriter writer)
            {
                BinaryWriterTools.WriteNumbers(Data, writer);
            }
        }

        #endregion

        #region Nested type: ValueWriteCommand

        private sealed class ValueWriteCommand : WriteCommand
        {
            public ValueWriteCommand(object data)
            {
                Data = data;
            }

            public object Data { get; set; }

            public override void Write(BinaryWriter writer)
            {
                BinaryWriterTools.WriteValue(Data, writer);
            }
        }

        #endregion

        #region Nested type: WriteCommand

        private abstract class WriteCommand
        {
            public abstract void Write(BinaryWriter writer);
        }

        #endregion
    }
}