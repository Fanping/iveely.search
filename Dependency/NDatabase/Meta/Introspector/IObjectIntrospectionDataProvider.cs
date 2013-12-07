using System;

namespace NDatabase.Meta.Introspector
{
    internal interface IObjectIntrospectionDataProvider
    {
        ClassInfo GetClassInfo(Type type);
        void Clear();
        NonNativeObjectInfo EnrichWithOid(NonNativeObjectInfo nnoi, object o);
    }
}