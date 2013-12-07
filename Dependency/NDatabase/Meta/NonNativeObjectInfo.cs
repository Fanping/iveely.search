using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NDatabase.Api;
using NDatabase.Exceptions;

namespace NDatabase.Meta
{
    /// <summary>
    ///   To keep info about a non native object <pre>- Keeps its class info : a meta information to describe its type
    ///                                            - All its attributes values
    ///                                            - Its Pointers : its position, the previous object OID, the next object OID
    ///                                            - The Object being represented by The meta information</pre>
    /// </summary>
    internal class NonNativeObjectInfo : AbstractObjectInfo
    {
        private readonly int _maxNbattributes;
        private AbstractObjectInfo[] _attributeValues;
        private ClassInfo _classInfo;
        private ObjectInfoHeader _objectHeader;

        /// <summary>
        ///   The object being represented
        /// </summary>
        [NonPersistent]
        private object _theObject;

        public NonNativeObjectInfo(ObjectInfoHeader oip, ClassInfo classInfo) : base(null)
        {
            _classInfo = classInfo;
            _objectHeader = oip;

            if (classInfo != null)
            {
                _maxNbattributes = classInfo.MaxAttributeId;
                _attributeValues = new AbstractObjectInfo[_maxNbattributes];
            }
        }

        protected NonNativeObjectInfo(ClassInfo classInfo) : base(null)
        {
            _classInfo = classInfo;
            _objectHeader = new ObjectInfoHeader(-1, null, null, (classInfo != null
                                                                      ? classInfo.ClassInfoId
                                                                      : null), null, null);
            if (classInfo != null)
            {
                _maxNbattributes = classInfo.MaxAttributeId;
                _attributeValues = new AbstractObjectInfo[_maxNbattributes];
            }
        }

        public NonNativeObjectInfo(object @object, ClassInfo info)
            : this(@object, info, null, null, null)
        {
        }

        private NonNativeObjectInfo(object @object, ClassInfo info, AbstractObjectInfo[] values,
                                   long[] attributesIdentification, int[] attributeIds)
            : base(OdbType.GetFromName(info.FullClassName))
        {
            _theObject = @object;
            _classInfo = info;
            _attributeValues = values;
            _maxNbattributes = _classInfo.MaxAttributeId;

            if (_attributeValues == null)
                _attributeValues = new AbstractObjectInfo[_maxNbattributes];

            _objectHeader = new ObjectInfoHeader(-1, null, null, (_classInfo != null
                                                                      ? _classInfo.ClassInfoId
                                                                      : null), attributesIdentification, attributeIds);
        }

        public ObjectInfoHeader GetHeader()
        {
            return _objectHeader;
        }

        public AbstractObjectInfo GetAttributeValueFromId(int attributeId)
        {
            return _attributeValues[attributeId - 1];
        }

        public ClassInfo GetClassInfo()
        {
            return _classInfo;
        }

        public void SetClassInfo(ClassInfo classInfo)
        {
            if (classInfo != null)
            {
                _classInfo = classInfo;
                _objectHeader.SetClassInfoId(classInfo.ClassInfoId);
            }
        }

        public override string ToString()
        {
            var buffer = new StringBuilder(_classInfo.FullClassName).Append("(").Append(GetOid()).Append(")=");

            if (_attributeValues == null)
            {
                buffer.Append("null attribute values");
                return buffer.ToString();
            }

            for (var i = 0; i < _attributeValues.Length; i++)
            {
                if (i != 0)
                    buffer.Append(",");

                var attributeName = (_classInfo != null
                                         ? (_classInfo.GetAttributeInfo(i)).GetName()
                                         : "?");

                buffer.Append(attributeName).Append("=");
                object @object = _attributeValues[i];

                if (@object == null)
                    buffer.Append(" null object - should not happen , ");
                else
                {
                    var type = OdbType.GetFromClass(_attributeValues[i].GetType());
                    if (@object is NonNativeNullObjectInfo)
                    {
                        buffer.Append("null");
                        continue;
                    }
                    if (@object is NonNativeDeletedObjectInfo)
                    {
                        buffer.Append("deleted object");
                        continue;
                    }
                    var noi = @object as NativeObjectInfo;
                    if (noi != null)
                    {
                        buffer.Append(noi);
                        continue;
                    }
                    var nnoi = @object as NonNativeObjectInfo;
                    if (nnoi != null)
                    {
                        buffer.Append("@").Append(nnoi.GetClassInfo().FullClassName).Append("(id=").Append(
                            nnoi.GetOid()).Append(")");
                        continue;
                    }
                    if (@object is ObjectReference)
                    {
                        buffer.Append(@object);
                        continue;
                    }
                    buffer.Append("@").Append(OdbClassNameResolver.GetClassName(type.Name));
                }
            }

            return buffer.ToString();
        }

        public OID GetNextObjectOID()
        {
            return _objectHeader.GetNextObjectOID();
        }

        public void SetNextObjectOID(OID nextObjectOID)
        {
            _objectHeader.SetNextObjectOID(nextObjectOID);
        }

        public OID GetPreviousObjectOID()
        {
            return _objectHeader.GetPreviousObjectOID();
        }

        public void SetPreviousInstanceOID(OID previousObjectOID)
        {
            _objectHeader.SetPreviousObjectOID(previousObjectOID);
        }

        public void SetPosition(long position)
        {
            _objectHeader.SetPosition(position);
        }

        public override object GetObject()
        {
            return _theObject;
        }

        public object GetValueOf(string attributeName)
        {
            Debug.Assert(attributeName != null);

            var isRelation = attributeName.IndexOf(".", StringComparison.Ordinal) != -1;

            if (!isRelation)
            {
                var attributeId = GetClassInfo().GetAttributeId(attributeName);
                return GetAttributeValueFromId(attributeId).GetObject();
            }

            var firstDotIndex = attributeName.IndexOf(".", StringComparison.Ordinal);
            var nnoi = GetNonNativeObjectInfo(attributeName, firstDotIndex);

            if (nnoi != null)
            {
                var beginIndex = firstDotIndex + 1;
                return nnoi.GetValueOf(attributeName.Substring(beginIndex, attributeName.Length - beginIndex));
            }

            throw new OdbRuntimeException(
                NDatabaseError.ClassInfoDoNotHaveTheAttribute.AddParameter(GetClassInfo().FullClassName).
                    AddParameter(attributeName));
        }

        private NonNativeObjectInfo GetNonNativeObjectInfo(string attributeName, int firstDotIndex)
        {
            var firstAttributeName = attributeName.Substring(0, firstDotIndex);
            var attributeId = GetClassInfo().GetAttributeId(firstAttributeName);

            return _attributeValues[attributeId] as NonNativeObjectInfo;
        }

        /// <summary>
        ///   Used to change the value of an attribute
        /// </summary>
        /// <param name="attributeName"> </param>
        /// <param name="aoi"> </param>
        public void SetValueOf(string attributeName, AbstractObjectInfo aoi)
        {
            var isRelation = attributeName.IndexOf(".", StringComparison.Ordinal) != -1;

            if (!isRelation)
            {
                var attributeId = GetClassInfo().GetAttributeId(attributeName);
                SetAttributeValue(attributeId, aoi);
                return;
            }

            var firstDotIndex = attributeName.IndexOf(".", StringComparison.Ordinal);

            var nnoi = GetNonNativeObjectInfo(attributeName, firstDotIndex);

            if (nnoi != null)
            {
                var beginIndex = firstDotIndex + 1;
                nnoi.SetValueOf(attributeName.Substring(beginIndex, attributeName.Length - beginIndex), aoi);
            }

            throw new OdbRuntimeException(
                NDatabaseError.ClassInfoDoNotHaveTheAttribute.AddParameter(GetClassInfo().FullClassName).
                    AddParameter(attributeName));
        }

        public OID GetOid()
        {
            if (GetHeader() == null)
                throw new OdbRuntimeException(NDatabaseError.UnexpectedSituation.AddParameter("Null Object Info Header"));
            return GetHeader().GetOid();
        }

        public void SetOid(OID oid)
        {
            if (GetHeader() != null)
                GetHeader().SetOid(oid);
        }

        public override bool IsNonNativeObject()
        {
            return true;
        }

        public override bool IsNull()
        {
            return false;
        }

        /// <summary>
        ///   Create a copy oh this meta object
        /// </summary>
        /// <param name="cache"> </param>
        /// <param name="onlyData"> if true, only copy attributes values </param>
        /// <returns> </returns>
        public override AbstractObjectInfo CreateCopy(IDictionary<OID, AbstractObjectInfo> cache, bool onlyData)
        {
            NonNativeObjectInfo nnoi;

            if (_objectHeader.GetOid() != null && cache.ContainsKey(_objectHeader.GetOid()))
            {
                nnoi = (NonNativeObjectInfo) cache[_objectHeader.GetOid()];
                if (nnoi != null)
                    return nnoi;
            }

            if (_theObject == null)
                return new NonNativeNullObjectInfo(_classInfo);

            if (onlyData)
            {
                var oih = new ObjectInfoHeader();
                nnoi = new NonNativeObjectInfo(_theObject, _classInfo, null, oih.GetAttributesIdentification(),
                                               oih.GetAttributeIds());
            }
            else
            {
                nnoi = new NonNativeObjectInfo(_theObject, _classInfo, null, _objectHeader.GetAttributesIdentification(),
                                               _objectHeader.GetAttributeIds());

                nnoi.GetHeader().SetOid(GetHeader().GetOid());
            }

            var newAttributeValues = new AbstractObjectInfo[_attributeValues.Length];

            for (var i = 0; i < _attributeValues.Length; i++)
                newAttributeValues[i] = _attributeValues[i].CreateCopy(cache, onlyData);

            nnoi._attributeValues = newAttributeValues;

            if (_objectHeader.GetOid() != null)
                cache.Add(_objectHeader.GetOid(), nnoi);

            return nnoi;
        }

        public void SetAttributeValue(int attributeId, AbstractObjectInfo aoi)
        {
            _attributeValues[attributeId - 1] = aoi;
        }

        public AbstractObjectInfo[] GetAttributeValues()
        {
            return _attributeValues;
        }

        public int GetMaxNbattributes()
        {
            return _maxNbattributes;
        }

        public void SetObject(object @object)
        {
            _theObject = @object;
        }

        public override int GetHashCode()
        {
            // This happens when the object is deleted
            if (_objectHeader == null)
                return -1;

            return _objectHeader.GetHashCode();
        }

        public void SetHeader(ObjectInfoHeader header)
        {
            _objectHeader = header;
        }
    }
}
