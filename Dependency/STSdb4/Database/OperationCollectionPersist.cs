using Iveely.General.Compression;
using Iveely.Data;
using Iveely.Database.Operations;
using Iveely.WaterfallTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iveely.General.Collections;
using Iveely.General.Persist;

namespace Iveely.Database
{
    public class OperationCollectionPersist : IPersist<IOperationCollection>
    {
        public const byte VERSION = 40;

        private readonly Action<BinaryWriter, IOperation>[] writes;
        private readonly Func<BinaryReader, IOperation>[] reads;

        public readonly IPersist<IData> KeyPersist;
        public readonly IPersist<IData> RecordPersist;
        public readonly IOperationCollectionFactory CollectionFactory;

        public OperationCollectionPersist(IPersist<IData> keyPersist, IPersist<IData> recordPersist, IOperationCollectionFactory collectionFactory)
        {
            KeyPersist = keyPersist;
            RecordPersist = recordPersist;

            writes = new Action<BinaryWriter, IOperation>[OperationCode.MAX];
            writes[OperationCode.REPLACE] = WriteReplaceOperation;
            writes[OperationCode.INSERT_OR_IGNORE] = WriteInsertOrIgnoreOperation;
            writes[OperationCode.DELETE] = WriteDeleteOperation;
            writes[OperationCode.DELETE_RANGE] = WriteDeleteRangeOperation;
            writes[OperationCode.CLEAR] = WriteClearOperation;

            reads = new Func<BinaryReader, IOperation>[OperationCode.MAX];
            reads[OperationCode.REPLACE] = ReadReplaceOperation;
            reads[OperationCode.INSERT_OR_IGNORE] = ReadInsertOrIgnoreOperation;
            reads[OperationCode.DELETE] = ReadDeleteOperation;
            reads[OperationCode.DELETE_RANGE] = ReadDeleteRangeOperation;
            reads[OperationCode.CLEAR] = ReadClearOperation;

            CollectionFactory = collectionFactory;
        }

        #region Write Methods

        private void WriteReplaceOperation(BinaryWriter writer, IOperation operation)
        {
            ReplaceOperation opr = (ReplaceOperation)operation;

            KeyPersist.Write(writer, opr.FromKey);
            RecordPersist.Write(writer, opr.Record);
        }

        private void WriteInsertOrIgnoreOperation(BinaryWriter writer, IOperation operation)
        {
            InsertOrIgnoreOperation opr = (InsertOrIgnoreOperation)operation;

            KeyPersist.Write(writer, opr.FromKey);
            RecordPersist.Write(writer, opr.Record);
        }

        private void WriteDeleteOperation(BinaryWriter writer, IOperation operation)
        {
            DeleteOperation opr = (DeleteOperation)operation;

            KeyPersist.Write(writer, operation.FromKey);
        }

        private void WriteDeleteRangeOperation(BinaryWriter writer, IOperation operation)
        {
            DeleteRangeOperation opr = (DeleteRangeOperation)operation;

            KeyPersist.Write(writer, operation.FromKey);
            KeyPersist.Write(writer, operation.ToKey);
        }

        private void WriteClearOperation(BinaryWriter writer, IOperation operation)
        {
            //do nothing
        }

        #endregion

        #region Read Methods

        private IOperation ReadReplaceOperation(BinaryReader reader)
        {
            IData key = KeyPersist.Read(reader);
            IData record = RecordPersist.Read(reader);

            return new ReplaceOperation(key, record);
        }

        private IOperation ReadInsertOrIgnoreOperation(BinaryReader reader)
        {
            IData key = KeyPersist.Read(reader);
            IData record = RecordPersist.Read(reader);

            return new InsertOrIgnoreOperation(key, record);
        }

        private IOperation ReadDeleteOperation(BinaryReader reader)
        {
            IData key = KeyPersist.Read(reader);

            return new DeleteOperation(key);
        }

        private IOperation ReadDeleteRangeOperation(BinaryReader reader)
        {
            IData from = KeyPersist.Read(reader);
            IData to = KeyPersist.Read(reader);

            return new DeleteRangeOperation(from, to);
        }

        private IOperation ReadClearOperation(BinaryReader reader)
        {
            return new ClearOperation();
        }

        #endregion

        public void Write(BinaryWriter writer, IOperationCollection item)
        {
            writer.Write(VERSION);
            
            writer.Write(item.Count);
            writer.Write(item.AreAllMonotoneAndPoint);

            int commonAction = item.CommonAction;
            writer.Write(commonAction);

            if (commonAction > 0)
            {
                switch (commonAction)
                {
                    case OperationCode.REPLACE:
                        {
                            for (int i = 0; i < item.Count; i++)
                                WriteReplaceOperation(writer, item[i]);
                        }
                        break;

                    case OperationCode.INSERT_OR_IGNORE:
                        {
                            for (int i = 0; i < item.Count; i++)
                                WriteInsertOrIgnoreOperation(writer, item[i]);
                        }
                        break;

                    case OperationCode.DELETE:
                        {
                            for (int i = 0; i < item.Count; i++)
                                WriteDeleteOperation(writer, item[i]);
                        }
                        break;

                    case OperationCode.DELETE_RANGE:
                        {
                            for (int i = 0; i < item.Count; i++)
                                WriteDeleteRangeOperation(writer, item[i]);
                        }
                        break;

                    case OperationCode.CLEAR:
                        {
                            for (int i = 0; i < item.Count; i++)
                                WriteClearOperation(writer, item[i]);
                        }
                        break;

                    default:
                        throw new NotSupportedException(commonAction.ToString());
                }
            }
            else
            {
                for (int i = 0; i < item.Count; i++)
                {
                    IOperation operation = item[i];
                    writer.Write(operation.Code);
                    writes[operation.Code](writer, operation);
                }
            }
        }

        public IOperationCollection Read(BinaryReader reader)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid OperationCollectionPersist version.");
            
            int count = reader.ReadInt32();
            bool areAllMonotoneAndPoint = reader.ReadBoolean();
            int commonAction = reader.ReadInt32();

            IOperation[] array = new IOperation[count];

            if (commonAction > 0)
            {
                switch (commonAction)
                {
                    case OperationCode.REPLACE:
                        {
                            for (int i = 0; i < count; i++)
                                array[i] = ReadReplaceOperation(reader);
                        }
                        break;

                    case OperationCode.INSERT_OR_IGNORE:
                        {
                            for (int i = 0; i < count; i++)
                                array[i] = ReadInsertOrIgnoreOperation(reader);
                        }
                        break;

                    case OperationCode.DELETE:
                        {
                            for (int i = 0; i < count; i++)
                                array[i] = ReadDeleteOperation(reader);
                        }
                        break;

                    case OperationCode.DELETE_RANGE:
                        {
                            for (int i = 0; i < count; i++)
                                array[i] = ReadDeleteRangeOperation(reader);
                        }
                        break;

                    case OperationCode.CLEAR:
                        {
                            for (int i = 0; i < count; i++)
                                array[i] = ReadClearOperation(reader);
                        }
                        break;

                    default:
                        throw new NotSupportedException(commonAction.ToString());
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    int code = reader.ReadInt32();
                    array[i] = reads[code](reader);
                }
            }

            IOperationCollection operations = CollectionFactory.Create(array, commonAction, areAllMonotoneAndPoint);

            return operations;
        }
    }
}