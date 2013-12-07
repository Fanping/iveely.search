using System;
using System.Collections;
using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Exceptions;

namespace NDatabase.Core.Query.Criteria.Evaluations
{
    internal sealed class CollectionSizeEvaluation : AEvaluation
    {
        internal const int SizeEq = 1;
        internal const int SizeNe = 2;
        internal const int SizeGt = 3;
        internal const int SizeGe = 4;
        internal const int SizeLt = 5;
        internal const int SizeLe = 6;

        private readonly IInternalQuery _query;
        private readonly int _sizeType;

        public CollectionSizeEvaluation(object theObject, string attributeName, IQuery query, int sizeType) 
            : base(theObject, attributeName)
        {
            _query = (IInternalQuery) query;
            _sizeType = sizeType;
        }

        public override bool Evaluate(object candidate)
        {
            candidate = AsAttributeValuesMapValue(candidate);

            if (!(TheObject is int))
                throw new ArgumentException("Constrain argument need to be int (size).");

            var size = (int)TheObject;

            if (candidate is OID)
            {
                var oid = (OID)candidate;
                candidate = _query.GetQueryEngine().GetObjectFromOid(oid);
            }

            if (candidate == null)
            {
                // Null list are considered 0-sized list
                if (_sizeType == SizeEq && size == 0)
                    return true;
                if ((_sizeType == SizeLe && size >= 0) || (_sizeType == SizeLt && size > 0))
                    return true;
                return _sizeType == SizeNe && size != 0;
            }

            var collection = candidate as ICollection;
            if (collection != null)
                return MatchSize(collection.Count, size, _sizeType);

            var clazz = candidate.GetType();
            if (clazz.IsArray)
            {
                var arrayLength = ((Array)candidate).GetLength(0);
                return MatchSize(arrayLength, size, _sizeType);
            }

            throw new OdbRuntimeException(NDatabaseError.QueryBadCriteria.AddParameter(candidate.GetType().FullName));
        }

        private static bool MatchSize(int collectionSize, int requestedSize, int sizeType)
        {
            switch (sizeType)
            {
                case SizeEq:
                    return collectionSize == requestedSize;
                case SizeNe:
                    return collectionSize != requestedSize;
                case SizeGt:
                    return collectionSize > requestedSize;
                case SizeGe:
                    return collectionSize >= requestedSize;
                case SizeLt:
                    return collectionSize < requestedSize;
                case SizeLe:
                    return collectionSize <= requestedSize;
            }

            throw new OdbRuntimeException(NDatabaseError.QueryCollectionSizeCriteriaNotSupported.AddParameter(sizeType));
        }
    }
}