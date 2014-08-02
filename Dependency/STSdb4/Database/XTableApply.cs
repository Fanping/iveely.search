using System;
using Iveely.STSdb4.General.Collections;
using Iveely.STSdb4.Data;
using Iveely.STSdb4.Database.Operations;
using Iveely.STSdb4.WaterfallTree;
using System.Collections.Generic;

namespace Iveely.STSdb4.Database
{
    public sealed class XTableApply : IApply
    {
        public event ReadOperationDelegate ReadCallback;

        public delegate void ReadOperationDelegate(long handle, bool exist, Locator path, IData key, IData record);

        public XTableApply(Locator locator)
        {
            Locator = locator;
        }

        public bool Internal(IOperationCollection operations)
        {
            return false;
        }

        private bool SequentialApply(IOperationCollection operations, IOrderedSet<IData, IData> data)
        {
            switch (operations.CommonAction)
            {
                case OperationCode.REPLACE:
                case OperationCode.INSERT_OR_IGNORE:
                    {
                        foreach (var operation in operations)
                        {
                            ValueOperation opr = (ValueOperation)operation;
                            data.UnsafeAdd(opr.FromKey, opr.Record);
                        }

                        return true;
                    }
                case OperationCode.DELETE:
                    {
                        return false;
                    }

                case OperationCode.DELETE_RANGE:
                case OperationCode.CLEAR:
                    {
                        throw new Exception("Logical error.");
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private bool CommonApply(IOperationCollection operations, IOrderedSet<IData, IData> data)
        {
            int commonAction = operations.CommonAction;

            int changes = 0;

            switch (commonAction)
            {
                case OperationCode.REPLACE:
                    {
                        foreach (var opr in operations)
                        {
                            data[opr.FromKey] = ((ReplaceOperation)opr).Record;
                            changes++;
                        }
                    }
                    break;

                case OperationCode.INSERT_OR_IGNORE:
                    {
                        foreach (var opr in operations)
                        {
                            if (data.ContainsKey(opr.FromKey))
                                continue;

                            data[opr.FromKey] = ((InsertOrIgnoreOperation)opr).Record;
                            changes++;
                        }
                    }
                    break;

                case OperationCode.DELETE:
                    {
                        foreach (var opr in operations)
                        {
                            if (data.Remove(opr.FromKey))
                                changes++;
                        }
                    }
                    break;

                case OperationCode.DELETE_RANGE:
                    {
                        foreach (var opr in operations)
                        {
                            if (data.Remove(opr.FromKey, true, opr.ToKey, true))
                                changes++;
                        }
                    }
                    break;

                case OperationCode.CLEAR:
                    {
                        foreach (var opr in operations)
                        {
                            data.Clear();
                            changes++;
                            break;
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            return changes > 0;
        }

        public bool Leaf(IOperationCollection operations, IOrderedSet<IData, IData> data)
        {
            //sequential optimization
            if (operations.AreAllMonotoneAndPoint && data.IsInternallyOrdered && (data.Count == 0 || operations.Locator.KeyComparer.Compare(data.Last.Key, operations[0].FromKey) < 0))
                return SequentialApply(operations, data);

            //common action optimization
            if (operations.CommonAction != OperationCode.UNDEFINED)
                return CommonApply(operations, data);

            //standart apply
            bool isModified = false;

            foreach (var opr in operations)
            {
                switch (opr.Code)
                {
                    case OperationCode.REPLACE:
                        {
                            data[opr.FromKey] = ((ReplaceOperation)opr).Record;

                            isModified = true;
                        }
                        break;
                    case OperationCode.INSERT_OR_IGNORE:
                        {
                            if (data.ContainsKey(opr.FromKey))
                                continue;

                            data[opr.FromKey] = ((InsertOrIgnoreOperation)opr).Record;

                            isModified = true;
                        }
                        break;
                    case OperationCode.DELETE:
                        {
                            if (data.Remove(opr.FromKey))
                                isModified = true;
                        }
                        break;
                    case OperationCode.DELETE_RANGE:
                        {
                            if (data.Remove(opr.FromKey, true, opr.ToKey, true))
                                isModified = true;
                        }
                        break;
                    case OperationCode.CLEAR:
                        {
                            data.Clear();
                            isModified = true;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return isModified;
        }

        public Locator Locator { get; private set; }
    }
}
