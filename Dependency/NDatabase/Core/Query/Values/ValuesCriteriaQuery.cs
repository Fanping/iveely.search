using System;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Core.Query.Criteria;
using NDatabase.Core.Query.Execution;
using NDatabase.Exceptions;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Values
{
    /// <summary>
    ///   A values Criteria query is a query to retrieve object values instead of objects.
    /// </summary>
    /// <remarks>
    ///   A values Criteria query is a query to retrieve object values instead of objects. 
    ///   Values Criteria Query allows one to retrieve one field value of an object: 
    ///    - A field values 
    ///    - The sum of a specific numeric field 
    ///    - The Max value of a specific numeric field 
    ///    - The Min value of a specific numeric field 
    ///    - The Average value of a specific numeric value
    /// </remarks>
    internal sealed class ValuesCriteriaQuery : SodaQuery, IInternalValuesQuery
    {
        private string[] _groupByFieldList;

        private bool _hasGroupBy;

        private IOdbList<IQueryFieldAction> _objectActions;

        /// <summary>
        ///   To specify if the result must build instance of object meta representation
        /// </summary>
        private bool _returnInstance;

        public ValuesCriteriaQuery(Type underlyingType, OID oid) 
            : base(underlyingType)
        {
            SetOidOfObjectToQuery(oid);
            Init();
        }

        public ValuesCriteriaQuery(Type underlyingType) 
            : base(underlyingType)
        {
            Init();
        }

        #region IValuesQuery Members

        public IValuesQuery Count(string alias)
        {
            _objectActions.Add(new CountAction(alias));
            return this;
        }

        public IValuesQuery Sum(string attributeName)
        {
            return Sum(attributeName, attributeName);
        }

        public IValuesQuery Sum(string attributeName, string alias)
        {
            _objectActions.Add(new SumAction(attributeName, alias));
            return this;
        }

        public IValuesQuery Sublist(string attributeName, int fromIndex, int size, bool throwException)
        {
            return Sublist(attributeName, attributeName, fromIndex, size, throwException);
        }

        public IValuesQuery Sublist(string attributeName, string alias, int fromIndex, int size, bool throwException)
        {
            _objectActions.Add(new SublistAction(this, attributeName, alias, fromIndex, size, throwException));
            return this;
        }

        public IValuesQuery Sublist(string attributeName, int fromIndex, int toIndex)
        {
            return Sublist(attributeName, attributeName, fromIndex, toIndex);
        }

        public IValuesQuery Sublist(string attributeName, string alias, int fromIndex, int toIndex)
        {
            _objectActions.Add(new SublistAction(this, attributeName, alias, fromIndex, toIndex));
            return this;
        }

        public IValuesQuery Size(string attributeName)
        {
            return Size(attributeName, attributeName);
        }

        public IValuesQuery Size(string attributeName, string alias)
        {
            _objectActions.Add(new SizeAction(this, attributeName, alias));
            return this;
        }

        public IValuesQuery Avg(string attributeName)
        {
            return Avg(attributeName, attributeName);
        }

        public IValuesQuery Avg(string attributeName, string alias)
        {
            _objectActions.Add(new AverageValueAction(attributeName, alias));
            return this;
        }

        public IValuesQuery Max(string attributeName)
        {
            return Max(attributeName, attributeName);
        }

        public IValuesQuery Max(string attributeName, string alias)
        {
            _objectActions.Add(new MaxValueAction(attributeName, alias));
            return this;
        }

        public IValuesQuery Field(string attributeName)
        {
            return Field(attributeName, attributeName);
        }

        public IValuesQuery Field(string attributeName, string alias)
        {
            _objectActions.Add(new FieldValueAction(attributeName, alias));
            return this;
        }

        public IEnumerable<IQueryFieldAction> GetObjectActions()
        {
            return _objectActions;
        }

        /// <summary>
        ///   Returns the list of involved fields for this query.
        /// </summary>
        /// <remarks>
        ///   Returns the list of involved fields for this query. List of String <pre>If query must return sum("value") and field("name"), involvedField will contain "value","name"</pre>
        /// </remarks>
        public override IOdbList<string> GetAllInvolvedFields()
        {
            IOdbList<string> list = new OdbList<string>();

            // To check field duplicity
            IDictionary<string, string> map = new OdbHashMap<string, string>();
            list.AddAll(base.GetAllInvolvedFields());

            if (list.IsNotEmpty())
            {
                foreach (var value in list)
                    map.Add(value, value);
            }

            var iterator = _objectActions.GetEnumerator();
            string name;

            while (iterator.MoveNext())
            {
                var queryFieldAction = iterator.Current;
                if (queryFieldAction is CountAction) 
                    continue;

                name = queryFieldAction.GetAttributeName();
                if (map.ContainsKey(name)) 
                    continue;

                list.Add(name);
                map.Add(name, name);
            }

            if (_hasGroupBy)
            {
                foreach (var groupByField in _groupByFieldList)
                {
                    name = groupByField;

                    if (map.ContainsKey(name)) 
                        continue;

                    list.Add(name);
                    map.Add(name, name);
                }
            }

            if (HasOrderBy())
            {
                foreach (var field in OrderByFields)
                {
                    name = field;
                    if (map.ContainsKey(name)) 
                        continue;

                    list.Add(name);
                    map.Add(name, name);
                }
            }
            map.Clear();

            return list;
        }

        public bool IsMultiRow()
        {
            var isMultiRow = true;
            IQueryFieldAction queryFieldAction;

            // Group by protection
            // When a group by with one field exist in the query, FieldObjectAction with this field must be set to SingleRow
            var groupBy = _hasGroupBy && _groupByFieldList.Length == 1;
            var iterator = _objectActions.GetEnumerator();

            if (groupBy)
            {
                var oneGroupByField = _groupByFieldList[0];
                while (iterator.MoveNext())
                {
                    queryFieldAction = iterator.Current;
                    if (queryFieldAction is FieldValueAction &&
                        queryFieldAction.GetAttributeName().Equals(oneGroupByField))
                        queryFieldAction.SetMultiRow(false);
                }
            }

            iterator = _objectActions.GetEnumerator();

            if (iterator.MoveNext())
            {
                queryFieldAction = iterator.Current;
                isMultiRow = queryFieldAction.IsMultiRow();
            }

            while (iterator.MoveNext())
            {
                queryFieldAction = iterator.Current;
                if (isMultiRow != queryFieldAction.IsMultiRow())
                    throw new OdbRuntimeException(NDatabaseError.ValuesQueryNotConsistent.AddParameter(this));
            }

            return isMultiRow;
        }

        public IValuesQuery GroupBy(string fieldList)
        {
            _groupByFieldList = fieldList.Split(',');

            _hasGroupBy = true;
            return this;
        }

        public bool HasGroupBy()
        {
            return _hasGroupBy;
        }

        public string[] GetGroupByFieldList()
        {
            return _groupByFieldList;
        }

        public bool ReturnInstance()
        {
            return _returnInstance;
        }

        public void SetReturnInstance(bool returnInstance)
        {
            _returnInstance = returnInstance;
        }

        public int ObjectActionsCount { get { return _objectActions.Count; } }

        public IValuesQuery Min(string fieldName)
        {
            return Min(fieldName, fieldName);
        }

        public IValuesQuery Min(string fieldName, string alias)
        {
            _objectActions.Add(new MinValueAction(fieldName, alias));
            return this;
        }

        public IValues Execute()
        {
            return ((IInternalQuery)this).GetQueryEngine().GetValues(this, -1, -1);
        }

        public override IObjectSet<TItem> Execute<TItem>()
        {
            throw new OdbRuntimeException(NDatabaseError.UnsupportedOperation.AddParameter("IValuesQuery.Execute()"));
        }

        public override IObjectSet<TItem> Execute<TItem>(bool inMemory)
        {
            throw new OdbRuntimeException(NDatabaseError.UnsupportedOperation.AddParameter("IValuesQuery.Execute()"));
        }

        public override IObjectSet<TItem> Execute<TItem>(bool inMemory, int startIndex, int endIndex)
        {
            throw new OdbRuntimeException(NDatabaseError.UnsupportedOperation.AddParameter("IValuesQuery.Execute()"));
        }

        #endregion

        private void Init()
        {
            _objectActions = new OdbList<IQueryFieldAction>();
            _returnInstance = true;
        }
    }
}
