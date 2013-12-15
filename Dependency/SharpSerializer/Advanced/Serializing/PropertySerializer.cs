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
using Polenter.Serialization.Core;
using Polenter.Serialization.Serializing;

namespace Polenter.Serialization.Advanced.Serializing
{
    /// <summary>
    ///   Base class for all Serializers (Xml, Binary, ...). XmlPropertySerializer inherits from this class
    /// </summary>
    public abstract class PropertySerializer : IPropertySerializer
    {
        #region IPropertySerializer Members

        /// <summary>
        ///   Serializes property
        /// </summary>
        /// <param name = "property"></param>
        public void Serialize(Property property)
        {
            SerializeCore(new PropertyTypeInfo<Property>(property, null));
        }

        /// <summary>
        ///   Open the stream for writing
        /// </summary>
        /// <param name = "stream"></param>
        public abstract void Open(Stream stream);

        /// <summary>
        ///   Cleaning, but the stream can be used further
        /// </summary>
        public abstract void Close();

        #endregion

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected void SerializeCore(PropertyTypeInfo<Property> property)
        {
            if (property == null) throw new ArgumentNullException("property");

            var nullProperty = property.Property as NullProperty;
            if (nullProperty != null)
            {
                SerializeNullProperty(new PropertyTypeInfo<NullProperty>(nullProperty, property.ExpectedPropertyType,
                                                                         property.ValueType));
                return;
            }

            // check if the value type is equal to the property type.
            // if so, there is no need to explicit define the value type
            if (property.ExpectedPropertyType != null && property.ExpectedPropertyType == property.ValueType)
            {
                // Type is not required, because the property has the same value type as the expected property type
                property.ValueType = null;
            }

            var simpleProperty = property.Property as SimpleProperty;
            if (simpleProperty != null)
            {
                SerializeSimpleProperty(new PropertyTypeInfo<SimpleProperty>(simpleProperty,
                                                                             property.ExpectedPropertyType,
                                                                             property.ValueType));
                return;
            }

            var referenceTarget = property.Property as ReferenceTargetProperty;
            if (referenceTarget != null)
            {
                if (serializeReference(referenceTarget))
                    // Reference to object was serialized
                    return;

                // Full Serializing of the object
                if (serializeReferenceTarget(new PropertyTypeInfo<ReferenceTargetProperty>(referenceTarget,
                                                                                       property.ExpectedPropertyType,
                                                                                       property.ValueType)))
                {                    
                    return;
                }
            }

            throw new InvalidOperationException(string.Format("Unknown Property: {0}", property.Property.GetType()));            
        }

        private bool serializeReferenceTarget(PropertyTypeInfo<ReferenceTargetProperty> property)
        {
            var multiDimensionalArrayProperty = property.Property as MultiDimensionalArrayProperty;
            if (multiDimensionalArrayProperty != null)
            {
                multiDimensionalArrayProperty.Reference.IsProcessed = true;
                SerializeMultiDimensionalArrayProperty(
                    new PropertyTypeInfo<MultiDimensionalArrayProperty>(multiDimensionalArrayProperty,
                                                                        property.ExpectedPropertyType,
                                                                        property.ValueType));
                return true;
            }

            var singleDimensionalArrayProperty = property.Property as SingleDimensionalArrayProperty;
            if (singleDimensionalArrayProperty != null)
            {
                singleDimensionalArrayProperty.Reference.IsProcessed = true;
                SerializeSingleDimensionalArrayProperty(
                    new PropertyTypeInfo<SingleDimensionalArrayProperty>(singleDimensionalArrayProperty,
                                                                         property.ExpectedPropertyType,
                                                                         property.ValueType));
                return true;
            }

            var dictionaryProperty = property.Property as DictionaryProperty;
            if (dictionaryProperty != null)
            {
                dictionaryProperty.Reference.IsProcessed = true;
                SerializeDictionaryProperty(new PropertyTypeInfo<DictionaryProperty>(dictionaryProperty,
                                                                                     property.ExpectedPropertyType,
                                                                                     property.ValueType));
                return true;
            }

            var collectionProperty = property.Property as CollectionProperty;
            if (collectionProperty != null)
            {
                collectionProperty.Reference.IsProcessed = true;
                SerializeCollectionProperty(new PropertyTypeInfo<CollectionProperty>(collectionProperty,
                                                                                     property.ExpectedPropertyType,
                                                                                     property.ValueType));
                return true;
            }

            var complexProperty = property.Property as ComplexProperty;
            if (complexProperty != null)
            {
                complexProperty.Reference.IsProcessed = true;
                SerializeComplexProperty(new PropertyTypeInfo<ComplexProperty>(complexProperty,
                                                                               property.ExpectedPropertyType,
                                                                               property.ValueType));
                return true;
            }

            return false;
        }

        private bool serializeReference(ReferenceTargetProperty property)
        {
            if (property.Reference.Count > 1)
            {
                // There are more references to this object
                if (property.Reference.IsProcessed)
                {
                    // The object is already serialized
                    // Only its reference should be stored
                    SerializeReference(property);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected abstract void SerializeNullProperty(PropertyTypeInfo<NullProperty> property);

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected abstract void SerializeSimpleProperty(PropertyTypeInfo<SimpleProperty> property);

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected abstract void SerializeMultiDimensionalArrayProperty(
            PropertyTypeInfo<MultiDimensionalArrayProperty> property);

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected abstract void SerializeSingleDimensionalArrayProperty(
            PropertyTypeInfo<SingleDimensionalArrayProperty> property);

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected abstract void SerializeDictionaryProperty(PropertyTypeInfo<DictionaryProperty> property);

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected abstract void SerializeCollectionProperty(PropertyTypeInfo<CollectionProperty> property);

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected abstract void SerializeComplexProperty(PropertyTypeInfo<ComplexProperty> property);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceTarget"></param>
        protected abstract void SerializeReference(ReferenceTargetProperty referenceTarget);
    }
}