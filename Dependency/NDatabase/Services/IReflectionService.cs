using System;
using System.Collections.Generic;
using System.Reflection;

namespace NDatabase.Services
{
    internal interface IReflectionService
    {
        IList<FieldInfo> GetFields(Type type);
        IList<PropertyInfo> GetProperties(Type type);
        IList<MemberInfo> GetFieldsAndProperties(Type type);
    }
}