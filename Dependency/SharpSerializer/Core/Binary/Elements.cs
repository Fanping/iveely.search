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

namespace Polenter.Serialization.Core.Binary
{
    /// <summary>
    ///   These elements are used during the binary serialization. They should be unique from SubElements and Attributes.
    /// </summary>
    public static class Elements
    {
        ///<summary>
        ///</summary>
        public const byte Collection = 1;

        ///<summary>
        ///</summary>
        public const byte ComplexObject = 2;

        ///<summary>
        ///</summary>
        public const byte Dictionary = 3;

        ///<summary>
        ///</summary>
        public const byte MultiArray = 4;

        ///<summary>
        ///</summary>
        public const byte Null = 5;

        ///<summary>
        ///</summary>
        public const byte SimpleObject = 6;

        ///<summary>
        ///</summary>
        public const byte SingleArray = 7;

        ///<summary>
        /// For binary compatibility reason extra type-id: same as ComplexObjectWith, but contains 
        ///</summary>
        public const byte ComplexObjectWithId = 8;

        ///<summary>
        /// reference to previosly serialized  ComplexObjectWithId
        ///</summary>
        public const byte Reference = 9;

        ///<summary>
        ///</summary>
        public const byte CollectionWithId = 10;

        ///<summary>
        ///</summary>
        public const byte DictionaryWithId = 11;

        ///<summary>
        ///</summary>
        public const byte SingleArrayWithId = 12;

        ///<summary>
        ///</summary>
        public const byte MultiArrayWithId = 13;

        ///<summary>
        ///</summary>
        ///<param name="elementId"></param>
        ///<returns></returns>
        public static bool IsElementWithId(byte elementId)
        {
            if (elementId == ComplexObjectWithId)
                return true;
            if (elementId == CollectionWithId)
                return true;
            if (elementId == DictionaryWithId)
                return true;
            if (elementId == SingleArrayWithId)
                return true;
            if (elementId == MultiArrayWithId)
                return true;
            return false;
        }
    }

    /// <summary>
    ///   These elements are used during the binary serialization. They should be unique from Elements and Attributes.
    /// </summary>
    public static class SubElements
    {
        ///<summary>
        ///</summary>
        public const byte Dimension = 51;

        ///<summary>
        ///</summary>
        public const byte Dimensions = 52;

        ///<summary>
        ///</summary>
        public const byte Item = 53;

        ///<summary>
        ///</summary>
        public const byte Items = 54;

        ///<summary>
        ///</summary>
        public const byte Properties = 55;

        ///<summary>
        ///</summary>
        public const byte Unknown = 254;

        ///<summary>
        ///</summary>
        public const byte Eof = 255;
    }

    /// <summary>
    ///   These attributes are used during the binary serialization. They should be unique from Elements and SubElements.
    /// </summary>
    public class Attributes
    {
        ///<summary>
        ///</summary>
        public const byte DimensionCount = 101;

        ///<summary>
        ///</summary>
        public const byte ElementType = 102;

        ///<summary>
        ///</summary>
        public const byte Indexes = 103;

        ///<summary>
        ///</summary>
        public const byte KeyType = 104;

        ///<summary>
        ///</summary>
        public const byte Length = 105;

        ///<summary>
        ///</summary>
        public const byte LowerBound = 106;

        ///<summary>
        ///</summary>
        public const byte Name = 107;

        ///<summary>
        ///</summary>
        public const byte Type = 108;

        ///<summary>
        ///</summary>
        public const byte Value = 109;

        ///<summary>
        ///</summary>
        public const byte ValueType = 110;
    }

    /// <summary>
    ///   How many bytes occupies a number value
    /// </summary>
    public static class NumberSize
    {
        ///<summary>
        ///  is zero
        ///</summary>
        public const byte Zero = 0;

        ///<summary>
        ///  serializes as 1 byte
        ///</summary>
        public const byte B1 = 1;

        ///<summary>
        ///  serializes as 2 bytes
        ///</summary>
        public const byte B2 = 2;

        ///<summary>
        ///  serializes as 4 bytes
        ///</summary>
        public const byte B4 = 4;

        /// <summary>
        ///   Gives the least required byte amount to store the number
        /// </summary>
        /// <param name = "value"></param>
        /// <returns></returns>
        public static byte GetNumberSize(int value)
        {
            if (value == 0) return Zero;
            if (value > Int16.MaxValue || value < Int16.MinValue) return B4;
            if (value < byte.MinValue || value > byte.MaxValue) return B2;
            return B1;
        }
    }
}