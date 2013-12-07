using System;
using System.Collections.Generic;
using System.Reflection;
using NDatabase.Api;
using NDatabase.Container;
using NDatabase.Exceptions;
using NDatabase.Meta;
using System.Linq;
using NDatabase.Services;

namespace NDatabase.Core.BTree
{
    internal sealed class IndexManager : IIndexManager
    {
        private readonly ClassInfo _classInfo;

        private readonly IStorageEngine _storageEngine;
        private readonly IReflectionService _reflectionService;

        internal IndexManager(IStorageEngine storageEngine, ClassInfo classInfo)
        {
            _storageEngine = storageEngine;
            _classInfo = classInfo;

            _reflectionService = DependencyContainer.Resolve<IReflectionService>();
        }

        #region IClassRepresentation Members

        public void AddUniqueIndexOn(string indexName, params string[] indexFields)
        {
            if (indexFields.Length == 0)
                throw new OdbRuntimeException(
                    NDatabaseError.InternalError.AddParameter("Index has to have at least one field"));

            indexFields = ValidateFields(indexName, indexFields);

            _storageEngine.AddIndexOn(_classInfo.FullClassName, indexName, indexFields, false);
        }

        public void AddIndexOn(string indexName, params string[] indexFields)
        {
            if (indexFields.Length == 0)
                throw new OdbRuntimeException(
                    NDatabaseError.InternalError.AddParameter("Index has to have at least one field"));

            indexFields = ValidateFields(indexName, indexFields);

            _storageEngine.AddIndexOn(_classInfo.FullClassName, indexName, indexFields, true);
        }

        public bool ExistIndex(string indexName)
        {
            return _classInfo.HasIndex(indexName);
        }

        /// <summary>
        ///   Used to rebuild an index
        /// </summary>
        public void RebuildIndex(string indexName)
        {
            _storageEngine.RebuildIndex(_classInfo.FullClassName, indexName);
        }

        public void DeleteIndex(string indexName)
        {
            _storageEngine.DeleteIndex(_classInfo.FullClassName, indexName);
        }

        #endregion

        private string[] ValidateFields(string indexName, IEnumerable<string> indexFields)
        {
            var withoutDuplicates = indexFields.Distinct().ToArray();

            var members = _reflectionService.GetFieldsAndProperties(_classInfo.UnderlyingType);

            foreach (var indexField in withoutDuplicates)
            {
                var memberInfo = members.FirstOrDefault(x => x.Name.Equals(indexField));

                var memberType = memberInfo is PropertyInfo
                                     ? ((PropertyInfo) memberInfo).PropertyType
                                     : ((FieldInfo) memberInfo).FieldType;

                if (memberInfo != null && (typeof(IComparable)).IsAssignableFrom(memberType)) 
                    continue;

                var fieldType = (memberInfo == null || memberInfo.DeclaringType == null)
                                    ? "Field doesn't exist"
                                    : memberType.FullName;

                throw new OdbRuntimeException(NDatabaseError.IndexKeysMustImplementComparable.AddParameter(indexName)
                                                            .AddParameter(indexField)
                                                            .AddParameter(fieldType));
            }

            return withoutDuplicates;
        }
    }
}
