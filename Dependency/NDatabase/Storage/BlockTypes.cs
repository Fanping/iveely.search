namespace NDatabase.Storage
{
    /// <summary>
    ///   All Block Types of the ODB database format.
    /// </summary>
    internal static class BlockTypes
    {
        public const byte BlockTypeClassHeader = 1;

        public const byte BlockTypeClassBody = 2;

        public const byte BlockTypeNativeObject = 3;

        public const byte BlockTypeNonNativeObject = 4;

        private const byte BlockTypePointer = 5;

        public const byte BlockTypeDeleted = 6;

        private const byte BlockTypeNonNativeNullObject = 7;

        public const byte BlockTypeNativeNullObject = 77;

        public const byte BlockTypeArrayObject = 9;

        public const byte BlockTypeIds = 20;

        public static bool IsClassHeader(int blockType)
        {
            return blockType == BlockTypeClassHeader;
        }

        public static bool IsClassBody(int blockType)
        {
            return blockType == BlockTypeClassBody;
        }

        public static bool IsPointer(int blockType)
        {
            return blockType == BlockTypePointer;
        }

        public static bool IsNullNativeObject(int blockType)
        {
            return blockType == BlockTypeNativeNullObject;
        }

        public static bool IsNullNonNativeObject(int blockType)
        {
            return blockType == BlockTypeNonNativeNullObject;
        }

        public static bool IsDeletedObject(int blockType)
        {
            return blockType == BlockTypeDeleted;
        }

        public static bool IsNative(int blockType)
        {
            return blockType == BlockTypeArrayObject || blockType == BlockTypeNativeObject;
        }

        public static bool IsNonNative(int blockType)
        {
            return blockType == BlockTypeNonNativeObject;
        }

        public static bool IsNull(byte blockType)
        {
            return blockType == BlockTypeNativeNullObject || blockType == BlockTypeNonNativeNullObject;
        }
    }
}