#region Copyright © 2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

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
using Iveely.Dependency.Polenter.Serialization.Core;

namespace Iveely.Dependency.Polenter.Serialization.Serializing
{
    /// <summary>
    ///   Contains info about property and its type.
    ///   It is of use to avoid double type definitions.
    /// </summary>
    /// <typeparam name = "TProperty"></typeparam>
    /// <remarks>
    ///   During serialization is each property wrapped in this class.
    ///   there is no need to define type of every array element if there is a global ElementType type defined
    ///   and each array element type is equal to that global ElementType
    ///   In such a case ElementType is stored in ExpectedPropertyType, ValueType contains null.
    /// </remarks>
    public sealed class PropertyTypeInfo<TProperty> where TProperty : Property
    {
        ///<summary>
        ///</summary>
        ///<param name = "property"></param>
        ///<param name = "valueType"></param>
        public PropertyTypeInfo(TProperty property, Type valueType)
        {
            Property = property;
            ExpectedPropertyType = valueType;
            ValueType = property.Type;
            Name = property.Name;
        }

        ///<summary>
        ///</summary>
        ///<param name = "property"></param>
        ///<param name = "expectedPropertyType"></param>
        ///<param name = "valueType"></param>
        public PropertyTypeInfo(TProperty property, Type expectedPropertyType, Type valueType)
        {
            Property = property;
            ExpectedPropertyType = expectedPropertyType;
            ValueType = valueType;
            Name = property.Name;
        }

        /// <summary>
        ///   Of what type should be this property
        /// </summary>
        public Type ExpectedPropertyType { get; set; }

        /// <summary>
        ///   Of what type is the property value. If it is null - then the value type is equal to expectedPropertyType
        ///   and does not need to be additionally serialized
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        ///   Property name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   Property itself
        /// </summary>
        public TProperty Property { get; set; }
    }
}