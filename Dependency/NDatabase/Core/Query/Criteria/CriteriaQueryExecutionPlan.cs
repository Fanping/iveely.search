using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Meta;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria
{
    /// <summary>
    ///   A simple Criteria execution plan Check if the query can use index and tries to find the best index to be used
    /// </summary>
    internal sealed class CriteriaQueryExecutionPlan : IQueryExecutionPlan
    {
        [NonPersistent] private readonly ClassInfo _classInfo;

        [NonPersistent] private readonly SodaQuery _query;

        [NonPersistent] private ClassInfoIndex _classInfoIndex;

        /// <summary>
        ///   To keep the execution detail
        /// </summary>
        private string _details;

        /// <summary>
        ///   to keep track of the end date time of the plan
        /// </summary>
        private long _end;

        /// <summary>
        ///   to keep track of the start date time of the plan
        /// </summary>
        private long _start;

        private bool _useIndex;

        public CriteriaQueryExecutionPlan(ClassInfo classInfo, SodaQuery query)
        {
            _classInfo = classInfo;
            _query = query;
            ((IInternalQuery) _query).SetExecutionPlan(this);
            Init();
        }

        #region IQueryExecutionPlan Members

        public ClassInfoIndex GetIndex()
        {
            return _classInfoIndex;
        }

        public bool UseIndex()
        {
            return _useIndex;
        }

        public string GetDetails()
        {
            if (_details != null)
                return _details;

            return _classInfoIndex == null
                       ? string.Format("No index used, Execution time={0}ms", GetDuration())
                       : string.Format("Following indexes have been used : {0}, Execution time={1}ms",
                                       _classInfoIndex.Name, GetDuration());
        }

        public void End()
        {
            _end = OdbTime.GetCurrentTimeInMs();
        }

        public void Start()
        {
            _start = OdbTime.GetCurrentTimeInMs();
        }

        #endregion

        private long GetDuration()
        {
            return (_end - _start);
        }

        private void Init()
        {
            _start = 0;
            _end = 0;

            // for instance, only manage index for one field query using 'equal'
            if (_classInfo.HasIndex() && _query.HasCriteria() &&
                ((IInternalConstraint) _query.GetCriteria()).CanUseIndex())
            {
                var fields = _query.GetAllInvolvedFields();
                if (fields.IsEmpty())
                    _useIndex = false;
                else
                {
                    var fieldIds = GetAllInvolvedFieldIds(fields);
                    _classInfoIndex = _classInfo.GetIndexForAttributeIds(fieldIds);
                    if (_classInfoIndex != null)
                        _useIndex = true;
                }
            }

            // Keep the detail
            _details = GetDetails();
        }

        /// <summary>
        ///   Transform a list of field names into a list of field ids
        /// </summary>
        /// <param name="fields"> </param>
        /// <returns> The array of field ids </returns>
        private int[] GetAllInvolvedFieldIds(IList<string> fields)
        {
            var nbFields = fields.Count;
            var fieldIds = new int[nbFields];
            for (var i = 0; i < nbFields; i++)
                fieldIds[i] = _classInfo.GetAttributeId(fields[i]);

            return fieldIds;
        }
    }
}