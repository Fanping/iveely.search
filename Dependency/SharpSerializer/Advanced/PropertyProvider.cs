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
using System.Collections.ObjectModel;
using System.Reflection;
using Iveely.Dependency.Polenter.Serialization.Serializing;

namespace Iveely.Dependency.Polenter.Serialization.Advanced
{
    /// <summary>
    /// Provides properties to serialize from source object. Implements the strategy 
    /// which subproperties to use and 
    /// wich to ignore and
    /// how to travese the source object to get subproperties
    ///   
    /// Its methods GetAllProperties and IgnoreProperty can be
    ///   overwritten in an inherited class to customize its functionality. 
    ///   Its property PropertiesToIgnore contains properties, which are ignored during the serialization.
    /// </summary>
    public class PropertyProvider
    {
        private PropertiesToIgnore _propertiesToIgnore;
        private IList<Type> _attributesToIgnore;
#if !Smartphone
        [ThreadStatic]
#endif
        private static PropertyCache _cache;

        /// <summary>
        ///   Which properties should be ignored
        /// </summary>
        /// <remarks>
        /// Sometimes you want to ignore some properties during the serialization.
        /// If they are parts of your own business objects, you can mark these properties with ExcludeFromSerializationAttribute. 
        /// However it is not possible to mark them in the built in .NET classes
        /// In such a case you add these properties to the list PropertiesToIgnore.
        /// I.e. System.Collections.Generic.List"string" has the "Capacity" property which is irrelevant for
        /// the whole Serialization and should be ignored
        /// PropertyProvider.PropertiesToIgnore.Add(typeof(List"string"), "Capacity")
        /// </remarks>
        public PropertiesToIgnore PropertiesToIgnore
        {
            get
            {
                if (_propertiesToIgnore == null) _propertiesToIgnore = new PropertiesToIgnore();
                return _propertiesToIgnore;
            }
            set { _propertiesToIgnore = value; }
        }

        /// <summary>
        /// All Properties markt with one of the contained attribute-types will be ignored on save.
        /// </summary>
        public IList<Type> AttributesToIgnore
        {
            get
            {
                if (_attributesToIgnore==null)_attributesToIgnore=new List<Type>();
                return _attributesToIgnore;
            }
            set { _attributesToIgnore = value; }
        }

        /// <summary>
        ///   Gives all properties back which:
        ///   - are public
        ///   - are not static
        ///   - does not contain ExcludeFromSerializationAttribute
        ///   - have their set and get accessors
        ///   - are not indexers
        /// </summary>
        /// <param name = "typeInfo"></param>
        /// <returns></returns>
        public IList<PropertyInfo> GetProperties(Polenter.Serialization.Serializing.TypeInfo typeInfo)
        {
            // Search in cache
            var propertyInfos = Cache.TryGetPropertyInfos(typeInfo.Type);
            if (propertyInfos != null)
            {
                return propertyInfos;
            }

            // Creating infos
            PropertyInfo[] properties = GetAllProperties(typeInfo.Type);
            var result = new List<PropertyInfo>();

            foreach (PropertyInfo property in properties)
            {
                if (!IgnoreProperty(typeInfo, property))
                {
                    result.Add(property);
                }
            }

            // adding result to Cache
            Cache.Add(typeInfo.Type, result);

            return result;
        }

        /// <summary>
        ///   Should the property be removed from serialization?
        /// </summary>
        /// <param name = "info"></param>
        /// <param name = "property"></param>
        /// <returns>
        ///   true if the property:
        ///   - is in the PropertiesToIgnore,
        ///   - contains ExcludeFromSerializationAttribute,
        ///   - does not have it's set or get accessor
        ///   - is indexer
        /// </returns>
        protected virtual bool IgnoreProperty(Polenter.Serialization.Serializing.TypeInfo info, PropertyInfo property)
        {
            // Soll die Eigenschaft ignoriert werden
            if (PropertiesToIgnore.Contains(info.Type, property.Name))
            {
                return true;
            }

            if (ContainsExcludeFromSerializationAttribute(property))
            {
                return true;
            }

            if (!property.CanRead || !property.CanWrite)
            {
                return true;
            }

            ParameterInfo[] indexParameters = property.GetIndexParameters();
            if (indexParameters.Length > 0)
            {
                // Indexer
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether <paramref name="property"/> is excluded from serialization or not.
        /// </summary>
        /// <param name="property">The property to be checked.</param>
        /// <returns>
        /// 	<c>true</c> if no serialization
        /// </returns>
        protected bool ContainsExcludeFromSerializationAttribute(ICustomAttributeProvider property)
        {
            foreach (Type attrType in AttributesToIgnore)
            {
                object[] attributes = property.GetCustomAttributes(attrType, false);
                if (attributes.Length > 0) 
                    return true;
            }
            return false;
        }

        /// <summary>
        ///   Gives all properties back which:
        ///   - are public
        ///   - are not static (instance properties)
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        protected virtual PropertyInfo[] GetAllProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        private static PropertyCache Cache
        {
            get
            {
                if (_cache==null)
                {
                    _cache=new PropertyCache();
                }
                return _cache;
            }
        }
    }

    /// <summary>
    ///   Cache which contains type as a key, and all associated property names
    /// </summary>
    public sealed class PropertiesToIgnore
    {
        private readonly TypePropertiesToIgnoreCollection _propertiesToIgnore = new TypePropertiesToIgnoreCollection();

        ///<summary>
        ///</summary>
        ///<param name = "type"></param>
        ///<param name = "propertyName"></param>
        public void Add(Type type, string propertyName)
        {
            TypePropertiesToIgnore item = getPropertiesToIgnore(type);
            if (!item.PropertyNames.Contains(propertyName))
            {
                item.PropertyNames.Add(propertyName);
            }
        }

        private TypePropertiesToIgnore getPropertiesToIgnore(Type type)
        {
            TypePropertiesToIgnore item = _propertiesToIgnore.TryFind(type);
            if (item == null)
            {
                item = new TypePropertiesToIgnore(type);
                _propertiesToIgnore.Add(item);
            }
            return item;
        }

        ///<summary>
        ///</summary>
        ///<param name = "type"></param>
        ///<param name = "propertyName"></param>
        ///<returns></returns>
        public bool Contains(Type type, string propertyName)
        {
            return _propertiesToIgnore.ContainsProperty(type, propertyName);
        }

        #region Nested type: TypePropertiesToIgnore

        private sealed class TypePropertiesToIgnore
        {
            private IList<string> _propertyNames;

            public TypePropertiesToIgnore(Type type)
            {
                Type = type;
            }

            public Type Type { get; set; }

            public IList<string> PropertyNames
            {
                get
                {
                    if (_propertyNames == null) _propertyNames = new List<string>();
                    return _propertyNames;
                }
                set { _propertyNames = value; }
            }
        }

        #endregion

        #region Nested type: TypePropertiesToIgnoreCollection

        private sealed class TypePropertiesToIgnoreCollection : KeyedCollection<Type, TypePropertiesToIgnore>
        {
            protected override Type GetKeyForItem(TypePropertiesToIgnore item)
            {
                return item.Type;
            }

            public int IndexOf(Type type)
            {
                for (int i = 0; i < Count; i++)
                {
                    TypePropertiesToIgnore item = this[i];
                    if (item.Type == type)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public TypePropertiesToIgnore TryFind(Type type)
            {
                foreach (TypePropertiesToIgnore item in Items)
                {
                    if (item.Type == type) return item;
                }
                return null;
            }

            public bool ContainsProperty(Type type, string propertyName)
            {
                TypePropertiesToIgnore propertiesToIgnore = TryFind(type);
                if (propertiesToIgnore == null) return false;
                return propertiesToIgnore.PropertyNames.Contains(propertyName);
            }
        }

        #endregion
    }
}