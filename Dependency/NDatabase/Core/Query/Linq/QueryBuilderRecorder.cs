using System;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal interface IQueryBuilderRecord
    {
        void Playback(IQuery query);
        void Playback(QueryBuilderContext context);
    }

    internal sealed class NullQueryBuilderRecord : IQueryBuilderRecord
    {
        public static readonly NullQueryBuilderRecord Instance = new NullQueryBuilderRecord();

        private NullQueryBuilderRecord()
        {
        }

        #region IQueryBuilderRecord Members

        public void Playback(IQuery query)
        {
        }

        public void Playback(QueryBuilderContext context)
        {
        }

        #endregion
    }

    internal abstract class QueryBuilderRecordImpl : IQueryBuilderRecord
    {
        #region IQueryBuilderRecord Members

        public void Playback(IQuery query)
        {
            Playback(new QueryBuilderContext(query));
        }

        public abstract void Playback(QueryBuilderContext context);

        #endregion
    }

    internal sealed class CompositeQueryBuilderRecord : QueryBuilderRecordImpl
    {
        private readonly IQueryBuilderRecord _first;
        private readonly IQueryBuilderRecord _second;

        public CompositeQueryBuilderRecord(IQueryBuilderRecord first, IQueryBuilderRecord second)
        {
            _first = first;
            _second = second;
        }

        public override void Playback(QueryBuilderContext context)
        {
            context.SaveQuery();
            _first.Playback(context);
            context.RestoreQuery();

            _second.Playback(context);
        }
    }

    internal sealed class ChainedQueryBuilderRecord : QueryBuilderRecordImpl
    {
        private readonly Action<QueryBuilderContext> _action;
        private readonly IQueryBuilderRecord _next;

        public ChainedQueryBuilderRecord(IQueryBuilderRecord next, Action<QueryBuilderContext> action)
        {
            _next = next;
            _action = action;
        }

        public override void Playback(QueryBuilderContext context)
        {
            _next.Playback(context);
            _action(context);
        }
    }

    internal sealed class QueryBuilderRecorder
    {
        private IQueryBuilderRecord _last = NullQueryBuilderRecord.Instance;

        public QueryBuilderRecorder()
        {
        }

        public QueryBuilderRecorder(IQueryBuilderRecord record)
        {
            _last = record;
        }

        public IQueryBuilderRecord Record
        {
            get { return _last; }
        }

        public void Add(Action<QueryBuilderContext> action)
        {
            _last = new ChainedQueryBuilderRecord(_last, action);
        }
    }
}
