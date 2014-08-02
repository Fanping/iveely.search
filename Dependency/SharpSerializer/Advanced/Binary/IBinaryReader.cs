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

namespace Iveely.Dependency.Polenter.Serialization.Advanced.Binary
{
    /// <summary>
    ///   Reads from a binary format
    /// </summary>
    public interface IBinaryReader
    {
        /// <summary>
        ///   Reads single byte
        /// </summary>
        /// <returns></returns>
        byte ReadElementId();

        /// <summary>
        ///   Read type
        /// </summary>
        /// <returns>null if no type defined</returns>
        Type ReadType();

        /// <summary>
        ///   Read integer which was saved as 1,2 or 4 bytes, according to its size
        /// </summary>
        /// <returns></returns>
        int ReadNumber();

        /// <summary>
        ///   Read array of integers which were saved as 1,2 or 4 bytes, according to their size
        /// </summary>
        /// <returns>empty array if no numbers defined</returns>
        int[] ReadNumbers();

        /// <summary>
        ///   Reads property name
        /// </summary>
        /// <returns>null if no name defined</returns>
        string ReadName();

        /// <summary>
        ///   Reads simple value (value of a simple property)
        /// </summary>
        /// <param name = "expectedType"></param>
        /// <returns>null if no value defined</returns>
        object ReadValue(Type expectedType);

        /// <summary>
        ///   Opens the stream for reading
        /// </summary>
        /// <param name = "stream"></param>
        void Open(Stream stream);

        /// <summary>
        ///   Does nothing, the stream can be further used and has to be manually closed
        /// </summary>
        void Close();
    }
}