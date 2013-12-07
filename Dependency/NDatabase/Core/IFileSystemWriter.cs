using NDatabase.Api;
using NDatabase.Core.Session;
using NDatabase.Meta;

namespace NDatabase.Core
{
    internal interface IFileSystemWriter
    {
        void BuildFileSystemInterface(IStorageEngine storageEngine, ISession session);

        /// <summary>
        ///   Write the status of the last odb close
        /// </summary>
        void WriteLastOdbCloseStatus(bool ok, bool writeInTransaction);

        /// <summary>
        ///   Write the number of classes in meta-model
        /// </summary>
        void WriteNumberOfClasses(long number, bool writeInTransaction);

        void WriteOid(OID oid, bool writeInTransaction);

        /// <summary>
        ///   Resets the position of the first class of the metamodel.
        /// </summary>
        /// <remarks>
        ///   Resets the position of the first class of the metamodel. It Happens when database is being refactored
        /// </remarks>
        void WriteFirstClassInfoOID(OID classInfoId, bool inTransaction);

        void Flush();
        IFileSystemInterface FileSystemInterface { get; }

        /// <summary>
        ///   Writes the header of a block of type ID - a block that contains ids of objects and classes
        /// </summary>
        /// <param name="position"> Position at which the block must be written, if -1, take the next available position </param>
        /// <param name="idBlockSize"> The block size in byte </param>
        /// <param name="blockStatus"> The block status </param>
        /// <param name="blockNumber"> The number of the block </param>
        /// <param name="previousBlockPosition"> The position of the previous block of the same type </param>
        /// <param name="writeInTransaction"> To indicate if write must be done in transaction </param>
        /// <returns> The position of the id @ </returns>
        long WriteIdBlock(long position, int idBlockSize, byte blockStatus, int blockNumber,
                          long previousBlockPosition, bool writeInTransaction);

        void Close();

        /// <summary>
        ///   Marks a block of type id as full, changes the status and the next block position
        /// </summary>
        /// <param name="blockPosition"> </param>
        /// <param name="nextBlockPosition"> </param>
        /// <param name="writeInTransaction"> </param>
        /// <returns> The block position @ </returns>
        void MarkIdBlockAsFull(long blockPosition, long nextBlockPosition, bool writeInTransaction);

        /// <summary>
        ///   Creates the header of the file
        /// </summary>
        /// <param name="storageEngine"> </param>
        /// <param name="creationDate"> The creation date </param>
        void CreateEmptyDatabaseHeader(IStorageEngine storageEngine, long creationDate);

        /// <summary>
        ///   Associate an object OID to its position
        /// </summary>
        /// <param name="idType"> The type : can be object or class </param>
        /// <param name="idStatus"> The status of the OID </param>
        /// <param name="currentBlockIdPosition"> The current OID block position </param>
        /// <param name="oid"> The OID </param>
        /// <param name="objectPosition"> The position </param>
        /// <param name="writeInTransaction"> To indicate if write must be executed in transaction </param>
        /// <returns> @ </returns>
        long AssociateIdToObject(byte idType, byte idStatus, long currentBlockIdPosition, OID oid,
                                 long objectPosition, bool writeInTransaction);

        /// <summary>
        ///   Updates the real object position of the object OID
        /// </summary>
        /// <param name="idPosition"> The OID position </param>
        /// <param name="objectPosition"> The real object position </param>
        /// <param name="writeInTransaction"> indicate if write must be done in transaction @ </param>
        void UpdateObjectPositionForObjectOIDWithPosition(long idPosition, long objectPosition,
                                                          bool writeInTransaction);

        /// <summary>
        ///   Udates the real class positon of the class OID
        /// </summary>
        void UpdateClassPositionForClassOIDWithPosition(long idPosition, long objectPosition,
                                                        bool writeInTransaction);

        void UpdateStatusForIdWithPosition(long idPosition, byte newStatus, bool writeInTransaction);

        /// <summary>
        ///   Updates the instance related field of the class info into the database file Updates the number of objects, the first object oid and the next class oid
        /// </summary>
        /// <param name="classInfo"> The class info to be updated </param>
        /// <param name="writeInTransaction"> To specify if it must be part of a transaction @ </param>
        void UpdateInstanceFieldsOfClassInfo(ClassInfo classInfo, bool writeInTransaction);

        void WriteBlockSizeAt(long writePosition, int blockSize, bool writeInTransaction, object @object);

        /// <summary>
        ///   Writes a class attribute info, an attribute of a class
        /// </summary>
        void WriteClassAttributeInfo(IStorageEngine storageEngine, ClassAttributeInfo cai, bool writeInTransaction);
    }
}