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

namespace Polenter.Serialization.Core.Xml
{
    /// <summary>
    ///   These elements are used as tags during the xml serialization.
    /// </summary>
    public static class Elements
    {
        ///<summary>
        ///</summary>
        public const string Collection = "Collection";

        ///<summary>
        ///</summary>
        public const string ComplexObject = "Complex";

        ///<summary>
        /// internal used as an id for referencing already serialized items
        /// Since v.2.12 Elements.Reference is used instead.
        ///</summary>
        public const string OldReference = "ComplexReference";
        ///<summary>
        /// used as an id for referencing already serialized items
        ///</summary>
        public const string Reference = "Reference";

        ///<summary>
        ///</summary>
        public const string Dictionary = "Dictionary";

        ///<summary>
        ///</summary>
        public const string MultiArray = "MultiArray";

        ///<summary>
        ///</summary>
        public const string Null = "Null";

        ///<summary>
        ///</summary>
        public const string SimpleObject = "Simple";

        ///<summary>
        ///</summary>
        public const string SingleArray = "SingleArray";
    }

    /// <summary>
    ///   These elements are used as tags during the xml serialization.
    /// </summary>
    public static class SubElements
    {
        ///<summary>
        ///</summary>
        public const string Dimension = "Dimension";

        ///<summary>
        ///</summary>
        public const string Dimensions = "Dimensions";

        ///<summary>
        ///</summary>
        public const string Item = "Item";

        ///<summary>
        ///</summary>
        public const string Items = "Items";

        ///<summary>
        ///</summary>
        public const string Properties = "Properties";
    }

    /// <summary>
    ///   These attributes are used during the xml serialization.
    /// </summary>
    public static class Attributes
    {
        ///<summary>
        ///</summary>
        public const string DimensionCount = "dimensionCount";

        ///<summary>
        ///</summary>
        public const string ElementType = "elementType";

        ///<summary>
        ///</summary>
        public const string Indexes = "indexes";

        ///<summary>
        ///</summary>
        public const string KeyType = "keyType";

        ///<summary>
        ///</summary>
        public const string Length = "length";

        ///<summary>
        ///</summary>
        public const string LowerBound = "lowerBound";

        ///<summary>
        ///</summary>
        public const string Name = "name";

        ///<summary>
        ///</summary>
        public const string Type = "type";

        ///<summary>
        ///</summary>
        public const string Value = "value";

        ///<summary>
        ///</summary>
        public const string ValueType = "valueType";

        ///<summary>
        /// used as an id to identify and refere already serialized items
        ///</summary>
        public const string ReferenceId = "id";
    }
}