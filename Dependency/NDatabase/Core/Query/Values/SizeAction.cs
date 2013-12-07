using System.Collections;
using NDatabase.Api;
using NDatabase.Core.Query.Execution;
using NDatabase.Exceptions;
using NDatabase.Meta;

namespace NDatabase.Core.Query.Values
{
    /// <summary>
    ///   An action to retrieve a size of a list.
    /// </summary>
    /// <remarks>
    ///   An action to retrieve a size of a list. It is used by the Object Values API. When calling odb.getValues(new ValuesCriteriaQuery(Handler.class, Where .equal("id", id)).size("parameters"); The sublist action will return Returns a view of the portion of this list between the specified fromIndex, inclusive, and toIndex, exclusive. if parameters list contains [param1,param2,param3,param4], sublist("parameters",1,2) will return a sublist containing [param2,param3]
    /// </remarks>
    internal sealed class SizeAction : AbstractQueryFieldAction
    {
        private readonly IInternalQuery _query;
        private long _size;

        public SizeAction(IInternalQuery query, string attributeName, string alias) : base(attributeName, alias, true)
        {
            _query = query;
        }

        public override void Execute(OID oid, AttributeValuesMap values)
        {
            var candidate = values[AttributeName];

            if (candidate is OID)
            {
                var candidateOid = (OID)candidate;
                candidate = _query.GetQueryEngine().GetObjectFromOid(candidateOid);
            }

            if (!(candidate is IList || candidate is string))
                throw new OdbRuntimeException(
                    NDatabaseError.UnsupportedOperation.AddParameter("Size() with string or collection as the argument"));

            var candidateAsString = candidate as string;
            if (candidateAsString != null)
            {
                _size = candidateAsString.Length;
            }
            else
            {
                var list = (IList)candidate;
                _size = list.Count;    
            }
        }

        public override object GetValue()
        {
            return _size;
        }

        public override void End()
        {
        }

        public override void Start()
        {
        }

        public long GetSize()
        {
            return _size;
        }

        public override IQueryFieldAction Copy()
        {
            return new SizeAction(_query, AttributeName, Alias);
        }
    }
}
