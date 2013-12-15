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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Core;
using Polenter.Serialization.Core.Binary;
using Polenter.Serialization.Serializing;

namespace Polenter.Serialization.Advanced
{
    /// <summary>
    ///   Contains logic to serialize data to a binary format. Format varies according to the used IBinaryWriter. 
    ///   Actually there are BurstBinaryWriter and SizeOptimizedBinaryWriter (see the constructor)
    /// </summary>
    public sealed class BinaryPropertySerializer : PropertySerializer
    {
        private readonly IBinaryWriter _writer;


        ///<summary>
        ///</summary>
        ///<param name = "writer"></param>
        public BinaryPropertySerializer(IBinaryWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _writer = writer;
        }

        /// <summary>
        ///   Open the stream for writing
        /// </summary>
        /// <param name = "stream" />
        public override void Open(Stream stream)
        {
            _writer.Open(stream);
        }

        /// <summary>
        ///   Closes the stream
        /// </summary>
        public override void Close()
        {
            _writer.Close();
        }


        private void writePropertyHeader(byte elementId, string name, Type valueType)
        {
            _writer.WriteElementId(elementId);
            _writer.WriteName(name);
            _writer.WriteType(valueType);
        }

        private bool writePropertyHeaderWithReferenceId(byte elementId, ReferenceInfo info, string name, Type valueType)
        {
            if (info.Count < 2)
                // no need to write id
                return false;
            writePropertyHeader(elementId, name, valueType);
            _writer.WriteNumber(info.Id);
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeNullProperty(PropertyTypeInfo<NullProperty> property)
        {
            writePropertyHeader(Elements.Null, property.Name, property.ValueType);
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeSimpleProperty(PropertyTypeInfo<SimpleProperty> property)
        {
            writePropertyHeader(Elements.SimpleObject, property.Name, property.ValueType);
            _writer.WriteValue(property.Property.Value);
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeMultiDimensionalArrayProperty(
            PropertyTypeInfo<MultiDimensionalArrayProperty> property)
        {
            if (!writePropertyHeaderWithReferenceId(Elements.MultiArrayWithId, property.Property.Reference, property.Name, property.ValueType))
            {
                // Property value is not referenced multiple times
                writePropertyHeader(Elements.MultiArray, property.Name, property.ValueType);
            } 

            // ElementType
            _writer.WriteType(property.Property.ElementType);

            // DimensionInfos
            writeDimensionInfos(property.Property.DimensionInfos);

            // Eintr�ge
            writeMultiDimensionalArrayItems(property.Property.Items, property.Property.ElementType);
        }

        private void writeMultiDimensionalArrayItems(IList<MultiDimensionalArrayItem> items, Type defaultItemType)
        {
            // Count
            _writer.WriteNumber(items.Count);

            // Items
            foreach (MultiDimensionalArrayItem item in items)
            {
                writeMultiDimensionalArrayItem(item, defaultItemType);
            }
        }

        private void writeMultiDimensionalArrayItem(MultiDimensionalArrayItem item, Type defaultItemType)
        {
            // Write coordinates
            _writer.WriteNumbers(item.Indexes);

            // Write Data
            SerializeCore(new PropertyTypeInfo<Property>(item.Value, defaultItemType));
        }

        private void writeDimensionInfos(IList<DimensionInfo> dimensionInfos)
        {
            // count
            _writer.WriteNumber(dimensionInfos.Count);

            // items
            foreach (DimensionInfo info in dimensionInfos)
            {
                writeDimensionInfo(info);
            }
        }

        private void writeDimensionInfo(DimensionInfo info)
        {
            // Length
            _writer.WriteNumber(info.Length);

            // LowerBound
            _writer.WriteNumber(info.LowerBound);
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeSingleDimensionalArrayProperty(
            PropertyTypeInfo<SingleDimensionalArrayProperty> property)
        {
            if (!writePropertyHeaderWithReferenceId(Elements.SingleArrayWithId, property.Property.Reference, property.Name, property.ValueType))
            {
                // Property value is not referenced multiple times
                writePropertyHeader(Elements.SingleArray, property.Name, property.ValueType);
            } 

            // ElementType
            _writer.WriteType(property.Property.ElementType);

            // Lower Bound
            _writer.WriteNumber(property.Property.LowerBound);

            // items
            writeItems(property.Property.Items, property.Property.ElementType);
        }

        private void writeItems(ICollection<Property> items, Type defaultItemType)
        {
            // Count
            _writer.WriteNumber(items.Count);

            // items
            foreach (Property item in items)
            {
                SerializeCore(new PropertyTypeInfo<Property>(item, defaultItemType));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeDictionaryProperty(PropertyTypeInfo<DictionaryProperty> property)
        {
            if (!writePropertyHeaderWithReferenceId(Elements.DictionaryWithId, property.Property.Reference, property.Name, property.ValueType))
            {
                // Property value is not referenced multiple times
                writePropertyHeader(Elements.Dictionary, property.Name, property.ValueType);
            } 

            // type of keys
            _writer.WriteType(property.Property.KeyType);

            // type of values
            _writer.WriteType(property.Property.ValueType);

            // Properties
            writeProperties(property.Property.Properties, property.Property.Type);

            // Items
            writeDictionaryItems(property.Property.Items, property.Property.KeyType, property.Property.ValueType);
        }

        private void writeDictionaryItems(IList<KeyValueItem> items, Type defaultKeyType, Type defaultValueType)
        {
            // count
            _writer.WriteNumber(items.Count);

            foreach (KeyValueItem item in items)
            {
                writeDictionaryItem(item, defaultKeyType, defaultValueType);
            }
        }

        private void writeDictionaryItem(KeyValueItem item, Type defaultKeyType, Type defaultValueType)
        {
            // Key
            SerializeCore(new PropertyTypeInfo<Property>(item.Key, defaultKeyType));

            // Value
            SerializeCore(new PropertyTypeInfo<Property>(item.Value, defaultValueType));
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeCollectionProperty(PropertyTypeInfo<CollectionProperty> property)
        {
            if (!writePropertyHeaderWithReferenceId(Elements.CollectionWithId, property.Property.Reference, property.Name, property.ValueType))
            {
                // Property value is not referenced multiple times
                writePropertyHeader(Elements.Collection, property.Name, property.ValueType);
            }  

            // ElementType
            _writer.WriteType(property.Property.ElementType);

            // Properties
            writeProperties(property.Property.Properties, property.Property.Type);

            //Items
            writeItems(property.Property.Items, property.Property.ElementType);
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeComplexProperty(PropertyTypeInfo<ComplexProperty> property)
        {
            if (!writePropertyHeaderWithReferenceId(Elements.ComplexObjectWithId, property.Property.Reference, property.Name, property.ValueType))
            {
                // Property value is not referenced multiple times
                writePropertyHeader(Elements.ComplexObject, property.Name, property.ValueType);                
            }            

            // Properties
            writeProperties(property.Property.Properties, property.Property.Type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceTarget"></param>
        protected override void SerializeReference(ReferenceTargetProperty referenceTarget)
        {
            writePropertyHeader(Elements.Reference, referenceTarget.Name, null);
            _writer.WriteNumber(referenceTarget.Reference.Id);
        }

        private void writeProperties(PropertyCollection properties, Type ownerType)
        {
            // How many
            _writer.WriteNumber(Convert.ToInt16(properties.Count));

            // Serialize all of them
            foreach (Property property in properties)
            {
                PropertyInfo propertyInfo = ownerType.GetProperty(property.Name);
                SerializeCore(new PropertyTypeInfo<Property>(property, propertyInfo.PropertyType));
            }
        }
    }
}