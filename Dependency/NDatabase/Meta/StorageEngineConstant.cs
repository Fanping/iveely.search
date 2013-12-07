using NDatabase.Api;
using NDatabase.Exceptions;

namespace NDatabase.Meta
{
    /// <summary>
    ///   Some Storage engine constants about offset position for object writing/reading.
    /// </summary>
    internal static class StorageEngineConstant
    {
        internal const int NbIdsPerBlock = 1000;

        internal const int IdBlockRepetitionSize = 18;

        /// <summary>
        /// Default max number of write object actions per transaction - 10 000
        /// </summary>
        internal const int MaxNumberOfWriteObjectPerTransaction = 10000;

        /// <summary>
        ///   header(34) + 1000 * 18
        /// </summary>
        internal const int IdBlockSize = 34 + NbIdsPerBlock * IdBlockRepetitionSize;

        internal const long NullObjectIdId = 0;

        internal const long DeletedObjectPosition = 0;

        internal const long NullObjectPosition = 0;

        internal const long ObjectIsNotInCache = -1;

        internal const long ObjectDoesNotExist = -2;

        /// <summary>
        ///   this occurs when a class has been refactored adding a field.
        /// </summary>
        /// <remarks>
        ///   this occurs when a class has been refactored adding a field. Old objects do not the new field
        /// </remarks>
        internal const long FieldDoesNotExist = -1;

        private const int Version30 = 30;

        internal const int CurrentFileFormatVersion = Version30;

        private const long ClassOffsetBlockSize = 0;
        private const long ObjectOffsetBlockSize = 0;

        /// <summary>
        ///   pull id type (byte),id(long),
        /// </summary>
        private const long BlockIdRepetitionIdType = 0;

        private const long NativeObjectOffsetBlockSize = 0;
        
        /// <summary>
        ///   Used to make an attribute reference a null object - setting its id to zero
        /// </summary>
        internal static readonly OID NullObjectId = null;

        /// <summary>
        ///   File format version : 1 int (4 bytes)
        /// </summary>
        internal const int DatabaseHeaderVersionPosition = 0;

        /// <summary>
        ///   The Database ID : 4 Long (4*8 bytes)
        /// </summary>
        internal static readonly int DatabaseHeaderDatabaseIdPosition = OdbType.Integer.Size;

        /// <summary>
        ///   The last Transaction ID 2 long (2*4*8 bytes)
        /// </summary>
        internal static readonly int DatabaseHeaderLastTransactionId = DatabaseHeaderDatabaseIdPosition +
                                                                          4 * OdbType.Long.Size;

        /// <summary>
        ///   The number of classes in the meta model 1 long (4*8 bytes)
        /// </summary>
        internal static readonly int DatabaseHeaderNumberOfClassesPosition = DatabaseHeaderLastTransactionId +
                                                                           2 * OdbType.Long.Size;

        /// <summary>
        ///   The first class OID : 1 Long (8 bytes)
        /// </summary>
        internal static readonly int DatabaseHeaderFirstClassOid = DatabaseHeaderNumberOfClassesPosition +
                                                                 OdbType.Long.Size;

        /// <summary>
        ///   The last ODB close status.
        /// </summary>
        /// <remarks>
        ///   The last ODB close status. Used to detect if the transaction is ok : 1 byte
        /// </remarks>
        internal static readonly int DatabaseHeaderLastCloseStatusPosition = DatabaseHeaderFirstClassOid +
                                                                           OdbType.Long.Size;

        internal static readonly int DatabaseHeaderEmptySpaceWhichCouldBeUsedInTheFuture =
            DatabaseHeaderLastCloseStatusPosition + OdbType.Byte.Size;

        /// <summary>
        ///   The Database character encoding : 50 bytes
        /// </summary>
        internal static readonly int DatabaseHeaderDatabaseCharacterEncodingPosition =
            DatabaseHeaderEmptySpaceWhichCouldBeUsedInTheFuture + 120 * OdbType.Byte.Size;

        /// <summary>
        ///   The position of the current id block: 1 long
        /// </summary>
        internal static readonly int DatabaseHeaderCurrentIdBlockPosition =
            DatabaseHeaderDatabaseCharacterEncodingPosition + 58 * OdbType.Byte.Size;
            

        /// <summary>
        ///   First ID Block position
        /// </summary>
        internal static readonly int DatabaseHeaderFirstIdBlockPosition = DatabaseHeaderCurrentIdBlockPosition +
                                                                        OdbType.Long.Size;

        internal static readonly int DatabaseHeaderProtectedZoneSize = DatabaseHeaderCurrentIdBlockPosition;

        internal static readonly int[] DatabaseHeaderPositions = new[]
            {
                DatabaseHeaderVersionPosition, DatabaseHeaderDatabaseIdPosition, DatabaseHeaderLastTransactionId,
                DatabaseHeaderNumberOfClassesPosition, DatabaseHeaderFirstClassOid,
                DatabaseHeaderLastCloseStatusPosition, DatabaseHeaderDatabaseCharacterEncodingPosition, DatabaseHeaderEmptySpaceWhichCouldBeUsedInTheFuture
            };

        private static readonly long ClassOffsetBlockType = ClassOffsetBlockSize + OdbType.Integer.Size;

        private static readonly long ClassOffsetCategory = ClassOffsetBlockType + OdbType.Byte.Size;

        private static readonly long ClassOffsetId = ClassOffsetCategory + OdbType.Byte.Size;

        private static readonly long ClassOffsetPreviousClassPosition = ClassOffsetId + OdbType.Long.Size;

        internal static readonly long ClassOffsetNextClassPosition = ClassOffsetPreviousClassPosition +
                                                                   OdbType.Long.Size;

        internal static readonly long ClassOffsetClassNbObjects = ClassOffsetNextClassPosition + OdbType.Long.Size;

        private static readonly long ObjectOffsetBlockType = ObjectOffsetBlockSize + OdbType.Integer.Size;

        private static readonly long ObjectOffsetObjectId = ObjectOffsetBlockType + OdbType.Byte.Size;

        private static readonly long ObjectOffsetClassInfoId = ObjectOffsetObjectId + OdbType.Long.Size;

        internal static readonly long ObjectOffsetPreviousObjectOid = ObjectOffsetClassInfoId + OdbType.Long.Size;

        internal static readonly long ObjectOffsetNextObjectOid = ObjectOffsetPreviousObjectOid + OdbType.Long.Size;


        /// <summary>
        ///   <pre>ID Block Header :
        ///     Block size             : 1 int
        ///     Block type             : 1 byte
        ///     Block status           : 1 byte
        ///     Prev block position    : 1 long
        ///     Next block position    : 1 long
        ///     Block number           : 1 int
        ///     Max id                 : 1 long
        ///     Total size = 34</pre>
        /// </summary>
        internal static readonly long BlockIdOffsetForBlockStatus = OdbType.Integer.Size + OdbType.Byte.Size;

        private static readonly long BlockIdOffsetForPrevBlock = BlockIdOffsetForBlockStatus + OdbType.Byte.Size;

        internal static readonly long BlockIdOffsetForNextBlock = BlockIdOffsetForPrevBlock + OdbType.Long.Size;

        internal static readonly long BlockIdOffsetForBlockNumber = BlockIdOffsetForNextBlock + OdbType.Long.Size;

        internal static readonly long BlockIdOffsetForMaxId = BlockIdOffsetForBlockNumber + OdbType.Integer.Size;

        internal static readonly long BlockIdOffsetForStartOfRepetition = BlockIdOffsetForMaxId + OdbType.Long.Size;

        private static readonly long BlockIdRepetitionId = BlockIdRepetitionIdType + OdbType.Byte.Size;

        internal static readonly long BlockIdRepetitionIdStatus = BlockIdRepetitionId + OdbType.Long.Size;

        internal static readonly long NativeObjectOffsetBlockType = NativeObjectOffsetBlockSize +
                                                                  OdbType.Integer.Size;

        // ********************************************************
        // DATABASE HEADER
        // ********************************************************
        // **********************************************************
        // END OF DATABASE HEADER
        // *********************************************************
        // CLASS OFFSETS
        // OBJECT OFFSETS - update this section when modifying the odb file format 

        internal static void CheckDbVersionCompatibility(int version)
        {
            var versionIsCompatible = version == CurrentFileFormatVersion;

            if (!versionIsCompatible)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.RuntimeIncompatibleVersion.AddParameter(version).AddParameter(
                        CurrentFileFormatVersion));
            }
        }

        internal static long GetIdBlockNumberOfOid(OID oid)
        {
            long number;
            var objectId = oid.ObjectId;
            if (objectId % NbIdsPerBlock == 0)
                number = objectId / NbIdsPerBlock;
            else
                number = objectId / NbIdsPerBlock + 1;
            return number;
        }
    }
}
