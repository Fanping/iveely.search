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
using System.Collections.ObjectModel;

namespace Polenter.Serialization.Core
{
    /// <summary>
    ///   Base class for all properties. Every object can be defined with inheritors of the Property class.
    /// </summary>
    public abstract class Property
    {
        /// <summary>
        /// </summary>
        /// <param name = "name"></param>
        /// <param name = "type"></param>
        protected Property(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        ///   Not all properties have name (i.e. items of a collection)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   Of what type is the property or its value
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        ///   If the properties are nested, i.e. collection items are nested in the collection
        /// </summary>
        public Property Parent { get; set; }

        ///<summary>
        /// Of what art is the property.
        ///</summary>
        public PropertyArt Art { get { return GetPropertyArt(); } }

        /// <summary>
        /// Gets the property art.
        /// </summary>
        /// <returns></returns>
        protected abstract PropertyArt GetPropertyArt();

        ///<summary>
        /// Creates property from PropertyArt
        ///</summary>
        ///<param name="art"></param>
        ///<param name="propertyName"></param>
        ///<param name="propertyType"></param>
        ///<returns>null if PropertyArt.Reference is requested</returns>
        ///<exception cref="InvalidOperationException">If unknown PropertyArt requested</exception>
        public static Property CreateInstance(PropertyArt art, string propertyName, Type propertyType)
        {
            switch (art)
            {
                case PropertyArt.Collection:
                    return new CollectionProperty(propertyName, propertyType);
                case PropertyArt.Complex:
                    return new ComplexProperty(propertyName, propertyType);
                case PropertyArt.Dictionary:
                    return new DictionaryProperty(propertyName, propertyType);
                case PropertyArt.MultiDimensionalArray:
                    return new MultiDimensionalArrayProperty(propertyName, propertyType);
                case PropertyArt.Null:
                    return new NullProperty(propertyName);
                case PropertyArt.Reference:
                    return null;
                case PropertyArt.Simple:
                    return new SimpleProperty(propertyName, propertyType);
                case PropertyArt.SingleDimensionalArray:
                    return new SingleDimensionalArrayProperty(propertyName, propertyType);
                default:
                    throw new InvalidOperationException(string.Format("Unknown PropertyArt {0}", art));
            }
        }


        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public override string ToString()
        {
            string name = Name ?? "null";
            string type = Type == null ? "null" : Type.Name;
            string parent = Parent == null ? "null" : Parent.GetType().Name;            
            return string.Format("{0}, Name={1}, Type={2}, Parent={3}", GetType().Name, name, type, parent);
        }
    }

    ///<summary>
    /// All properties derived from this property can be a target of a reference
    ///</summary>
    public abstract class ReferenceTargetProperty : Property
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        protected ReferenceTargetProperty(string name, Type type) : base(name, type)
        {
        }

        ///<summary>
        /// Information about the References for this property
        ///</summary>
        public ReferenceInfo Reference { get; set; }

        ///<summary>
        /// Makes flat copy (only references) of vital properties
        ///</summary>
        ///<param name="source"></param>
        public virtual void MakeFlatCopyFrom(ReferenceTargetProperty source)
        {
            Reference = source.Reference;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public override string ToString()
        {
            var text = base.ToString();
            string reference = Reference != null ? Reference.ToString() : "null";
            return string.Format("{0}, Reference={1}", text, reference);
        }
    }

    ///<summary>
    /// Provides information about property references
    ///</summary>
    public sealed class ReferenceInfo
    {
        ///<summary>
        ///</summary>
        public ReferenceInfo()
        {
            Count = 1;
        }

        ///<summary>
        /// How many references to the same object
        ///</summary>
        public int Count { get; set; }

        ///<summary>
        /// Every Object must have a unique Id
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// During serialization is true if the target object was already serialized.
        /// Then the target must not be serialized again. Only its reference must be created.
        /// During deserialization it means, the target object was parsed and read
        /// from the stream. It can be further used to resolve its references.
        ///</summary>
        public bool IsProcessed { get; set; }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override string ToString()
        {
            return string.Format("{0}, Count={1}, Id={2}, IsProcessed={3}", GetType().Name, Count, Id, IsProcessed);
        }
    }

    /// <summary>
    ///   Represents the null value. Null values are serialized too.
    /// </summary>
    public sealed class NullProperty : Property
    {
        ///<summary>
        ///</summary>
        public NullProperty() : base(null, null)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name = "name"></param>
        public NullProperty(string name)
            : base(name, null)
        {
        }

        /// <summary>
        /// Gets the property art.
        /// </summary>
        /// <returns></returns>
        protected override PropertyArt GetPropertyArt()
        {
            return PropertyArt.Null;
        }
    }

    /// <summary>
    ///   It represents some properties of an object, or some items of a collection/dictionary/array
    /// </summary>
    public sealed class PropertyCollection : Collection<Property>
    {
        ///<summary>
        ///  Parent property
        ///</summary>
        public Property Parent { get; set; }

        /// <summary>
        /// </summary>
        protected override void ClearItems()
        {
            foreach (Property item in Items)
            {
                item.Parent = null;
            }
            base.ClearItems();
        }

        /// <summary>
        /// </summary>
        /// <param name = "index"></param>
        /// <param name = "item"></param>
        protected override void InsertItem(int index, Property item)
        {
            base.InsertItem(index, item);
            item.Parent = Parent;
        }

        /// <summary>
        /// </summary>
        /// <param name = "index"></param>
        protected override void RemoveItem(int index)
        {
            Items[index].Parent = null;
            base.RemoveItem(index);
        }

        /// <summary>
        /// </summary>
        /// <param name = "index"></param>
        /// <param name = "item"></param>
        protected override void SetItem(int index, Property item)
        {
            Items[index].Parent = null;
            base.SetItem(index, item);
            item.Parent = Parent;
        }
    }


    /// <summary>
    ///   Represents all primitive types (i.e. int, double...) and additionally
    ///   DateTime, TimeSpan, Decimal und enumerations
    ///   Contains no nested properties
    /// </summary>
    /// <remarks>
    ///   See SimpleValueConverter for a list of supported types.
    /// </remarks>
    public sealed class SimpleProperty : Property
    {
        ///<summary>
        ///</summary>
        ///<param name = "name"></param>
        ///<param name = "type"></param>
        public SimpleProperty(string name, Type type)
            : base(name, type)
        {
        }

        ///<summary>
        ///  It could only one of the simple types, see Tools.IsSimple(...)
        ///</summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets the property art.
        /// </summary>
        /// <returns></returns>
        protected override PropertyArt GetPropertyArt()
        {
            return PropertyArt.Simple;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override string ToString()
        {
            string text = base.ToString();
            return Value != null
                       ? string.Format("{0}, ({1})", text, Value)
                       : string.Format("{0}, (null)", text);
        }
    }

    /// <summary>
    ///   Represents complex type which contains properties.
    /// </summary>
    public class ComplexProperty : ReferenceTargetProperty
    {
        private PropertyCollection _properties;

        ///<summary>
        ///</summary>
        ///<param name = "name"></param>
        ///<param name = "type"></param>
        public ComplexProperty(string name, Type type)
            : base(name, type)
        {
        }

        ///<summary>
        ///</summary>
        public PropertyCollection Properties
        {
            get
            {
                if (_properties == null) _properties = new PropertyCollection {Parent = this};
                return _properties;
            }
            set { _properties = value; }
        }


        ///<summary>
        /// Makes flat copy (only references) of vital properties
        ///</summary>
        ///<param name="source"></param>
        public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
        {
            var complexProperty = source as ComplexProperty;
            if (complexProperty == null)
                throw new InvalidCastException(
                    string.Format("Invalid property type to make a flat copy. Expected {0}, current {1}",
                                  typeof(ComplexProperty), source.GetType()));

            base.MakeFlatCopyFrom(source);

            Properties = complexProperty.Properties;
        }

        /// <summary>
        /// Gets the property art.
        /// </summary>
        /// <returns></returns>
        protected override PropertyArt GetPropertyArt()
        {
            return PropertyArt.Complex;
        }
    }

    /// <summary>
    ///   Represents type which is ICollection
    /// </summary>
    public sealed class CollectionProperty : ComplexProperty
    {
        private IList<Property> _items;

        ///<summary>
        ///</summary>
        ///<param name = "name"></param>
        ///<param name = "type"></param>
        public CollectionProperty(string name, Type type)
            : base(name, type)
        {
        }

        ///<summary>
        ///</summary>
        public IList<Property> Items
        {
            get
            {
                if (_items == null) _items = new List<Property>();
                return _items;
            }
            set { _items = value; }
        }

        /// <summary>
        ///   Of what type are items. It's important for polymorphic collection
        /// </summary>
        public Type ElementType { get; set; }

        ///<summary>
        /// Makes flat copy (only references) of vital properties
        ///</summary>
        ///<param name="source"></param>
        public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
        {
            var collectionSource = source as CollectionProperty;
            if (collectionSource == null)
                throw new InvalidCastException(
                    string.Format("Invalid property type to make a flat copy. Expected {0}, current {1}",
                                  typeof(CollectionProperty), source.GetType()));

            base.MakeFlatCopyFrom(source);

            ElementType = collectionSource.ElementType;
            Items = collectionSource.Items;
        }

        /// <summary>
        /// Gets the property art.
        /// </summary>
        /// <returns></returns>
        protected override PropertyArt GetPropertyArt()
        {
            return PropertyArt.Collection;
        }
    }


    /// <summary>
    ///   Represents dictionary. Every item is composed of the key and value
    /// </summary>
    public sealed class DictionaryProperty : ComplexProperty
    {
        private IList<KeyValueItem> _items;

        ///<summary>
        ///</summary>
        ///<param name = "name"></param>
        ///<param name = "type"></param>
        public DictionaryProperty(string name, Type type)
            : base(name, type)
        {
        }

        ///<summary>
        ///</summary>
        public IList<KeyValueItem> Items
        {
            get
            {
                if (_items == null) _items = new List<KeyValueItem>();
                return _items;
            }
            set { _items = value; }
        }

        /// <summary>
        ///   Of what type are keys
        /// </summary>
        public Type KeyType { get; set; }

        /// <summary>
        ///   Of what type are values
        /// </summary>
        public Type ValueType { get; set; }

        ///<summary>
        /// Makes flat copy (only references) of vital properties
        ///</summary>
        ///<param name="source"></param>
        public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
        {
            var dictionarySource = source as DictionaryProperty;
            if (dictionarySource == null)
                throw new InvalidCastException(
                    string.Format("Invalid property type to make a flat copy. Expected {0}, current {1}",
                                  typeof(DictionaryProperty), source.GetType()));

            base.MakeFlatCopyFrom(source);

            KeyType = dictionarySource.KeyType;
            ValueType = dictionarySource.ValueType;
            Items = dictionarySource.Items;
        }

        /// <summary>
        /// Gets the property art.
        /// </summary>
        /// <returns></returns>
        protected override PropertyArt GetPropertyArt()
        {
            return PropertyArt.Dictionary;
        }
    }

    /// <summary>
    ///   Represents one dimensional array
    /// </summary>
    public sealed class SingleDimensionalArrayProperty : ReferenceTargetProperty
    {
        private PropertyCollection _items;

        ///<summary>
        ///</summary>
        ///<param name = "name"></param>
        ///<param name = "type"></param>
        public SingleDimensionalArrayProperty(string name, Type type)
            : base(name, type)
        {
        }

        ///<summary>
        ///</summary>
        public PropertyCollection Items
        {
            get
            {
                if (_items == null) _items = new PropertyCollection {Parent = this};
                return _items;
            }
            set { _items = value; }
        }

        /// <summary>
        ///   As default is 0, but there can be higher start index
        /// </summary>
        public int LowerBound { get; set; }

        /// <summary>
        ///   Of what type are elements
        /// </summary>
        public Type ElementType { get; set; }


        ///<summary>
        /// Makes flat copy (only references) of vital properties
        ///</summary>
        ///<param name="source"></param>
        public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
        {
            var arrayProp = source as SingleDimensionalArrayProperty;
            if (arrayProp == null)
                throw new InvalidCastException(
                    string.Format("Invalid property type to make a flat copy. Expected {0}, current {1}",
                                  typeof(SingleDimensionalArrayProperty), source.GetType()));

            base.MakeFlatCopyFrom(source);

            LowerBound = arrayProp.LowerBound;
            ElementType = arrayProp.ElementType;
            Items = arrayProp.Items;
        }

        /// <summary>
        /// Gets the property art.
        /// </summary>
        /// <returns></returns>
        protected override PropertyArt GetPropertyArt()
        {
            return PropertyArt.SingleDimensionalArray;
        }
    }

    /// <summary>
    ///   Represents multidimensional array. Array properties are in DimensionInfos
    /// </summary>
    public sealed class MultiDimensionalArrayProperty : ReferenceTargetProperty
    {
        private IList<DimensionInfo> _dimensionInfos;
        private IList<MultiDimensionalArrayItem> _items;

        ///<summary>
        ///</summary>
        ///<param name = "name"></param>
        ///<param name = "type"></param>
        public MultiDimensionalArrayProperty(string name, Type type)
            : base(name, type)
        {
        }

        ///<summary>
        ///</summary>
        public IList<MultiDimensionalArrayItem> Items
        {
            get
            {
                if (_items == null) _items = new List<MultiDimensionalArrayItem>();
                return _items;
            }
            set { _items = value; }
        }

        /// <summary>
        ///   Information about the array
        /// </summary>
        public IList<DimensionInfo> DimensionInfos
        {
            get
            {
                if (_dimensionInfos == null) _dimensionInfos = new List<DimensionInfo>();
                return _dimensionInfos;
            }
            set { _dimensionInfos = value; }
        }

        /// <summary>
        ///   Of what type are elements. All elements in all all dimensions must be inheritors of this type.
        /// </summary>
        public Type ElementType { get; set; }

        ///<summary>
        /// Makes flat copy (only references) of vital properties
        ///</summary>
        ///<param name="source"></param>
        public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
        {
            var arrayProp = source as MultiDimensionalArrayProperty;
            if (arrayProp == null)
                throw new InvalidCastException(
                    string.Format("Invalid property type to make a flat copy. Expected {0}, current {1}",
                                  typeof(SingleDimensionalArrayProperty), source.GetType()));

            base.MakeFlatCopyFrom(source);

            ElementType = arrayProp.ElementType;
            DimensionInfos = arrayProp.DimensionInfos;
            Items = arrayProp.Items;
        }

        /// <summary>
        /// Gets the property art.
        /// </summary>
        /// <returns></returns>
        protected override PropertyArt GetPropertyArt()
        {
            return PropertyArt.MultiDimensionalArray;
        }
    }

    /// <summary>
    ///   Information about one item in a multidimensional array
    /// </summary>
    public sealed class MultiDimensionalArrayItem
    {
        ///<summary>
        ///</summary>
        ///<param name = "indexes"></param>
        ///<param name = "value"></param>
        public MultiDimensionalArrayItem(int[] indexes, Property value)
        {
            Indexes = indexes;
            Value = value;
        }

        /// <summary>
        ///   Represents item coordinates in the array (i.e. [1,5,3] - item has index 1 in the dimension 0, index 5 in the dimension 1 and index 3 in the dimension 2).
        /// </summary>
        public int[] Indexes { get; set; }

        /// <summary>
        ///   Item value. It can contain any type.
        /// </summary>
        public Property Value { get; set; }
    }

    /// <summary>
    ///   Every array is composed of dimensions. Singledimensional arrays have only one info,
    ///   multidimensional have more dimension infos.
    /// </summary>
    public sealed class DimensionInfo
    {
        /// <summary>
        ///   Start index for the array
        /// </summary>
        public int LowerBound { get; set; }

        /// <summary>
        ///   How many items are in this dimension
        /// </summary>
        public int Length { get; set; }
    }

    /// <summary>
    ///   Represents one item from the dictionary, a key-value pair.
    /// </summary>
    public sealed class KeyValueItem
    {
        ///<summary>
        ///</summary>
        ///<param name = "key"></param>
        ///<param name = "value"></param>
        public KeyValueItem(Property key, Property value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        ///   Represents key. There can be everything
        /// </summary>
        public Property Key { get; set; }

        /// <summary>
        ///   Represents value. There can be everything
        /// </summary>
        public Property Value { get; set; }
    }

    ///<summary>
    /// Of what art is the property
    ///</summary>
    public enum PropertyArt
    {
        ///<summary>
        ///</summary>
        Unknown = 0,
        ///<summary>
        ///</summary>
        Simple,
        ///<summary>
        ///</summary>
        Complex,
        ///<summary>
        ///</summary>
        Collection,
        ///<summary>
        ///</summary>
        Dictionary,
        ///<summary>
        ///</summary>
        SingleDimensionalArray,
        ///<summary>
        ///</summary>
        MultiDimensionalArray,
        ///<summary>
        ///</summary>
        Null,
        ///<summary>
        ///</summary>
        Reference
    }
}