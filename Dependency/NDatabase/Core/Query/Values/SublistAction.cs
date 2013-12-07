using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NDatabase.Api;
using NDatabase.Core.Query.Execution;
using NDatabase.Core.Query.List;
using NDatabase.Meta;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Values
{
    /// <summary>
    ///   An action to retrieve a sublist of list.
    /// </summary>
    /// <remarks>
    ///   An action to retrieve a sublist of list. It is used by the Object Values API. When calling odb.getValues(new ValuesCriteriaQuery(Handler.class, Where .equal("id", id)).sublist("parameters",fromIndex, size); The sublist action will return Returns a view of the portion of this list between the specified fromIndex, inclusive, and toIndex, exclusive. if parameters list contains [param1,param2,param3,param4], sublist("parameters",1,2) will return a sublist containing [param2,param3]
    /// </remarks>
    internal sealed class SublistAction : AbstractQueryFieldAction
    {
        private readonly IInternalQuery _query;
        private readonly int _fromIndex;
        private readonly int _size;
        private readonly bool _throwExceptionIfOutOfBound;
        private IOdbList<object> _sublist;

        public SublistAction(IInternalQuery query, string attributeName, string alias, int fromIndex, int size, bool throwExceptionIfOutOfBound) 
            : base(attributeName, alias, true)
        {
            _query = query;
            _fromIndex = fromIndex;
            _size = size;
            _throwExceptionIfOutOfBound = throwExceptionIfOutOfBound;
        }

        public SublistAction(IInternalQuery query, string attributeName, string alias, int fromIndex, int toIndex)
            : base(attributeName, alias, true)
        {
            _query = query;
            _fromIndex = fromIndex;
            _size = toIndex - fromIndex;
            _throwExceptionIfOutOfBound = true;
        }

        public override void Execute(OID oid, AttributeValuesMap values)
        {
            var candidate = values[AttributeName];

            if (candidate is OID)
            {
                var candidateOid = (OID)candidate;
                candidate = _query.GetQueryEngine().GetObjectFromOid(candidateOid);
            }

            var l = ((IEnumerable)candidate).Cast<object>().ToList();
            var localFromIndex = _fromIndex;
            var localEndIndex = _fromIndex + _size;

            // If not throw exception, we must implement 
            // Index Out Of Bound protection
            if (!_throwExceptionIfOutOfBound)
            {
                // Check from index
                if (localFromIndex > l.Count - 1)
                    localFromIndex = 0;

                // Check end index
                if (localEndIndex > l.Count)
                    localEndIndex = l.Count;
            }

            _sublist = new LazySimpleListOfAoi<object>(GetInstanceBuilder(), ReturnInstance());
            var count = localEndIndex - localFromIndex;
            var sublist = l.GetRange(localFromIndex, count);

            _sublist.AddAll(sublist);
        }

        public override object GetValue()
        {
            return _sublist;
        }

        public override void End()
        {
        }

        public override void Start()
        {
        }

        public IList<object> GetSubList()
        {
            return _sublist;
        }

        public override IQueryFieldAction Copy()
        {
            return new SublistAction(_query, AttributeName, Alias, _fromIndex, _size, _throwExceptionIfOutOfBound);
        }
    }
}
