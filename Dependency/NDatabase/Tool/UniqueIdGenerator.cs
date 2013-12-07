using NDatabase.Tool.Wrappers;

namespace NDatabase.Tool
{
    internal static class UniqueIdGenerator
    {
        internal static long GetRandomLongId()
        {
            lock (typeof (UniqueIdGenerator))
            {
                return (long) (OdbRandom.GetRandomDouble() * long.MaxValue);
            }
        }
    }
}
