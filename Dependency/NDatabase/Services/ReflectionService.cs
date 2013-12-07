using System;
using System.Collections.Generic;
using System.Reflection;

namespace NDatabase.Services
{
    internal class ReflectionService : IReflectionService
    {
        public IList<MemberInfo> GetFieldsAndProperties(Type type)
        {
            var result = new List<MemberInfo>();

            result.AddRange(GetFields(type));
            result.AddRange(GetProperties(type));

            return result;
        }


        public IList<FieldInfo> GetFields(Type type)
        {
            const int capacity = 50;

            var attributesNames = new List<string>(capacity);
            var result = new List<FieldInfo>(capacity);

            var classes = GetAllClasses(type);

            foreach (var class1 in classes)
            {
                var baseClassfields =
                    class1.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                     BindingFlags.DeclaredOnly);

                foreach (var fieldInfo in baseClassfields)
                {
                    if (attributesNames.Contains(fieldInfo.Name))
                        continue;

                    result.Add(fieldInfo);
                    attributesNames.Add(fieldInfo.Name);
                }
            }

            return result;
        }

        public IList<PropertyInfo> GetProperties(Type type)
        {
            const int capacity = 50;

            var attributesNames = new List<string>(capacity);
            var result = new List<PropertyInfo>(capacity);

            var classes = GetAllClasses(type);

            foreach (var class1 in classes)
            {
                var baseProperties =
                    class1.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                         BindingFlags.DeclaredOnly);

                foreach (var propertyInfo in baseProperties)
                {
                    if (attributesNames.Contains(propertyInfo.Name))
                        continue;

                    result.Add(propertyInfo);
                    attributesNames.Add(propertyInfo.Name);
                }
            }

            return result;
        }

        private IEnumerable<Type> GetAllClasses(Type type)
        {
            var result = new List<Type> {type};

            var baseType = type.BaseType;

            while (baseType != null && baseType != typeof (Object))
            {
                result.Add(baseType);
                baseType = baseType.BaseType;
            }

            return result;
        }
    }
}