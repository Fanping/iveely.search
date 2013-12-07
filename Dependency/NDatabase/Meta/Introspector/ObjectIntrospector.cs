using System;
using System.Collections.Generic;
using NDatabase.Exceptions;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Meta.Introspector
{
    /// <summary>
    ///   The local implementation of the Object Introspector.
    /// </summary>
    internal sealed class ObjectIntrospector : IObjectIntrospector
    {
        private readonly IObjectIntrospectionDataProvider _classInfoProvider;
        
        public ObjectIntrospector(IObjectIntrospectionDataProvider classInfoProvider)
        {
            _classInfoProvider = classInfoProvider;
        }

        #region IObjectIntrospector Members

        public AbstractObjectInfo GetMetaRepresentation(object plainObject, bool recursive,
                                                        IDictionary<object, NonNativeObjectInfo> alreadyReadObjects,
                                                        IIntrospectionCallback callback)
        {
            if (plainObject == null)
                return GetObjectInfo(null, null, recursive, alreadyReadObjects, callback);

            // The object must be transformed into meta representation
            var type = plainObject.GetType();

            var classInfo = _classInfoProvider.GetClassInfo(type);

            return GetObjectInfo(plainObject, classInfo, recursive, alreadyReadObjects, callback);
        }

        public void Clear()
        {
            _classInfoProvider.Clear();
        }

        #endregion

        private NonNativeObjectInfo BuildNnoi(object o, ClassInfo classInfo)
        {
            var nnoi = new NonNativeObjectInfo(o, classInfo);

            return _classInfoProvider.EnrichWithOid(nnoi, o);
        }

        /// <summary>
        ///   retrieve object data
        /// </summary>
        /// <returns> The object info </returns>
        private AbstractObjectInfo GetObjectInfo(object o, ClassInfo ci, bool recursive,
                                                 IDictionary<object, NonNativeObjectInfo> alreadyReadObjects,
                                                 IIntrospectionCallback callback)
        {
            return GetObjectInfoInternal(null, o, ci, recursive, alreadyReadObjects, callback);
        }

        private AbstractObjectInfo GetNativeObjectInfoInternal(OdbType type, object o, bool recursive,
                                                               IDictionary<object, NonNativeObjectInfo>
                                                                   alreadyReadObjects, IIntrospectionCallback callback)
        {
            AbstractObjectInfo aoi = null;
            if (type.IsAtomicNative())
            {
                if (o == null)
                    aoi = new NullNativeObjectInfo(type.Id);
                else
                    aoi = new AtomicNativeObjectInfo(o, type.Id);
            }
            else if (type.IsArray())
            {
                if (o == null)
                    aoi = new ArrayObjectInfo(null);
                else
                {
                    // Gets the type of the elements of the array
                    var realArrayClassName = OdbClassNameResolver.GetFullName(o.GetType().GetElementType());
                    var arrayObjectInfo = recursive
                                              ? IntrospectArray(o, alreadyReadObjects, type, callback)
                                              : new ArrayObjectInfo((object[]) o);

                    arrayObjectInfo.SetRealArrayComponentClassName(realArrayClassName);
                    aoi = arrayObjectInfo;
                }
            }
            else if (type.IsEnum())
            {
                var enumObject = (Enum) o;
                
                // Here we must check if the enum is already in the meta model. Enum must be stored in the meta
                // model to optimize its storing as we need to keep track of the enum class
                // for each enum stored. So instead of storing the enum class name, we can store enum class id, a long
                // instead of the full enum class name string
                var classInfo = GetClassInfo(enumObject.GetType());
                var enumValue = enumObject.ToString();
                aoi = new EnumNativeObjectInfo(classInfo, enumValue);
            }

            return aoi;
        }

        /// <summary>
        ///   Build a meta representation of an object 
        ///   <pre>warning: When an object has two fields with the same name 
        ///        (a private field with the same name in a parent class, the deeper field (of the parent) is ignored!)</pre>
        /// </summary>
        /// <returns> The ObjectInfo </returns>
        private AbstractObjectInfo GetObjectInfoInternal(AbstractObjectInfo nnoi, object o, ClassInfo classInfo,
                                                         bool recursive,
                                                         IDictionary<object, NonNativeObjectInfo> alreadyReadObjects,
                                                         IIntrospectionCallback callback)
        {
            if (o == null)
                return NullNativeObjectInfo.GetInstance();

            var clazz = o.GetType();
            var type = OdbType.GetFromClass(clazz);
            if (type.IsNative())
                return GetNativeObjectInfoInternal(type, o, recursive, alreadyReadObjects, callback);

            // sometimes the type.getName() may not match the ci.getClassName()
            // It happens when the attribute is an interface or superclass of the
            // real attribute class
            // In this case, ci must be updated to the real class info
            if (classInfo != null && !classInfo.FullClassName.Equals(OdbClassNameResolver.GetFullName(clazz)))
            {
                classInfo = GetClassInfo(clazz);
                nnoi = null;
            }
            var mainAoi = (NonNativeObjectInfo) nnoi;
            var isRootObject = false;

            if (alreadyReadObjects == null)
            {
                alreadyReadObjects = new OdbHashMap<object, NonNativeObjectInfo>();
                isRootObject = true;
            }

            NonNativeObjectInfo cachedNnoi;
            alreadyReadObjects.TryGetValue(o, out cachedNnoi);

            if (cachedNnoi != null)
                return new ObjectReference(cachedNnoi);

            if (callback != null)
                callback.ObjectFound(o);

            if (mainAoi == null)
                mainAoi = BuildNnoi(o, classInfo);

            alreadyReadObjects[o] = mainAoi;
            
            var fields = ClassIntrospector.GetAllFieldsFrom(clazz);
            
            foreach (var field in fields)
            {
                try
                {
                    var value = field.GetValue(o);
                    var attributeId = classInfo.GetAttributeId(field.Name);
                    if (attributeId == -1)
                    {
                        throw new OdbRuntimeException(
                            NDatabaseError.ObjectIntrospectorNoFieldWithName.AddParameter(classInfo.FullClassName).
                                AddParameter(field.Name));
                    }

                    var valueType = OdbType.GetFromClass(value == null
                                                             ? field.FieldType
                                                             : value.GetType());
                    // for native fields
                    AbstractObjectInfo abstractObjectInfo;

                    if (valueType.IsNative())
                    {
                        abstractObjectInfo = GetNativeObjectInfoInternal(valueType, value, recursive, alreadyReadObjects,
                                                                         callback);
                        mainAoi.SetAttributeValue(attributeId, abstractObjectInfo);
                    }
                    else
                    {
                        // Non Native Objects
                        if (value == null)
                        {
                            var classInfo1 = GetClassInfo(field.GetType());

                            abstractObjectInfo = new NonNativeNullObjectInfo(classInfo1);
                            mainAoi.SetAttributeValue(attributeId, abstractObjectInfo);
                        }
                        else
                        {
                            var classInfo2 = GetClassInfo(value.GetType());
                            if (recursive)
                            {
                                abstractObjectInfo = GetObjectInfoInternal(null, value, classInfo2, true,
                                                                           alreadyReadObjects, callback);
                                mainAoi.SetAttributeValue(attributeId, abstractObjectInfo);
                            }
                            else
                            {
                                // When it is not recursive, simply add the object
                                // values.add(value);
                                throw new OdbRuntimeException(
                                    NDatabaseError.InternalError.AddParameter(
                                        "Should not enter here - ObjectIntrospector - 'simply add the object'"));
                            }
                        }
                    }
                }
                catch (ArgumentException e)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.InternalError.AddParameter("in getObjectInfoInternal"), e);
                }
                catch (MemberAccessException e)
                {
                    throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter("getObjectInfoInternal"), e);
                }
            }

            if (isRootObject)
                alreadyReadObjects.Clear();

            return mainAoi;
        }

        private ClassInfo GetClassInfo(Type type)
        {
            return _classInfoProvider.GetClassInfo(type);
        }

        private ArrayObjectInfo IntrospectArray(object array,
                                                IDictionary<object, NonNativeObjectInfo> alreadyReadObjects,
                                                OdbType odbType, IIntrospectionCallback callback)
        {
            var length = ((Array) array).GetLength(0);
            var elementType = array.GetType().GetElementType();
            var type = OdbType.GetFromClass(elementType);

            if (type.IsAtomicNative())
                return IntropectAtomicNativeArray(array, type);

            var arrayCopy = new object[length];
            for (var i = 0; i < length; i++)
            {
                var o = ((Array) array).GetValue(i);
                if (o != null)
                {
                    var classInfo = GetClassInfo(o.GetType());
                    var abstractObjectInfo = GetObjectInfo(o, classInfo, true, alreadyReadObjects, callback);
                    arrayCopy[i] = abstractObjectInfo;
                }
                else
                    arrayCopy[i] = NullNativeObjectInfo.GetInstance();
            }

            return new ArrayObjectInfo(arrayCopy, odbType, type.Id);
        }

        private static ArrayObjectInfo IntropectAtomicNativeArray(object array, OdbType type)
        {
            var length = ((Array) array).GetLength(0);
            var arrayCopy = new object[length];
            for (var i = 0; i < length; i++)
            {
                var o = ((Array) array).GetValue(i);
                if (o != null)
                {
                    // If object is not null, try to get the exact type
                    var typeId = OdbType.GetFromClass(o.GetType()).Id;
                    var atomicNativeObjectInfo = new AtomicNativeObjectInfo(o, typeId);
                    arrayCopy[i] = atomicNativeObjectInfo;
                }
                else
                {
                    // Else take the declared type
                    arrayCopy[i] = new NullNativeObjectInfo(type.Id);
                }
            }

            return new ArrayObjectInfo(arrayCopy, OdbType.Array, type.Id);
        }
    }
}
