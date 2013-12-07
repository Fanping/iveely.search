using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Indexing;
using NDatabase.Meta;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria
{
    internal sealed class CriteriaQueryExecutor : GenericQueryExecutor
    {
        private IOdbList<string> _involvedFields;

        public CriteriaQueryExecutor(IQuery query, IStorageEngine engine) : base(query, engine)
        {
        }

        protected override IQueryExecutionPlan GetExecutionPlan()
        {
            return new CriteriaQueryExecutionPlan(ClassInfo, Query);
        }

        protected override void PrepareQuery()
        {
            _involvedFields = Query.GetAllInvolvedFields();
        }

        protected override bool MatchObjectWithOid(OID oid, bool returnObject, bool inMemory)
        {
            CurrentOid = oid;
            var tmpCache = Session.GetTmpCache();
            try
            {
                ObjectInfoHeader objectInfoHeader;

                if (!Query.HasCriteria())
                {
                    // true, false = use cache, false = do not return object
                    // TODO Warning setting true to useCache will put all objects in the cache
                    // This is not a good idea for big queries!, But use cache=true
                    // resolves when object have not been committed yet!
                    // for big queries, user should use a LazyCache!
                    if (inMemory)
                    {
                        CurrentNnoi = ObjectReader.ReadNonNativeObjectInfoFromOid(ClassInfo, CurrentOid, true,
                                                                                  returnObject);
                        if (CurrentNnoi.IsDeletedObject())
                            return false;
                        CurrentOid = CurrentNnoi.GetOid();
                        NextOID = CurrentNnoi.GetNextObjectOID();
                    }
                    else
                    {
                        objectInfoHeader = ObjectReader.ReadObjectInfoHeaderFromOid(CurrentOid, false);
                        NextOID = objectInfoHeader.GetNextObjectOID();
                    }
                    return true;
                }

                // Gets a map with the values with the fields involved in the query
                var attributeValues = ObjectReader.ReadObjectInfoValuesFromOID(ClassInfo, CurrentOid, true,
                                                                               _involvedFields, _involvedFields, 0);

                // Then apply the query on the field values
                var objectMatches = Query.Match(attributeValues);

                if (objectMatches)
                {
                    // Then load the entire object
                    // true, false = use cache
                    CurrentNnoi = ObjectReader.ReadNonNativeObjectInfoFromOid(ClassInfo, CurrentOid, true, returnObject);
                    CurrentOid = CurrentNnoi.GetOid();
                }

                objectInfoHeader = attributeValues.GetObjectInfoHeader();
                // Stores the next position
                NextOID = objectInfoHeader.GetNextObjectOID();
                return objectMatches;
            }
            finally
            {
                tmpCache.ClearObjectInfos();
            }
        }

        protected override IOdbComparable ComputeIndexKey(ClassInfoIndex index)
        {
            var constraint = Query.GetCriteria();
            var values = ((IInternalConstraint)constraint).GetValues();

            // if values.hasOid() is true, this means that we are working of the full object,
            // the index key is then the oid and not the object itself
            return values.HasOid()
                       ? new SimpleCompareKey(values.GetOid())
                       : base.ComputeIndexKey(index);
        }

        protected override object GetCurrentObjectMetaRepresentation()
        {
            return CurrentNnoi;
        }
    }
}
