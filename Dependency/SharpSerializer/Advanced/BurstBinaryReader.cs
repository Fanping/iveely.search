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
    ///   Reads data which was stored with the BurstBinaryWriter
    /// </summary>
    public sealed class BurstBinaryReader : IBinaryReader
    {
        private readonly Encoding _encoding;
        private readonly ITypeNameConverter _typeNameConverter;
        private BinaryReader _reader;

        ///<summary>
        ///</summary>
        ///<param name = "typeNameConverter"></param>
        ///<param name = "encoding"></param>
        ///<exception cref = "ArgumentNullException"></exception>
        public BurstBinaryReader(ITypeNameConverter typeNameConverter, Encoding encoding)
        {
            if (typeNameConverter == null) throw new ArgumentNullException("typeNameConverter");
            if (encoding == null) throw new ArgumentNullException("encoding");
            _typeNameConverter = typeNameConverter;
            _encoding = encoding;
        }

        #region IBinaryReader Members

        /// <summary>
        ///   Reads property name
        /// </summary>
        /// <returns></returns>
        public string ReadName()
        {
            return BinaryReaderTools.ReadString(_reader);
        }

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
            if (!_reader.ReadBoolean()) return null;
            string typeAsName = _reader.ReadString();
            return _typeNameConverter.ConvertToType(typeAsName);
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
        /// <returns>Empty array if there are no indexes</returns>
        public int[] ReadNumbers()
        {
            return BinaryReaderTools.ReadNumbers(_reader);
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
        }

        /// <summary>
        ///   Does nothing, the stream can be further used and has to be manually closed
        /// </summary>
        public void Close()
        {
            // don't close the stream if you want further read it
            //_reader.Close();
        }

        #endregion
    }
}