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
using Polenter.Serialization.Core;

namespace Polenter.Serialization.Serializing
{
    /// <summary>
    ///   Gives extended information about a Type
    /// </summary>
    public sealed class TypeInfo
    {
        /// <summary>
        ///   Cache stores type info and spares time be recall the info every time it is needed
        /// </summary>
#if !Smartphone
        [ThreadStatic]
#endif
        private static TypeInfoCollection _cache;

        ///<summary>
        ///</summary>
        public bool IsSimple { get; set; }

        ///<summary>
        ///</summary>
        public bool IsArray { get; set; }

        ///<summary>
        ///</summary>
        public bool IsEnumerable { get; set; }

        ///<summary>
        ///</summary>
        public bool IsCollection { get; set; }

        ///<summary>
        ///</summary>
        public bool IsDictionary { get; set; }

        /// <summary>
        ///   Of what type are elements of Array, Collection or values in a Dictionary
        /// </summary>
        public Type ElementType { get; set; }

        /// <summary>
        ///   Of what type are dictionary keys
        /// </summary>
        public Type KeyType { get; set; }

        /// <summary>
        ///   Valid dimensions start with 1
        /// </summary>
        public int ArrayDimensionCount { get; set; }

        ///<summary>
        ///  Property type
        ///</summary>
        public Type Type { get; set; }

        private static TypeInfoCollection Cache
        {
            get
            {
                if (_cache==null)
                {
                    _cache=new TypeInfoCollection();
                }
                return _cache;
            }
        }


        ///<summary>
        ///</summary>
        ///<param name = "obj"></param>
        ///<returns></returns>
        ///<exception cref = "ArgumentNullException"></exception>
        public static TypeInfo GetTypeInfo(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            Type type = obj.GetType();
            return GetTypeInfo(type);
        }


        ///<summary>
        ///</summary>
        ///<param name = "type"></param>
        ///<returns></returns>
        public static TypeInfo GetTypeInfo(Type type)
        {
            // check if Info is in cache
            TypeInfo typeInfo = Cache.TryGetTypeInfo(type);
            if (typeInfo == null)
            {
                // no info in cache yet
                typeInfo = new TypeInfo();
                typeInfo.Type = type;

                typeInfo.IsSimple = Tools.IsSimple(type);   

                // new since v.2.16
                // check if array of byte
                if (type==typeof(byte[]))
                {
                    typeInfo.ElementType = typeof (byte);
                }

                // Only not simple types can be Collections
                if (!typeInfo.IsSimple)
                {
                    // check if it is an Array
                    typeInfo.IsArray = Tools.IsArray(type);

                    if (typeInfo.IsArray)
                    {
                        // Array? What is its element type?
                        if (type.HasElementType)
                        {
                            typeInfo.ElementType = type.GetElementType();
                        }

                        // How many dimensions
                        typeInfo.ArrayDimensionCount = type.GetArrayRank();
                    }
                    else
                    {
                        // It is not Array, maybe Enumerable?
                        typeInfo.IsEnumerable = Tools.IsEnumerable(type);
                        if (typeInfo.IsEnumerable)
                        {
                            // it is Enumerable maybe Collection?
                            typeInfo.IsCollection = Tools.IsCollection(type);

                            if (typeInfo.IsCollection)
                            {
                                // Sure it is a Collection, but maybe Dictionary also?
                                typeInfo.IsDictionary = Tools.IsDictionary(type);

                                // Fill its key and value types, if the listing is generic
                                bool elementTypeDefinitionFound;
                                var examinedType = type;
                                do
                                {
                                    elementTypeDefinitionFound = fillKeyAndElementType(typeInfo, examinedType);
                                    examinedType = examinedType.BaseType;
                                    // until key and element definition was found, or the base typ is an object
                                } while (!elementTypeDefinitionFound && examinedType!=null && examinedType!=typeof(object));
                            }
                        }
                    }
                }
#if Smartphone
                Cache.AddIfNotExists(typeInfo);
#else
                Cache.Add(typeInfo);
#endif
            }

            return typeInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="type"></param>
        /// <returns>true if the key and value definition was found</returns>
        private static bool fillKeyAndElementType(TypeInfo typeInfo, Type type)
        {
            if (type.IsGenericType)
            {
                Type[] arguments = type.GetGenericArguments();

                if (typeInfo.IsDictionary)
                {
                    // in Dictionary there are keys and values
                    typeInfo.KeyType = arguments[0];
                    typeInfo.ElementType = arguments[1];
                }
                else
                {
                    // In Collection there are only items
                    typeInfo.ElementType = arguments[0];
                }
                return arguments.Length > 0;
            }
            return false;
        }
    }
}