using NDatabase.Cache;
using NDatabase.Meta;

namespace NDatabase.Core
{
    internal interface IInstanceBuilder
    {
        object BuildOneInstance(NonNativeObjectInfo objectInfo, IOdbCache cache);

        object BuildOneInstance(NonNativeObjectInfo objectInfo);
    }
}