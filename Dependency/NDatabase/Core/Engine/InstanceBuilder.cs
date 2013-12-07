using System;
using System.Globalization;
using System.Runtime.Serialization;
using NDatabase.Api;
using NDatabase.Cache;
using NDatabase.Exceptions;
using NDatabase.Meta;
using NDatabase.Meta.Introspector;
using NDatabase.Tool;
using NDatabase.Triggers;

namespace NDatabase.Core.Engine
{
    /// <summary>
    ///   Class used to build instance from Meta Object representation.
    /// </summary>
    /// <remarks>
    ///   Class used to build instance from Meta Object representation. Layer 2 to Layer 1 conversion.
    /// </remarks>
    internal sealed class InstanceBuilder : IInstanceBuilder
    {
        private readonly IStorageEngine _engine;
        private readonly IInternalTriggerManager _triggerManager;

        public InstanceBuilder(IStorageEngine engine)
        {
            _triggerManager = engine.GetLocalTriggerManager();
            _engine = engine;
        }

        #region IInstanceBuilder Members

        public object BuildOneInstance(NonNativeObjectInfo objectInfo)
        {
            return BuildOneInstance(objectInfo, _engine.GetSession().GetCache());
        }

        public object BuildOneInstance(NonNativeObjectInfo objectInfo, IOdbCache cache)
        {
            // verify if the object is check to delete
            if (objectInfo.IsDeletedObject())
                throw new OdbRuntimeException(
                    NDatabaseError.ObjectIsMarkedAsDeletedForOid.AddParameter(objectInfo.GetOid()));

            // Then check if object is in cache
            var o = cache.GetObject(objectInfo.GetOid());
            if (o != null)
                return o;

            try
            {
                o = FormatterServices.GetUninitializedObject(objectInfo.GetClassInfo().UnderlyingType);
            }
            catch (Exception e)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.InstanciationError.AddParameter(objectInfo.GetClassInfo().FullClassName), e);
            }

            // This can happen if ODB can not create the instance from security reasons
            if (o == null)
                throw new OdbRuntimeException(
                    NDatabaseError.InstanciationError.AddParameter(objectInfo.GetClassInfo().FullClassName));

            // Keep the initial hash code. In some cases, when the class redefines
            // the hash code method
            // Hash code can return wrong values when attributes are not set (when
            // hash code depends on attribute values)
            // Hash codes are used as the key of the map,
            // So at the end of this method, if hash codes are different, object
            // will be removed from the cache and inserted back
            var hashCodeIsOk = true;
            var initialHashCode = 0;

            try
            {
                initialHashCode = o.GetHashCode();
            }
            catch
            {
                hashCodeIsOk = false;
            }

            // Adds this incomplete instance in the cache to manage cyclic reference
            if (hashCodeIsOk)
                cache.AddObject(objectInfo.GetOid(), o, objectInfo.GetHeader());

            var classInfo = objectInfo.GetClassInfo();
            var fields = ClassIntrospector.GetAllFieldsFrom(classInfo.UnderlyingType);

            object value = null;

            foreach (var fieldInfo in fields)
            {
                // Gets the id of this field
                var attributeId = classInfo.GetAttributeId(fieldInfo.Name);
                if (OdbConfiguration.IsLoggingEnabled())
                    DLogger.Debug(string.Concat("InstanceBuilder: ", "getting field with name ", fieldInfo.Name, ", attribute id is ",
                                                attributeId.ToString()));

                var abstractObjectInfo = objectInfo.GetAttributeValueFromId(attributeId);

                // Check consistency
                // ensureClassCompatibily(field,
                // instanceInfo.getClassInfo().getAttributeinfo(i).getFullClassname());
                if (abstractObjectInfo == null || (abstractObjectInfo.IsNull())) 
                    continue;

                if (abstractObjectInfo.IsNative())
                {
                    if (abstractObjectInfo.IsAtomicNativeObject())
                    {
                        value = abstractObjectInfo.IsNull()
                                    ? null
                                    : abstractObjectInfo.GetObject();
                    }

                    if (abstractObjectInfo.IsArrayObject())
                        value = BuildArrayInstance((ArrayObjectInfo) abstractObjectInfo);
                    if (abstractObjectInfo.IsEnumObject())
                        value = BuildEnumInstance((EnumNativeObjectInfo) abstractObjectInfo, fieldInfo.FieldType);
                }
                else
                {
                    if (abstractObjectInfo.IsNonNativeObject())
                    {
                        if (abstractObjectInfo.IsDeletedObject())
                        {
                            if (OdbConfiguration.IsLoggingEnabled())
                            {
                                var warning =
                                    NDatabaseError.AttributeReferencesADeletedObject.AddParameter(
                                        objectInfo.GetClassInfo().FullClassName).AddParameter(
                                            objectInfo.GetOid()).AddParameter(fieldInfo.Name);
                                DLogger.Warning("InstanceBuilder: " + warning);
                            }
                            value = null;
                        }
                        else
                            value = BuildOneInstance((NonNativeObjectInfo) abstractObjectInfo);
                    }
                }
                if (value == null) 
                    continue;

                if (OdbConfiguration.IsLoggingEnabled())
                {
                    DLogger.Debug(String.Format("InstanceBuilder: Setting field {0}({1}) to {2} / {3}", fieldInfo.Name,
                                                fieldInfo.GetType().FullName, value, value.GetType().FullName));
                }
                try
                {
                    fieldInfo.SetValue(o, value);
                }
                catch (Exception e)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.InstanceBuilderWrongObjectContainerType.AddParameter(
                            objectInfo.GetClassInfo().FullClassName).AddParameter(value.GetType().FullName)
                            .AddParameter(fieldInfo.GetType().FullName), e);
                }
            }
            if (o.GetType() != objectInfo.GetClassInfo().UnderlyingType)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.InstanceBuilderWrongObjectType.AddParameter(
                        objectInfo.GetClassInfo().FullClassName).AddParameter(o.GetType().FullName));
            }
            if (hashCodeIsOk || initialHashCode != o.GetHashCode())
            {
                // Bug (sf bug id=1875544 )detected by glsender
                // This can happen when an object has redefined its own hashcode
                // method and depends on the field values
                // Then, we have to remove object from the cache and re-insert to
                // correct map hash code
                cache.RemoveObjectByOid(objectInfo.GetOid());
                // re-Adds instance in the cache
                cache.AddObject(objectInfo.GetOid(), o, objectInfo.GetHeader());
            }

            if (_triggerManager != null)
                _triggerManager.ManageSelectTriggerAfter(objectInfo.GetClassInfo().UnderlyingType, o,
                                                         objectInfo.GetOid());

            return o;
        }

        #endregion

        private object BuildOneInstance(AbstractObjectInfo objectInfo)
        {
            if (objectInfo.IsNull())
                return null;

            var instance = objectInfo.GetType() == typeof (NonNativeObjectInfo)
                               ? BuildOneInstance((NonNativeObjectInfo) objectInfo)
                               : BuildOneInstance((NativeObjectInfo) objectInfo);

            return instance;
        }

        /// <summary>
        ///   Builds an instance of an enum
        /// </summary>
        private static object BuildEnumInstance(EnumNativeObjectInfo enoi, Type enumClass)
        {
            return Enum.Parse(enumClass, enoi.GetEnumValue(), false);
        }

        /// <summary>
        ///   Builds an instance of an array
        /// </summary>
        private object BuildArrayInstance(ArrayObjectInfo aoi)
        {
            // first check if array element type is native (int,short, for example)
            var type = OdbType.GetFromName(aoi.GetRealArrayComponentClassName());

            var arrayClazz = type.GetNativeClass();
            object array = Array.CreateInstance(arrayClazz, aoi.GetArray().Length);

            for (var i = 0; i < aoi.GetArrayLength(); i++)
            {
                var abstractObjectInfo = (AbstractObjectInfo) aoi.GetArray()[i];
                if (abstractObjectInfo == null || abstractObjectInfo.IsDeletedObject() || abstractObjectInfo.IsNull())
                    continue;

                var instance = BuildOneInstance(abstractObjectInfo);
                ((Array) array).SetValue(instance, i);
            }
            return array;
        }

        private object BuildOneInstance(NativeObjectInfo objectInfo)
        {
            if (objectInfo.IsAtomicNativeObject())
                return BuildOneInstance((AtomicNativeObjectInfo) objectInfo);
            if (objectInfo.IsArrayObject())
                return BuildArrayInstance((ArrayObjectInfo) objectInfo);
            if (objectInfo.IsNull())
                return null;
            throw new OdbRuntimeException(
                NDatabaseError.InstanceBuilderNativeType.AddParameter(OdbType.GetNameFromId(objectInfo.GetOdbTypeId())));
        }

        private static object BuildOneInstance(AtomicNativeObjectInfo objectInfo)
        {
            var odbTypeId = objectInfo.GetOdbTypeId();
            

            return CheckIfNull(objectInfo, odbTypeId);
        }

        private static object CheckIfNull(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            return odbTypeId == OdbType.NullId ? null : CheckIfString(objectInfo, odbTypeId);
        }

        private static object CheckIfString(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            return odbTypeId == OdbType.StringId ? objectInfo.GetObject() : CheckIfDate(objectInfo, odbTypeId);
        }

        private static object CheckIfDate(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            return odbTypeId == OdbType.DateId ? objectInfo.GetObject() : CheckIfLong(objectInfo, odbTypeId);
        }

        private static object CheckIfLong(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            if (odbTypeId == OdbType.LongId)
            {
                if (objectInfo.GetObject() is long)
                    return objectInfo.GetObject();
                return Convert.ToInt64(objectInfo.GetObject().ToString());
            }

            if (odbTypeId == OdbType.ULongId)
            {
                if (objectInfo.GetObject() is ulong)
                    return objectInfo.GetObject();
                return Convert.ToUInt64(objectInfo.GetObject().ToString());
            }

            return CheckIfInt(objectInfo, odbTypeId);
        }

        private static object CheckIfInt(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            if (odbTypeId == OdbType.IntegerId)
            {
                if (objectInfo.GetObject() is int)
                    return objectInfo.GetObject();
                return Convert.ToInt32(objectInfo.GetObject().ToString());
            }

            if (odbTypeId == OdbType.UIntegerId)
            {
                if (objectInfo.GetObject() is uint)
                    return objectInfo.GetObject();
                return Convert.ToUInt32(objectInfo.GetObject().ToString());
            }

            return CheckIfBool(objectInfo, odbTypeId);
        }

        private static object CheckIfBool(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            if (odbTypeId == OdbType.BooleanId)
            {
                if (objectInfo.GetObject() is bool)
                    return objectInfo.GetObject();
                return Convert.ToBoolean(objectInfo.GetObject().ToString());
            }

            return CheckIfByte(objectInfo, odbTypeId);
        }

        private static object CheckIfByte(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            if (odbTypeId == OdbType.ByteId)
            {
                if (objectInfo.GetObject() is byte)
                    return objectInfo.GetObject();
                return Convert.ToByte(objectInfo.GetObject().ToString());
            }

            if (odbTypeId == OdbType.SByteId)
            {
                if (objectInfo.GetObject() is sbyte)
                    return objectInfo.GetObject();
                return Convert.ToSByte(objectInfo.GetObject().ToString());
            }

            return CheckIfShort(objectInfo, odbTypeId);
        }

        private static object CheckIfShort(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            if (odbTypeId == OdbType.ShortId)
            {
                if (objectInfo.GetObject() is short)
                    return objectInfo.GetObject();
                return Convert.ToInt16(objectInfo.GetObject().ToString());
            }

            if (odbTypeId == OdbType.UShortId)
            {
                if (objectInfo.GetObject() is ushort)
                    return objectInfo.GetObject();
                return Convert.ToUInt16(objectInfo.GetObject().ToString());
            }

            return CheckIfFloatOrDouble(objectInfo, odbTypeId);
        }

        private static object CheckIfFloatOrDouble(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            if (odbTypeId == OdbType.FloatId)
            {
                if (objectInfo.GetObject() is float)
                    return objectInfo.GetObject();
                return Convert.ToSingle(objectInfo.GetObject().ToString());
            }

            if (odbTypeId == OdbType.DoubleId)
            {
                if (objectInfo.GetObject() is double)
                    return objectInfo.GetObject();
                return Convert.ToDouble(objectInfo.GetObject().ToString());
            }

            return CheckIfDecimal(objectInfo, odbTypeId);
        }

        private static object CheckIfDecimal(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            return odbTypeId == OdbType.DecimalId
                       ? Decimal.Parse(objectInfo.GetObject().ToString(), NumberStyles.Any)
                       : CheckIfCharacter(objectInfo, odbTypeId);
        }

        private static object CheckIfCharacter(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            if (odbTypeId == OdbType.CharacterId)
            {
                if (objectInfo.GetObject() is char)
                    return objectInfo.GetObject();
                return objectInfo.GetObject().ToString()[0];
            }

            return CheckIfOid(objectInfo, odbTypeId);
        }

        private static object CheckIfOid(AbstractObjectInfo objectInfo, int odbTypeId)
        {
            long l;
            if (odbTypeId == OdbType.ObjectOidId)
            {
                if (objectInfo.GetObject() is long)
                {
                    l = (long) objectInfo.GetObject();
                }
                else
                {
                    var oid = (OID) objectInfo.GetObject();
                    l = oid.ObjectId;
                }

                return OIDFactory.BuildObjectOID(l);
            }
            
            if (odbTypeId == OdbType.ClassOidId)
            {
                if (objectInfo.GetObject() is long)
                    l = (long) objectInfo.GetObject();
                else
                    l = Convert.ToInt64(objectInfo.GetObject().ToString());
                return OIDFactory.BuildClassOID(l);
            }
            
            return ThrowIfNotFound(odbTypeId);
        }

        private static object ThrowIfNotFound(int odbTypeId)
        {
            throw new OdbRuntimeException(
                NDatabaseError.InstanceBuilderNativeTypeInCollectionNotSupported.AddParameter(
                    OdbType.GetNameFromId(odbTypeId)));
        }
    }
}
