using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NDatabase.Api;
using NDatabase.Container;
using NDatabase.Services;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;
using NDatabase.TypeResolution;

namespace NDatabase.Meta.Introspector
{
    /// <summary>
    ///   The Class Introspector is used to introspect classes.
    /// </summary>
    /// <remarks>
    ///   The Class Introspector is used to introspect classes. 
    ///   It uses Reflection to extract class information. 
    ///   It transforms a native Class into a ClassInfo (a meta representation of the class) 
    ///   that contains all informations about the class.
    /// </remarks>
    internal static class ClassIntrospector
    {
        private static readonly Dictionary<Type, IList<FieldInfo>> Fields =
            new Dictionary<Type, IList<FieldInfo>>();

        /// <summary>
        /// </summary>
        /// <param name="type"> The class to introspect </param>
        /// <param name="recursive"> If true, goes does the hierarchy to try to analyze all classes </param>
        /// <returns> </returns>
        public static ClassInfoList Introspect(Type type, bool recursive)
        {
            return InternalIntrospect(type, recursive, null);
        }

        public static IList<FieldInfo> GetAllFieldsFrom(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type", "Type cannot be null.");

            return Fields.GetOrAdd(type, GetFieldInfo);
        }

        private static IList<FieldInfo> GetFieldInfo(Type type)
        {
            var reflectionService = DependencyContainer.Resolve<IReflectionService>();
            var result = reflectionService.GetFields(type);

            result = FilterFields(result).OrderBy(field => field.Name).ToList();
            return result;
        }

        /// <summary>
        ///   introspect a list of classes This method return the current meta model based on the classes that currently exist in the execution classpath.
        /// </summary>
        /// <remarks>
        ///   introspect a list of classes This method return the current meta model based on the classes that currently exist in the execution classpath. 
        ///   The result will be used to check meta model compatibility between the meta model that is currently persisted in the database and the meta 
        ///   model currently executing in JVM. This is used b the automatic meta model refactoring
        /// </remarks>
        /// <returns> A map where the key is the class name and the key is the ClassInfo: the class meta representation </returns>
        public static IDictionary<Type, ClassInfo> Instrospect(IEnumerable<ClassInfo> classInfos)
        {
            IDictionary<Type, ClassInfo> classInfoSet = new Dictionary<Type, ClassInfo>();

            foreach (var persistedClassInfo in classInfos)
            {
                var currentClassInfo = GetClassInfo(persistedClassInfo.FullClassName, persistedClassInfo);

                classInfoSet.Add(currentClassInfo.UnderlyingType, currentClassInfo);
            }

            return classInfoSet;
        }

        public static ClassInfoList Introspect(String fullClassName)
        {
            return Introspect(TypeResolutionUtils.ResolveType(fullClassName), true);
        }

        /// <summary>
        ///   Builds a class info from a class and an existing class info 
        ///   The existing class info is used to make sure that fields with the same name will have the same id
        /// </summary>
        /// <param name="fullClassName"> The name of the class to get info </param>
        /// <param name="existingClassInfo"> </param>
        /// <returns> A ClassInfo - a meta representation of the class </returns>
        private static ClassInfo GetClassInfo(String fullClassName, ClassInfo existingClassInfo)
        {
            var type = TypeResolutionUtils.ResolveType(fullClassName);
            var classInfo = new ClassInfo(type);

            var fields = GetAllFieldsFrom(type);
            var attributes = new OdbList<ClassAttributeInfo>(fields.Count);

            var maxAttributeId = existingClassInfo.MaxAttributeId;
            foreach (var fieldInfo in fields)
            {
                // Gets the attribute id from the existing class info
                var attributeId = existingClassInfo.GetAttributeId(fieldInfo.Name);
                if (attributeId == - 1)
                {
                    maxAttributeId++;
                    // The attribute with field.getName() does not exist in existing class info
                    //  create a new id
                    attributeId = maxAttributeId;
                }
                var fieldClassInfo = !OdbType.GetFromClass(fieldInfo.FieldType).IsNative()
                                         ? new ClassInfo(fieldInfo.FieldType)
                                         : null;

                attributes.Add(new ClassAttributeInfo(attributeId, fieldInfo.Name, fieldInfo.FieldType,
                                                      OdbClassNameResolver.GetFullName(fieldInfo.FieldType), fieldClassInfo));
            }

            classInfo.Attributes = attributes;
            classInfo.MaxAttributeId = maxAttributeId;

            return classInfo;
        }

        private static IEnumerable<FieldInfo> FilterFields(ICollection<FieldInfo> fields)
        {
            var fieldsToRemove = new OdbList<FieldInfo>(fields.Count);

            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.IsNotSerialized)
                    fieldsToRemove.Add(fieldInfo);
                else if (fieldInfo.FieldType == typeof (IntPtr))
                    fieldsToRemove.Add(fieldInfo);
                else if (fieldInfo.FieldType == typeof (UIntPtr))
                    fieldsToRemove.Add(fieldInfo);
                else if (fieldInfo.FieldType == typeof (void*))
                    fieldsToRemove.Add(fieldInfo);
                else if (fieldInfo.FieldType == typeof (Pointer))
                    fieldsToRemove.Add(fieldInfo);
                else if (fieldInfo.FieldType.FullName.StartsWith("System.Reflection.CerHashtable"))
                    fieldsToRemove.Add(fieldInfo);
                else if (fieldInfo.Name.StartsWith("this$"))
                    fieldsToRemove.Add(fieldInfo);
                else
                {
                    var oattr = fieldInfo.GetCustomAttributes(true);
                    var isNonPersistent = oattr.OfType<NonPersistentAttribute>().Any();

                    if (isNonPersistent)
                        fieldsToRemove.Add(fieldInfo);
                }
            }

            foreach (var item in fieldsToRemove)
                fields.Remove(item);

            return fields;
        }

        /// <param name="type"> The class to introspect </param>
        /// <param name="recursive"> If true, goes does the hierarchy to try to analyze all classes </param>
        /// <param name="classInfoList"> map with class name that are being introspected, to avoid recursive calls </param>
        private static ClassInfoList InternalIntrospect(Type type, bool recursive, ClassInfoList classInfoList)
        {
            if (classInfoList != null)
            {
                var existingClassInfo = classInfoList.GetClassInfoBy(type);

                if (existingClassInfo != null)
                    return classInfoList;
            }

            var classInfo = new ClassInfo(type);

            if (classInfoList == null)
                classInfoList = new ClassInfoList(classInfo);
            else
                classInfoList.AddClassInfo(classInfo);

            var fields = GetAllFieldsFrom(type);
            var attributes = new OdbList<ClassAttributeInfo>(fields.Count);

            for (var i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                ClassInfo classInfoByName;

                if (OdbType.GetFromClass(field.FieldType).IsNative())
                {
                    classInfoByName = null;
                }
                else
                {
                    if (recursive)
                    {
                        classInfoList = InternalIntrospect(field.FieldType, true, classInfoList);
                        classInfoByName = classInfoList.GetClassInfoBy(field.FieldType);
                    }
                    else
                        classInfoByName = new ClassInfo(field.FieldType);
                }

                attributes.Add(new ClassAttributeInfo((i + 1), field.Name, field.FieldType,
                                                      OdbClassNameResolver.GetFullName(field.FieldType), classInfoByName));
            }

            classInfo.Attributes = attributes;
            classInfo.MaxAttributeId = fields.Count;

            return classInfoList;
        }
    }
}