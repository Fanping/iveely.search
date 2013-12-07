using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDatabase.Api;
using NDatabase.Exceptions;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;
using NDatabase.TypeResolution;

namespace NDatabase.Meta
{
    /// <summary>
    ///   A meta representation of a class
    /// </summary>
    internal sealed class ClassInfo
    {
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();
        private readonly AttributesCache _attributesCache;

        /// <summary>
        ///   To keep session numbers, number of committed objects,first and last object position
        /// </summary>
        private readonly CommittedCIZoneInfo _committed;

        /// <summary>
        ///   The full class name with package
        /// </summary>
        private readonly string _fullClassName;

        private readonly OidInfo _oidInfo;

        /// <summary>
        ///   To keep session original numbers, original number of committed objects,first and last object position
        /// </summary>
        private readonly CommittedCIZoneInfo _original;

        private readonly Type _underlyingType;

        /// <summary>
        ///   To keep session uncommitted numbers, number of uncommitted objects,first and last object position
        /// </summary>
        private readonly CIZoneInfo _uncommitted;

        private IOdbList<ClassAttributeInfo> _attributes;

        private IOdbList<ClassInfoIndex> _indexes;

        private ClassInfo()
        {
            _attributes = null;
            _original = new CommittedCIZoneInfo();
            _committed = new CommittedCIZoneInfo();
            _uncommitted = new CIZoneInfo();
            _oidInfo = new OidInfo();
            Position = -1;
            MaxAttributeId = -1;
            _attributesCache = new AttributesCache();
        }

        public ClassInfo(Type underlyingType) : this()
        {
            _underlyingType = underlyingType;

            _fullClassName = OdbClassNameResolver.GetFullName(underlyingType);

            TypeCache.GetOrAdd(_fullClassName, UnderlyingType);
        }

        public ClassInfo(string fullClassName) : this()
        {
            _underlyingType = CheckIfTypeIsInstantiable(fullClassName);
            _fullClassName = fullClassName;
        }

        internal IOdbList<ClassAttributeInfo> Attributes
        {
            get { return _attributes; }
            set
            {
                _attributes = value;
                MaxAttributeId = value.Count;
                FillAttributesMap();
            }
        }

        internal CommittedCIZoneInfo CommitedZoneInfo
        {
            get { return _committed; }
        }

        /// <summary>
        ///   Where starts the block of attributes definition of this class ?
        /// </summary>
        public long AttributesDefinitionPosition { get; set; }

        public OID NextClassOID
        {
            get { return _oidInfo.NextClassOID; }
            set { _oidInfo.NextClassOID = value; }
        }

        public OID PreviousClassOID
        {
            get { return _oidInfo.PreviousClassOID; }
            set { _oidInfo.PreviousClassOID = value; }
        }

        /// <summary>
        ///   Physical location of this class in the file (in byte)
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        ///   The full class name with namespace
        /// </summary>
        public string FullClassName
        {
            get { return _fullClassName; }
        }

        public OID ClassInfoId
        {
            get { return _oidInfo.ID; }
            set { _oidInfo.ID = value; }
        }

        /// <summary>
        ///   The max id is used to give a unique id for each attribute and allow refactoring like new field and/or removal
        /// </summary>
        public int MaxAttributeId { get; set; }

        public int NumberOfAttributes
        {
            get { return _attributes.Count; }
        }

        /// <summary>
        ///   Infos about the last object of this class
        /// </summary>
        public ObjectInfoHeader LastObjectInfoHeader { get; set; }

        internal CIZoneInfo UncommittedZoneInfo
        {
            get { return _uncommitted; }
        }

        /// <summary>
        ///   Get number of objects: committed and uncommitted
        /// </summary>
        /// <value> The number of committed and uncommitted objects </value>
        public long NumberOfObjects
        {
            get { return _committed.GetNumberbOfObjects() + _uncommitted.GetNumberbOfObjects(); }
        }

        internal CommittedCIZoneInfo OriginalZoneInfo
        {
            get { return _original; }
        }

        public Type UnderlyingType
        {
            get { return _underlyingType; }
        }

        private static Type CheckIfTypeIsInstantiable(string fullClassName)
        {
            return TypeCache.GetOrAdd(fullClassName, GetType);
        }

        private static Type GetType(string fullClassName)
        {
            var type = TypeResolutionUtils.ResolveType(fullClassName);

            if (type == null)
                CannotInstantiateType(fullClassName);

            return type;
        }

        private static void CannotInstantiateType(string fullClassName)
        {
            var message = string.Format("Given full class name is not enough to create the Type from that: {0}",
                                        fullClassName);

            throw new ArgumentException(message);
        }

        private void FillAttributesMap()
        {
            if (_attributesCache.AttributesByName == null)
            {
                _attributesCache.AttributesByName = new OdbHashMap<string, ClassAttributeInfo>();
                _attributesCache.AttributesById = new OdbHashMap<int, ClassAttributeInfo>();
            }

            foreach (var classAttributeInfo in _attributes)
            {
                _attributesCache.AttributesByName[classAttributeInfo.GetName()] = classAttributeInfo;
                _attributesCache.AttributesById[classAttributeInfo.GetId()] = classAttributeInfo;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof (ClassInfo))
                return false;

            var classInfo = (ClassInfo) obj;
            return classInfo._fullClassName.Equals(_fullClassName);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append(" [ ").Append(_fullClassName).Append(" - id=").Append(_oidInfo.ID);
            buffer.Append(" - previousClass=").Append(_oidInfo.PreviousClassOID).Append(" - nextClass=").Append(
                _oidInfo.NextClassOID).Append(" - attributes=(");

            if (_attributes != null)
            {
                foreach (var classAttributeInfo in _attributes)
                    buffer.Append(classAttributeInfo.GetName()).Append(",");
            }
            else
                buffer.Append("not yet defined");

            buffer.Append(") ]");

            return buffer.ToString();
        }

        /// <summary>
        ///   This method could be optimized, but it is only on Class creation, one time in the database life time...
        /// </summary>
        /// <remarks>
        ///   This method could be optimized, but it is only on Class creation, one time in the database life time... 
        ///   This is used to get all (non native) attributes a class info have to store them in the meta model
        ///   before storing the class itself
        /// </remarks>
        internal IOdbList<ClassAttributeInfo> GetAllNonNativeAttributes()
        {
            IOdbList<ClassAttributeInfo> result = new OdbList<ClassAttributeInfo>(_attributes.Count);

            foreach (var classAttributeInfo in _attributes)
            {
                if (!classAttributeInfo.IsNative() || classAttributeInfo.GetAttributeType().IsEnum())
                    result.Add(classAttributeInfo);
                else
                {
                    if (classAttributeInfo.GetAttributeType().IsArray() &&
                        !classAttributeInfo.GetAttributeType().SubType.IsNative())
                        result.Add(new ClassAttributeInfo(-1, "subtype",
                                                          classAttributeInfo.GetAttributeType().SubType.Name, null));
                }
            }

            return result;
        }

        public override int GetHashCode()
        {
            return (_fullClassName != null
                        ? _fullClassName.GetHashCode()
                        : 0);
        }

        internal ClassAttributeInfo GetAttributeInfoFromId(int id)
        {
            return _attributesCache.AttributesById[id];
        }

        public int GetAttributeId(string name)
        {
            var classAttributeInfo = _attributesCache.AttributesByName[name];

            if (classAttributeInfo != null)
                return classAttributeInfo.GetId();

            var enrichedName = EnrichNameForAutoProperty(name);

            classAttributeInfo = _attributesCache.AttributesByName[enrichedName];

            return classAttributeInfo != null
                       ? classAttributeInfo.GetId()
                       : -1;
        }

        private static string EnrichNameForAutoProperty(string name)
        {
            return string.Format("<{0}>k__BackingField", name);
        }

        internal ClassAttributeInfo GetAttributeInfoFromName(string name)
        {
            return _attributesCache.AttributesByName[name];
        }

        internal ClassAttributeInfo GetAttributeInfo(int index)
        {
            return _attributes[index];
        }

        public ClassInfoCompareResult ExtractDifferences(ClassInfo newCI, bool update)
        {
            string attributeName;
            ClassAttributeInfo cai1;
            ClassAttributeInfo cai2;

            var result = new ClassInfoCompareResult(FullClassName);
            IOdbList<ClassAttributeInfo> attributesToRemove = new OdbList<ClassAttributeInfo>(10);
            IOdbList<ClassAttributeInfo> attributesToAdd = new OdbList<ClassAttributeInfo>(10);

            var attributesCount = _attributes.Count;
            for (var id = 0; id < attributesCount; id++)
            {
                // !!!WARNING : ID start with 1 and not 0
                cai1 = _attributes[id];
                if (cai1 == null)
                    continue;
                attributeName = cai1.GetName();
                cai2 = newCI.GetAttributeInfoFromId(cai1.GetId());
                if (cai2 == null)
                {
                    result.AddCompatibleChange(string.Format("Field '{0}' has been removed", attributeName));
                    if (update)
                    {
                        // Simply remove the attribute from meta-model
                        attributesToRemove.Add(cai1);
                    }
                }
                else
                {
                    if (!OdbType.TypesAreCompatible(cai1.GetAttributeType(), cai2.GetAttributeType()))
                    {
                        result.AddIncompatibleChange(
                            string.Format("Type of Field '{0}' has changed : old='{1}' - new='{2}'", attributeName,
                                          cai1.GetFullClassname(), cai2.GetFullClassname()));
                    }
                }
            }

            var nbNewAttributes = newCI._attributes.Count;
            for (var id = 0; id < nbNewAttributes; id++)
            {
                // !!!WARNING : ID start with 1 and not 0
                cai2 = newCI._attributes[id];
                if (cai2 == null)
                    continue;
                attributeName = cai2.GetName();
                cai1 = GetAttributeInfoFromId(cai2.GetId());
                if (cai1 == null)
                {
                    result.AddCompatibleChange("Field '" + attributeName + "' has been added");
                    if (update)
                    {
                        // Sets the right id of attribute
                        cai2.SetId(MaxAttributeId + 1);
                        MaxAttributeId++;
                        // Then adds the new attribute to the meta-model
                        attributesToAdd.Add(cai2);
                    }
                }
            }
            _attributes.RemoveAll(attributesToRemove);
            _attributes.AddAll(attributesToAdd);
            FillAttributesMap();
            return result;
        }

        public ClassInfoIndex AddIndexOn(string name, string[] indexFields, bool acceptMultipleValuesForSameKey)
        {
            if (_indexes == null)
                _indexes = new OdbList<ClassInfoIndex>();

            var cii = new ClassInfoIndex
                {
                    ClassInfoId = _oidInfo.ID,
                    Name = name,
                    IsUnique = !acceptMultipleValuesForSameKey
                };

            var attributeIds = new int[indexFields.Length];

            for (var i = 0; i < indexFields.Length; i++)
                attributeIds[i] = GetAttributeId(indexFields[i]);

            cii.AttributeIds = attributeIds;
            _indexes.Add(cii);
            return cii;
        }

        public void RemoveIndex(ClassInfoIndex cii)
        {
            _indexes.Remove(cii);
        }

        public void SetIndexes(IOdbList<ClassInfoIndex> indexes2)
        {
            _indexes = indexes2;
        }

        /// <summary>
        ///   To detect if a class has cyclic reference
        /// </summary>
        /// <returns> true if this class info has cyclic references </returns>
        internal bool HasCyclicReference()
        {
            return HasCyclicReference(new OdbHashMap<string, ClassInfo>());
        }

        /// <summary>
        ///   To detect if a class has cyclic reference
        /// </summary>
        /// <param name="alreadyVisitedClasses"> A dictionary containing all the already visited classes </param>
        /// <returns> true if this class info has cyclic references </returns>
        private bool HasCyclicReference(IDictionary<string, ClassInfo> alreadyVisitedClasses)
        {
            if (alreadyVisitedClasses[_fullClassName] != null)
                return true;

            alreadyVisitedClasses.Add(_fullClassName, this);

            for (var i = 0; i < _attributes.Count; i++)
            {
                var classAttributeInfo = GetAttributeInfo(i);
                if (classAttributeInfo.IsNative())
                    continue;

                var localMap = new OdbHashMap<string, ClassInfo>(alreadyVisitedClasses);
                var hasCyclicRef = classAttributeInfo.GetClassInfo().HasCyclicReference(localMap);
                if (hasCyclicRef)
                    return true;
            }
            return false;
        }

        public ClassInfoIndex GetIndexWithName(string name)
        {
            return _indexes == null
                       ? null
                       : _indexes.FirstOrDefault(classInfoIndex => classInfoIndex.Name.Equals(name));
        }

        public ClassInfoIndex GetIndexForAttributeIds(int[] attributeIds)
        {
            return _indexes == null
                       ? null
                       : _indexes.FirstOrDefault(classInfoIndex => classInfoIndex.MatchAttributeIds(attributeIds));
        }

        public string[] GetAttributeNames(int[] attributeIds)
        {
            var attributeIdsLength = attributeIds.Length;
            var names = new string[attributeIdsLength];

            for (var i = 0; i < attributeIdsLength; i++)
                names[i] = GetAttributeInfoFromId(attributeIds[i]).GetName();

            return names.Select(name => name.StartsWith("<")
                                            ? name.Substring(1, name.IndexOf('>') - 1)
                                            : name).ToArray();
        }

        public IOdbList<ClassInfoIndex> GetIndexes()
        {
            return _indexes ?? new OdbList<ClassInfoIndex>();
        }

        internal void RemoveAttribute(ClassAttributeInfo cai)
        {
            _attributes.Remove(cai);
            _attributesCache.AttributesByName.Remove(cai.GetName());
        }

        internal void AddAttribute(ClassAttributeInfo cai)
        {
            cai.SetId(MaxAttributeId++);
            _attributes.Add(cai);
            _attributesCache.AttributesByName.Add(cai.GetName(), cai);
        }

        public bool HasIndex(string indexName)
        {
            return _indexes != null && _indexes.Any(classInfoIndex => indexName.Equals(classInfoIndex.Name));
        }

        public bool HasIndex()
        {
            return _indexes != null && !_indexes.IsEmpty();
        }

        internal ClassAttributeInfo GetAttributeInfo(int attributeId, string attributeNameToSearch)
        {
            if (attributeId == -1)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.CriteriaQueryUnknownAttribute.AddParameter(attributeNameToSearch).
                                   AddParameter(FullClassName));
            }

            return GetAttributeInfoFromId(attributeId);
        }
    }
}
