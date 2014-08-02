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
using System.Reflection;
using Iveely.Dependency.Polenter.Serialization.Advanced.Deserializing;
using Iveely.Dependency.Polenter.Serialization.Advanced.Xml;
using Iveely.Dependency.Polenter.Serialization.Core;
using Iveely.Dependency.Polenter.Serialization.Core.Xml;

namespace Iveely.Dependency.Polenter.Serialization.Advanced
{
    /// <summary>
    ///   Contains logic to read data stored with XmlPropertySerializer
    /// </summary>
    public sealed class XmlPropertyDeserializer : IPropertyDeserializer
    {
        private readonly IXmlReader _reader;

        /// <summary>
        /// All reference targets already processed. Used to for reference resolution.
        /// </summary>
        private readonly Dictionary<int, ReferenceTargetProperty> _propertyCache =
            new Dictionary<int, ReferenceTargetProperty>();

        ///<summary>
        ///</summary>
        ///<param name = "reader"></param>
        public XmlPropertyDeserializer(IXmlReader reader)
        {
            _reader = reader;
        }

        #region IPropertyDeserializer Members

        /// <summary>
        ///   Open the stream to read
        /// </summary>
        /// <param name = "stream"></param>
        public void Open(Stream stream)
        {
            _reader.Open(stream);
        }

        /// <summary>
        ///   Reading the property
        /// </summary>
        /// <returns></returns>
        public Property Deserialize()
        {
            // give the first valid tag back
            string elementName = _reader.ReadElement();

            // In what xml tag is the property saved
            PropertyArt propertyArt = getPropertyArtFromString(elementName);

            // check if the property was found
            if (propertyArt == PropertyArt.Unknown) return null;

            Property result = deserialize(propertyArt, null);
            return result;
        }

        /// <summary>
        ///   Cleans all
        /// </summary>
        public void Close()
        {
            _reader.Close();
        }

        #endregion

        private Property deserialize(PropertyArt propertyArt, Type expectedType)
        {
            // Establish the property name
            string propertyName = _reader.GetAttributeAsString(Attributes.Name);

            // Establish the property type
            Type propertyType = _reader.GetAttributeAsType(Attributes.Type);

            // id propertyType is not defined, we'll take the expectedType)
            if (propertyType == null)
            {
                propertyType = expectedType;
            }

            // create the property from the tag
            Property property = Property.CreateInstance(propertyArt, propertyName, propertyType);

            // Null property?
            var nullProperty = property as NullProperty;
            if (nullProperty != null)
            {
                return nullProperty;
            }

            // is it simple property?
            var simpleProperty = property as SimpleProperty;
            if (simpleProperty != null)
            {
                parseSimpleProperty(_reader, simpleProperty);
                return simpleProperty;
            }

            // This is not a null property and not a simple property
            // it could be only ReferenceProperty or a reference

            int referenceId = _reader.GetAttributeAsInt(Attributes.ReferenceId);

            // Adding property to cache, it must be done before deserializing the object.
            // Otherwise stack overflow occures if the object references itself
            var referenceTarget = property as ReferenceTargetProperty;
            if (referenceTarget != null && referenceId > 0)
            {
                referenceTarget.Reference = new ReferenceInfo() {Id = referenceId, IsProcessed = true};
                _propertyCache.Add(referenceId, referenceTarget);
            }

            if (property==null)
            {
                // Property was not created yet, it can be created as a reference from its id
                if (referenceId < 1)
                    // there is no reference, so the property cannot be restored
                    return null;

                property = createProperty(referenceId, propertyName, propertyType);
                if (property == null)
                    // Reference was not created
                    return null;

                // property was successfully restored as a reference
                return property;
            }

            var multiDimensionalArrayProperty = property as MultiDimensionalArrayProperty;
            if (multiDimensionalArrayProperty != null)
            {
                parseMultiDimensionalArrayProperty(multiDimensionalArrayProperty);
                return multiDimensionalArrayProperty;
            }

            var singleDimensionalArrayProperty = property as SingleDimensionalArrayProperty;
            if (singleDimensionalArrayProperty != null)
            {
                parseSingleDimensionalArrayProperty(singleDimensionalArrayProperty);
                return singleDimensionalArrayProperty;
            }

            var dictionaryProperty = property as DictionaryProperty;
            if (dictionaryProperty != null)
            {
                parseDictionaryProperty(dictionaryProperty);
                return dictionaryProperty;
            }

            var collectionProperty = property as CollectionProperty;
            if (collectionProperty != null)
            {
                parseCollectionProperty(collectionProperty);
                return collectionProperty;
            }

            var complexProperty = property as ComplexProperty;
            if (complexProperty != null)
            {
                parseComplexProperty(complexProperty);
                return complexProperty;
            }

            return property;
        }

        private void parseCollectionProperty(CollectionProperty property)
        {
            // ElementType
            property.ElementType = property.Type != null ? Polenter.Serialization.Serializing.TypeInfo.GetTypeInfo(property.Type).ElementType : null;

            foreach (string subElement in _reader.ReadSubElements())
            {
                if (subElement == SubElements.Properties)
                {
                    // Properties
                    readProperties(property.Properties, property.Type);
                    continue;
                }

                if (subElement == SubElements.Items)
                {
                    // Items
                    readItems(property.Items, property.ElementType);
                }
            }
        }

        private void parseDictionaryProperty(DictionaryProperty property)
        {
            if (property.Type!=null)
            {
                var typeInfo = Polenter.Serialization.Serializing.TypeInfo.GetTypeInfo(property.Type);
                property.KeyType = typeInfo.KeyType;
                property.ValueType = typeInfo.ElementType;
            }

            foreach (string subElement in _reader.ReadSubElements())
            {
                if (subElement == SubElements.Properties)
                {
                    // Properties
                    readProperties(property.Properties, property.Type);
                    continue;
                }
                if (subElement == SubElements.Items)
                {
                    // Items
                    readDictionaryItems(property.Items, property.KeyType, property.ValueType);
                }
            }
        }

        private void readDictionaryItems(IList<KeyValueItem> items, Type expectedKeyType, Type expectedValueType)
        {
            foreach (string subElement in _reader.ReadSubElements())
            {
                if (subElement == SubElements.Item)
                {
                    readDictionaryItem(items, expectedKeyType, expectedValueType);
                }
            }
        }

        private void readDictionaryItem(IList<KeyValueItem> items, Type expectedKeyType, Type expectedValueType)
        {
            Property keyProperty = null;
            Property valueProperty = null;
            foreach (string subElement in _reader.ReadSubElements())
            {
                // check if key and value was found
                if (keyProperty != null && valueProperty != null) break;

                // check if valid tag was found
                PropertyArt propertyArt = getPropertyArtFromString(subElement);
                if (propertyArt == PropertyArt.Unknown) continue;

                // items are as pair key-value defined

                // first is always the key
                if (keyProperty == null)
                {
                    // Key was not defined yet (the first item was found)
                    keyProperty = deserialize(propertyArt, expectedKeyType);
                    continue;
                }

                // key was defined (the second item was found)
                valueProperty = deserialize(propertyArt, expectedValueType);
            }

            // create the item
            var item = new KeyValueItem(keyProperty, valueProperty);
            items.Add(item);
        }

        private void parseMultiDimensionalArrayProperty(MultiDimensionalArrayProperty property)
        {
            property.ElementType = property.Type != null ? Polenter.Serialization.Serializing.TypeInfo.GetTypeInfo(property.Type).ElementType : null;

            foreach (string subElement in _reader.ReadSubElements())
            {
                if (subElement == SubElements.Dimensions)
                {
                    // Read dimensions
                    readDimensionInfos(property.DimensionInfos);
                }

                if (subElement == SubElements.Items)
                {
                    // Read items
                    readMultiDimensionalArrayItems(property.Items, property.ElementType);
                }
            }
        }

        private void readMultiDimensionalArrayItems(IList<MultiDimensionalArrayItem> items, Type expectedElementType)
        {
            foreach (string subElement in _reader.ReadSubElements())
            {
                if (subElement == SubElements.Item)
                {
                    readMultiDimensionalArrayItem(items, expectedElementType);
                }
            }
        }

        private void readMultiDimensionalArrayItem(IList<MultiDimensionalArrayItem> items, Type expectedElementType)
        {
            int[] indexes = _reader.GetAttributeAsArrayOfInt(Attributes.Indexes);
            foreach (string subElement in _reader.ReadSubElements())
            {
                PropertyArt propertyArt = getPropertyArtFromString(subElement);
                if (propertyArt == PropertyArt.Unknown) continue;

                Property value = deserialize(propertyArt, expectedElementType);
                var item = new MultiDimensionalArrayItem(indexes, value);
                items.Add(item);
            }
        }

        private void readDimensionInfos(IList<DimensionInfo> dimensionInfos)
        {
            foreach (string subElement in _reader.ReadSubElements())
            {
                if (subElement == SubElements.Dimension)
                {
                    readDimensionInfo(dimensionInfos);
                }
            }
        }

        private void readDimensionInfo(IList<DimensionInfo> dimensionInfos)
        {
            var info = new DimensionInfo();
            info.Length = _reader.GetAttributeAsInt(Attributes.Length);
            info.LowerBound = _reader.GetAttributeAsInt(Attributes.LowerBound);
            dimensionInfos.Add(info);
        }

        private void parseSingleDimensionalArrayProperty(SingleDimensionalArrayProperty property)
        {
            // ElementType
            property.ElementType = property.Type != null ? Polenter.Serialization.Serializing.TypeInfo.GetTypeInfo(property.Type).ElementType : null;

            // LowerBound
            property.LowerBound = _reader.GetAttributeAsInt(Attributes.LowerBound);

            // Items
            foreach (string subElement in _reader.ReadSubElements())
            {
                if (subElement == SubElements.Items)
                {
                    readItems(property.Items, property.ElementType);
                }
            }
        }

        private void readItems(ICollection<Property> items, Type expectedElementType)
        {
            foreach (string subElement in _reader.ReadSubElements())
            {
                PropertyArt propertyArt = getPropertyArtFromString(subElement);
                if (propertyArt != PropertyArt.Unknown)
                {
                    // Property is found
                    Property subProperty = deserialize(propertyArt, expectedElementType);
                    items.Add(subProperty);
                }
            }
        }

        private void parseComplexProperty(ComplexProperty property)
        {

            foreach (string subElement in _reader.ReadSubElements())
            {
                if (subElement == SubElements.Properties)
                {
                    readProperties(property.Properties, property.Type);
                }
            }
        }

        private void readProperties(PropertyCollection properties, Type ownerType)
        {
            foreach (string subElement in _reader.ReadSubElements())
            {
                PropertyArt propertyArt = getPropertyArtFromString(subElement);
                if (propertyArt != PropertyArt.Unknown)
                {
                    // check if the property with the name exists
                    string subPropertyName = _reader.GetAttributeAsString(Attributes.Name);
                    if (string.IsNullOrEmpty(subPropertyName)) continue;

                    // estimating the propertyInfo
                    PropertyInfo subPropertyInfo = ownerType.GetProperty(subPropertyName);
                    if (subPropertyInfo != null)
                    {
                        Property subProperty = deserialize(propertyArt, subPropertyInfo.PropertyType);
                        properties.Add(subProperty);
                    }
                }
            }
        }

        private void parseSimpleProperty(IXmlReader reader, SimpleProperty property)
        {
            property.Value = _reader.GetAttributeAsObject(Attributes.Value, property.Type);
        }

        private Property createProperty(int referenceId, string propertyName, Type propertyType)
        {
            var cachedProperty = _propertyCache[referenceId];
            var property = (ReferenceTargetProperty)Property.CreateInstance(cachedProperty.Art, propertyName, propertyType);
            cachedProperty.Reference.Count++;
            property.MakeFlatCopyFrom(cachedProperty);
            // Reference must be recreated, cause IsProcessed differs for reference and the full property
            property.Reference = new ReferenceInfo() {Id = referenceId};
            return property;
        }

        private static PropertyArt getPropertyArtFromString(string name)
        {
            if (name == Elements.SimpleObject) return PropertyArt.Simple;
            if (name == Elements.ComplexObject) return PropertyArt.Complex;
            if (name == Elements.Collection) return PropertyArt.Collection;
            if (name == Elements.SingleArray) return PropertyArt.SingleDimensionalArray;
            if (name == Elements.Null) return PropertyArt.Null;
            if (name == Elements.Dictionary) return PropertyArt.Dictionary;
            if (name == Elements.MultiArray) return PropertyArt.MultiDimensionalArray;
            // is used only for backward compatibility
            if (name == Elements.OldReference) return PropertyArt.Reference;
            // is used since the v.2.12
            if (name == Elements.Reference) return PropertyArt.Reference;

            return PropertyArt.Unknown;
        }
    }
}