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
using Iveely.Dependency.Polenter.Serialization.Advanced.Binary;
using Iveely.Dependency.Polenter.Serialization.Advanced.Serializing;
using Iveely.Dependency.Polenter.Serialization.Core.Binary;

namespace Iveely.Dependency.Polenter.Serialization.Advanced
{
    /// <summary>
    ///   Reads data which was stored with SizeOptimizedBinaryWriter
    /// </summary>
    public sealed class SizeOptimizedBinaryReader : IBinaryReader
    {
        // .NET 2.0 doesn't support Func, it has to be manually declared
        private delegate T HeaderCallback<T>(string text);

        private readonly Encoding _encoding;
        private readonly IList<string> _names = new List<string>();
        private readonly ITypeNameConverter _typeNameConverter;

        // Translation table of types
        private readonly IList<Type> _types = new List<Type>();
        private BinaryReader _reader;

        // Translation table of property names


        ///<summary>
        ///</summary>
        ///<param name = "typeNameConverter"></param>
        ///<param name = "encoding"></param>
        ///<exception cref = "ArgumentNullException"></exception>
        public SizeOptimizedBinaryReader(ITypeNameConverter typeNameConverter, Encoding encoding)
        {
            if (typeNameConverter == null) throw new ArgumentNullException("typeNameConverter");
            if (encoding == null) throw new ArgumentNullException("encoding");
            _typeNameConverter = typeNameConverter;
            _encoding = encoding;
        }

        #region IBinaryReader Members

        /// <summary>
        ///   Reads single byte
        /// </summary>
        /// <returns></returns>
        public byte ReadElementId()
        {
            return _reader.ReadByte();
        }

        /// <summary>
        ///   Read type
        /// </summary>
        /// <returns></returns>
        public Type ReadType()
        {
            int index = BinaryReaderTools.ReadNumber(_reader);
            return _types[index];
        }

        /// <summary>
        ///   Read integer which was saved as 1,2 or 4 bytes, according to its size
        /// </summary>
        /// <returns></returns>
        public int ReadNumber()
        {
            return BinaryReaderTools.ReadNumber(_reader);
        }

        /// <summary>
        ///   Read array of integers which were saved as 1,2 or 4 bytes, according to their size
        /// </summary>
        /// <returns></returns>
        public int[] ReadNumbers()
        {
            return BinaryReaderTools.ReadNumbers(_reader);
        }

        /// <summary>
        ///   Reads property name
        /// </summary>
        /// <returns></returns>
        public string ReadName()
        {
            int index = BinaryReaderTools.ReadNumber(_reader);
            return _names[index];
        }

        /// <summary>
        ///   Reads simple value (value of a simple property)
        /// </summary>
        /// <param name = "expectedType"></param>
        /// <returns></returns>
        public object ReadValue(Type expectedType)
        {
            return BinaryReaderTools.ReadValue(expectedType, _reader);
        }

        /// <summary>
        ///   Opens the stream for reading
        /// </summary>
        /// <param name = "stream"></param>
        public void Open(Stream stream)
        {
            _reader = new BinaryReader(stream, _encoding);

            // read names
            _names.Clear();
            readHeader(_reader, _names, text => text);

            // read types
            _types.Clear();
            readHeader(_reader, _types, _typeNameConverter.ConvertToType);
        }

        /// <summary>
        ///   Does nothing, the stream can be further used and has to be manually closed
        /// </summary>
        public void Close()
        {
            // nothing to do
        }

        #endregion

        private static void readHeader<T>(BinaryReader reader, IList<T> items, HeaderCallback<T> readCallback)
        {
            // Count
            int count = BinaryReaderTools.ReadNumber(reader);

            // Items)
            for (int i = 0; i < count; i++)
            {
                string itemAsText = BinaryReaderTools.ReadString(reader);
                T item = readCallback(itemAsText);
                items.Add(item);
            }
        }
    }
}