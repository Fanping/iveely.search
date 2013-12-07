using NDatabase.Meta.Introspector;

namespace NDatabase.Meta
{
    internal interface IClassInfoProvider
    {
        IObjectIntrospectionDataProvider GetClassInfoProvider();
    }
}