using System;
using System.Collections.ObjectModel;

namespace Iveely.Dependency.Polenter.Serialization.Serializing
{
    ///<summary>
    ///</summary>
    public sealed class TypeInfoCollection : KeyedCollection<Type, TypeInfo>
    {
        /// <summary>
        /// </summary>
        /// <returns>null if the key was not found</returns>
        public TypeInfo TryGetTypeInfo(Type type)
        {
            if (!Contains(type))
            {
                return null;
            }
            return this[type];
        }

        /// <summary>
        /// </summary>
        /// <param name = "item"></param>
        /// <returns></returns>
        protected override Type GetKeyForItem(TypeInfo item)
        {
            return item.Type;
        }
    }
}