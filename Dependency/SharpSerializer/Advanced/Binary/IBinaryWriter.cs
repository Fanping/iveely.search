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

namespace Polenter.Serialization.Advanced.Binary
{
    /// <summary>
    ///   Writes in a binary format
    /// </summary>
    public interface IBinaryWriter
    {
        /// <summary>
        ///   Writes Element Id
        /// </summary>
        /// <param name = "id"></param>
        void WriteElementId(byte id);

        /// <summary>
        ///   Writes type
        /// </summary>
        /// <param name = "type"></param>
        void WriteType(Type type);

        /// <summary>
        ///   Writes property name
        /// </summary>
        /// <param name = "name"></param>
        void WriteName(string name);

        /// <summary>
        ///   Writes a simple value (value of a simple property)
        /// </summary>
        /// <param name = "value"></param>
        void WriteValue(object value);

        /// <summary>
        ///   Writes an integer. It saves the number with the least required bytes
        /// </summary>
        /// <param name = "number"></param>
        void WriteNumber(int number);

        /// <summary>
        ///   Writes an array of numbers. It saves numbers with the least required bytes
        /// </summary>
        /// <param name = "numbers"></param>
        void WriteNumbers(int[] numbers);

        /// <summary>
        ///   Opens the stream for writing
        /// </summary>
        /// <param name = "stream"></param>
        void Open(Stream stream);

        /// <summary>
        ///   Saves the data to the stream, the stream is not closed and can be further used
        /// </summary>
        void Close();
    }
}