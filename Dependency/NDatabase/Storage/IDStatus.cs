namespace NDatabase.Storage
{
    internal static class IDStatus
    {
        public const byte Active = 1;

        public const byte Deleted = 2;

        public static bool IsActive(byte status)
        {
            return status == Active;
        }
    }
}