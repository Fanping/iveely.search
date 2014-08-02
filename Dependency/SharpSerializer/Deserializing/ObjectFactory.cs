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
using System.Reflection;
using Iveely.Dependency.Polenter.Serialization.Core;

namespace Iveely.Dependency.Polenter.Serialization.Deserializing
{
    /// <summary>
    ///   Takes Property and converts it to an object
    /// </summary>
    public sealed class ObjectFactory
    {
        private readonly object[] _emptyObjectArray = new object[0];

        /// <summary>
        /// Contains already created objects. Is used for reference resolving.
        /// </summary>
        private readonly Dictionary<int, object > _objectCache = new Dictionary<int, object>();

        /// <summary>
        ///   Builds object from property
        /// </summary>
        /// <param name = "property"></param>
        /// <returns></returns>
        public object CreateObject(Property property)
        {
            if (property == null) throw new ArgumentNullException("property");

            // Is it NullProperty?
            var nullProperty = property as NullProperty;
            if (nullProperty != null)
            {
                return null;
            }

            if (property.Type == null)
            {
                // there is no property type and no expected type defined. Give up!
                throw new InvalidOperationException(string.Format("Property type is not defined. Property: \"{0}\"",
                                                                  property.Name));
            }

            // Is it SimpleProperty?
            var simpleProperty = property as SimpleProperty;
            if (simpleProperty != null)
            {
                return createObjectFromSimpleProperty(simpleProperty);
            }

            var referenceTarget = property as ReferenceTargetProperty;
            if (referenceTarget == null)
                return null;

            if (referenceTarget.Reference != null)
            {
                if (!referenceTarget.Reference.IsProcessed)
                {
                    // object was created already
                    // get object from cache
                    return _objectCache[referenceTarget.Reference.Id];                    
                }
            }

            object value = createObjectCore(referenceTarget);
            if (value != null)
                return value;

            // No idea what it is
            throw new InvalidOperationException(string.Format("Unknown Property type: {0}", property.GetType().Name));                
        }

        private object createObjectCore(ReferenceTargetProperty property)
        {
            // Is it multidimensional array?
            var multiDimensionalArrayProperty = property as MultiDimensionalArrayProperty;
            if (multiDimensionalArrayProperty != null)
            {
                return createObjectFromMultidimensionalArrayProperty(multiDimensionalArrayProperty);
            }

            // Is it singledimensional array?
            var singleDimensionalArrayProperty = property as SingleDimensionalArrayProperty;
            if (singleDimensionalArrayProperty != null)
            {
                return createObjectFromSingleDimensionalArrayProperty(singleDimensionalArrayProperty);
            }

            // Is it dictionary?
            var dictionaryProperty = property as DictionaryProperty;
            if (dictionaryProperty != null)
            {
                return createObjectFromDictionaryProperty(dictionaryProperty);
            }

            // Is it collection?
            var collectionProperty = property as CollectionProperty;
            if (collectionProperty != null)
            {
                return createObjectFromCollectionProperty(collectionProperty);
            }

            // Is it complex type? Class? Structure?
            var complexProperty = property as ComplexProperty;
            if (complexProperty != null)
            {
                return createObjectFromComplexProperty(complexProperty);
            }

            return null;
        }

        private static object createObjectFromSimpleProperty(SimpleProperty property)
        {
            return property.Value;
        }

        private object createObjectFromComplexProperty(ComplexProperty property)
        {
            object obj = Tools.CreateInstance(property.Type);

            if (property.Reference != null)
            {
                // property has Reference, only objects referenced multiple times
                // have properties with references. Object must be cached to
                // resolve its references in the future.
                _objectCache.Add(property.Reference.Id, obj);
            }

            fillProperties(obj, property.Properties);

            return obj;
        }


        private object createObjectFromCollectionProperty(CollectionProperty property)
        {
            Type type = property.Type;
            object collection = Tools.CreateInstance(type);

            if (property.Reference != null)
            {
                // property has Reference, only objects referenced multiple times
                // have properties with references. Object must be cached to
                // resolve its references in the future.
                _objectCache.Add(property.Reference.Id, collection);
            }

            // fill the properties
            fillProperties(collection, property.Properties);

            // Fill the items but only if the "Add" method was found, which has only 1 parameter
            MethodInfo methodInfo = collection.GetType().GetMethod("Add");
            if (methodInfo != null)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length == 1)
                {
                    foreach (Property item in property.Items)
                    {
                        object value = CreateObject(item);
                        methodInfo.Invoke(collection, new[] {value});
                    }
                }
            }
            return collection;
        }

        /// <summary>
        ///   Items will be added only if the "Add" method was found, which exactly 2 parameters (key, value) has
        /// </summary>
        /// <param name = "property"></param>
        /// <returns></returns>
        private object createObjectFromDictionaryProperty(DictionaryProperty property)
        {
            object dictionary = Tools.CreateInstance(property.Type);

            if (property.Reference != null)
            {
                // property has Reference, only objects referenced multiple times
                // have properties with references. Object must be cached to
                // resolve its references in the future.
                _objectCache.Add(property.Reference.Id, dictionary);
            }

            // fill the properties
            fillProperties(dictionary, property.Properties);

            // fill items, but only if Add(key, value) was found
            MethodInfo methodInfo = dictionary.GetType().GetMethod("Add");
            if (methodInfo != null)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length == 2)
                {
                    foreach (KeyValueItem item in property.Items)
                    {
                        object keyValue = CreateObject(item.Key);
                        object valueValue = CreateObject(item.Value);

                        methodInfo.Invoke(dictionary, new[] {keyValue, valueValue});
                    }
                }
            }


            return dictionary;
        }

        /// <summary>
        ///   Fill properties of the class or structure
        /// </summary>
        /// <param name = "obj"></param>
        /// <param name = "properties"></param>
        private void fillProperties(object obj, IEnumerable<Property> properties)
        {
            foreach (Property property in properties)
            {
                PropertyInfo propertyInfo = obj.GetType().GetProperty(property.Name);
                if (propertyInfo == null) continue;

                object value = CreateObject(property);
                if (value == null) continue;

                propertyInfo.SetValue(obj, value, _emptyObjectArray);
            }
        }

        private object createObjectFromSingleDimensionalArrayProperty(SingleDimensionalArrayProperty property)
        {
            int itemsCount = property.Items.Count;

            Array array = createArrayInstance(property.ElementType, new[] {itemsCount}, new[] {property.LowerBound});

            if (property.Reference != null)
            {
                // property has Reference, only objects referenced multiple times
                // have properties with references. Object must be cached to
                // resolve its references in the future.
                _objectCache.Add(property.Reference.Id, array);
            }

            // Items
            for (int index = property.LowerBound; index < property.LowerBound + itemsCount; index++)
            {
                Property item = property.Items[index];
                object value = CreateObject(item);
                if (value != null)
                {
                    array.SetValue(value, index);
                }
            }

            return array;
        }

        private object createObjectFromMultidimensionalArrayProperty(MultiDimensionalArrayProperty property)
        {
            // determine array type
            MultiDimensionalArrayCreatingInfo creatingInfo =
                getMultiDimensionalArrayCreatingInfo(property.DimensionInfos);

            // Instantiating the array
            Array array = createArrayInstance(property.ElementType, creatingInfo.Lengths, creatingInfo.LowerBounds);

            if (property.Reference != null)
            {
                // property has Reference, only objects referenced multiple times
                // have properties with references. Object must be cached to
                // resolve its references in the future.
                _objectCache.Add(property.Reference.Id, array);
            }

            // fill the values
            foreach (MultiDimensionalArrayItem item in property.Items)
            {
                object value = CreateObject(item.Value);
                if (value != null)
                {
                    array.SetValue(value, item.Indexes);
                }
            }

            return array;
        }

        private static Array createArrayInstance(Type elementType, int[] lengths, int[] lowerBounds)
        {
#if Smartphone
            return Array.CreateInstance(elementType, lengths);
#elif SILVERLIGHT
            return Array.CreateInstance(elementType, lengths);
#else
            return Array.CreateInstance(elementType, lengths, lowerBounds);
#endif
        }

        /// <summary>
        ///   This internal class helps to instantiate the multidimensional array
        /// </summary>
        /// <param name = "infos"></param>
        /// <returns></returns>
        private static MultiDimensionalArrayCreatingInfo getMultiDimensionalArrayCreatingInfo(
            IEnumerable<DimensionInfo> infos)
        {
            var lengths = new List<int>();
            var lowerBounds = new List<int>();
            foreach (DimensionInfo info in infos)
            {
                lengths.Add(info.Length);
                lowerBounds.Add(info.LowerBound);
            }

            var result = new MultiDimensionalArrayCreatingInfo();
            result.Lengths = lengths.ToArray();
            result.LowerBounds = lowerBounds.ToArray();
            return result;
        }

        #region Nested type: MultiDimensionalArrayCreatingInfo

        private class MultiDimensionalArrayCreatingInfo
        {
            public int[] Lengths { get; set; }
            public int[] LowerBounds { get; set; }
        }

        #endregion
    }
}