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
using System.Text.RegularExpressions;
using Iveely.Dependency.Polenter.Serialization.Advanced.Serializing;

namespace Iveely.Dependency.Polenter.Serialization.Advanced
{
    /// <summary>
    ///   Converts Type to its text representation and vice versa. Since v.2.12 all types serialize to the AssemblyQualifiedName.
    ///   Use overloaded constructor to shorten type names.
    /// </summary>
    public sealed class TypeNameConverter : ITypeNameConverter
    {
        private readonly Dictionary<Type, string> _cache = new Dictionary<Type, string>();

        /// <summary>
        /// Since v.2.12 as default the type name is equal to Type.AssemblyQualifiedName
        /// </summary>
        public TypeNameConverter()
        {
        }

        /// <summary>
        ///   Some values from the Type.AssemblyQualifiedName can be removed
        /// </summary>
        /// <param name = "includeAssemblyVersion"></param>
        /// <param name = "includeCulture"></param>
        /// <param name = "includePublicKeyToken"></param>
        public TypeNameConverter(bool includeAssemblyVersion, bool includeCulture, bool includePublicKeyToken)
        {
            IncludeAssemblyVersion = includeAssemblyVersion;
            IncludeCulture = includeCulture;
            IncludePublicKeyToken = includePublicKeyToken;
        }

        /// <summary>
        ///   Version=x.x.x.x will be inserted to the type name
        /// </summary>
        public bool IncludeAssemblyVersion { get; private set; }

        /// <summary>
        ///   Culture=.... will be inserted to the type name
        /// </summary>
        public bool IncludeCulture { get; private set; }

        /// <summary>
        ///   PublicKeyToken=.... will be inserted to the type name
        /// </summary>
        public bool IncludePublicKeyToken { get; private set; }

        #region ITypeNameConverter Members

        /// <summary>
        ///   Gives type as text
        /// </summary>
        /// <param name = "type"></param>
        /// <returns>string.Empty if the type is null</returns>
        public string ConvertToTypeName(Type type)
        {
            if (type == null) return string.Empty;

            // Search in cache
            if (_cache.ContainsKey(type))
            {
                return _cache[type];
            }

            string typename = type.AssemblyQualifiedName;

            if (!IncludeAssemblyVersion)
            {
                typename = removeAssemblyVersion(typename);
            }

            if (!IncludeCulture)
            {
                typename = removeCulture(typename);
            }

            if (!IncludePublicKeyToken)
            {
                typename = removePublicKeyToken(typename);
            }

            // Adding to cache
            _cache.Add(type, typename);

            return typename;
        }

        /// <summary>
        ///   Gives back Type from the text.
        /// </summary>
        /// <param name = "typeName"></param>
        /// <returns></returns>
        public Type ConvertToType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            Type type = Type.GetType(typeName, true);
            return type;
        }

        #endregion

        private static string removePublicKeyToken(string typename)
        {
            return Regex.Replace(typename, @", PublicKeyToken=\w+", string.Empty);
        }

        private static string removeCulture(string typename)
        {
            return Regex.Replace(typename, @", Culture=\w+", string.Empty);
        }

        private static string removeAssemblyVersion(string typename)
        {
            return Regex.Replace(typename, @", Version=\d+.\d+.\d+.\d+", string.Empty);
        }
    }
}