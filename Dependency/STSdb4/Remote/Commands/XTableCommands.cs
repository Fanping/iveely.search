using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.STSdb4.Data;
using Iveely.STSdb4.WaterfallTree;

namespace Iveely.STSdb4.Remote.Commands
{
    #region ITable Operations

    public class ReplaceCommand : ICommand
    {
        public IData Key;
        public IData Record;

        public ReplaceCommand(IData key, IData record)
        {
            Key = key;
            Record = record;
        }

        public int Code
        {
            get { return CommandCode.REPLACE; }
        }

        public bool IsSynchronous
        {
            get { return false; }
        }
    }

    public class DeleteCommand : ICommand
    {
        public IData Key;

        public DeleteCommand(IData key)
        {
            Key = key;
        }

        public int Code
        {
            get { return CommandCode.DELETE; }
        }

        public bool IsSynchronous
        {
            get { return false; }
        }
    }

    public class DeleteRangeCommand : ICommand
    {
        public IData FromKey;
        public IData ToKey;

        public DeleteRangeCommand(IData fromKey, IData toKey)
        {
            FromKey = fromKey;
            ToKey = toKey;
        }

        public int Code
        {
            get { return CommandCode.DELETE_RANGE; }
        }

        public bool IsSynchronous
        {
            get { return false; }
        }
    }

    public class InsertOrIgnoreCommand : ICommand
    {
        public IData Key;
        public IData Record;

        public InsertOrIgnoreCommand(IData key, IData record)
        {
            Key = key;
            Record = record;
        }

        public int Code
        {
            get { return CommandCode.INSERT_OR_IGNORE; }
        }

        public bool IsSynchronous
        {
            get { return false; }
        }
    }

    public class ClearCommand : ICommand
    {
        public ClearCommand()
        {
        }

        public int Code
        {
            get { return CommandCode.CLEAR; }
        }

        public bool IsSynchronous
        {
            get { return false; }
        }
    }

    public class FirstRowCommand : ICommand
    {
        public KeyValuePair<IData, IData>? Row;

        public FirstRowCommand(KeyValuePair<IData, IData>? row)
        {
            Row = row;
        }

        public FirstRowCommand()
            : this(null)
        {
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        public int Code
        {
            get { return CommandCode.FIRST_ROW; }
        }
    }

    public class LastRowCommand : ICommand
    {
        public KeyValuePair<IData, IData>? Row;

        public LastRowCommand(KeyValuePair<IData, IData>? row)
        {
            Row = row;
        }

        public LastRowCommand()
            : this(null)
        {
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        public int Code
        {
            get { return CommandCode.LAST_ROW; }
        }
    }

    public class CountCommand : ICommand
    {
        public long Count;

        public CountCommand(long count)
        {
            Count = count;
        }

        public CountCommand()
            : this(0)
        {
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        public int Code
        {
            get { return CommandCode.COUNT; }
        }
    }

    public abstract class OutValueCommand : ICommand
    {
        private int code;

        public IData Key;
        public IData Record;

        public OutValueCommand(int code, IData key, IData record)
        {
            this.code = code;

            Key = key;
            Record = record;
        }

        public int Code
        {
            get { return code; }
        }

        public bool IsSynchronous
        {
            get { return true; }
        }
    }

    public class TryGetCommand : OutValueCommand
    {
        public TryGetCommand(IData key, IData record)
            : base(CommandCode.TRY_GET, key, record)
        {
        }

        public TryGetCommand(IData key)
            : this(key, null)
        {
        }
    }

    public abstract class OutKeyValueCommand : ICommand
    {
        private int code;

        public IData Key;
        public KeyValuePair<IData, IData>? KeyValue;

        public OutKeyValueCommand(int code, IData key, KeyValuePair<IData, IData>? keyValue)
        {
            this.code = code;

            Key = key;
            KeyValue = keyValue;
        }

        public int Code
        {
            get { return code; }
        }

        public bool IsSynchronous
        {
            get { return true; }
        }
    }

    public class FindNextCommand : OutKeyValueCommand
    {
        public FindNextCommand(IData key, KeyValuePair<IData, IData>? keyValue)
            : base(CommandCode.FIND_NEXT, key, keyValue)
        {
        }

        public FindNextCommand(IData key)
            : this(key, null)
        {
        }
    }

    public class FindAfterCommand : OutKeyValueCommand
    {
        public FindAfterCommand(IData key, KeyValuePair<IData, IData>? keyValue)
            : base(CommandCode.FIND_AFTER, key, keyValue)
        {
        }

        public FindAfterCommand(IData key)
            : this(key, null)
        {
        }
    }

    public class FindPrevCommand : OutKeyValueCommand
    {
        public FindPrevCommand(IData key, KeyValuePair<IData, IData>? keyValue)
            : base(CommandCode.FIND_PREV, key, keyValue)
        {
        }

        public FindPrevCommand(IData key)
            : this(key, null)
        {
        }
    }

    public class FindBeforeCommand : OutKeyValueCommand
    {
        public FindBeforeCommand(IData key, KeyValuePair<IData, IData>? keyValue)
            : base(CommandCode.FIND_BEFORE, key, keyValue)
        {
        }

        public FindBeforeCommand(IData key)
            : this(key, null)
        {
        }
    }

    #endregion

    #region IteratorOperations

    public abstract class IteratorCommand : ICommand
    {
        private int code;

        public IData FromKey;
        public IData ToKey;

        public int PageCount;
        public List<KeyValuePair<IData, IData>> List;

        public IteratorCommand(int code, int pageCount, IData from, IData to, List<KeyValuePair<IData, IData>> list)
        {
            this.code = code;

            FromKey = from;
            ToKey = to;

            PageCount = pageCount;
            List = list;
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        public int Code
        {
            get { return code; }
        }
    }

    public class ForwardCommand : IteratorCommand
    {
        public ForwardCommand(int pageCount, IData from, IData to, List<KeyValuePair<IData, IData>> list)
            : base(CommandCode.FORWARD, pageCount, from, to, list)
        {
        }
    }

    public class BackwardCommand : IteratorCommand
    {
        public BackwardCommand(int pageCount, IData from, IData to, List<KeyValuePair<IData, IData>> list)
            : base(CommandCode.BACKWARD, pageCount, from, to, list)
        {
        }
    }

    #endregion

    #region Descriptor

    public class XTableDescriptorGetCommand : ICommand
    {
        public IDescriptor Descriptor;

        public XTableDescriptorGetCommand(IDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public int Code
        {
            get { return CommandCode.XTABLE_DESCRIPTOR_GET; }
        }

        public bool IsSynchronous
        {
            get { return true; }
        }
    }

    public class XTableDescriptorSetCommand : ICommand
    {
        public IDescriptor Descriptor;

        public XTableDescriptorSetCommand(IDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public int Code
        {
            get { return CommandCode.XTABLE_DESCRIPTOR_SET; }
        }

        public bool IsSynchronous
        {
            get { return true; }
        }
    }

    #endregion
}
