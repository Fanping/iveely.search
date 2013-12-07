using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NDatabase.Container;
using NDatabase.Services;
using NDatabase.Tool;

namespace NDatabase.Api.Triggers
{
    internal sealed class EnrichWithOidTrigger : SelectTrigger
    {
        private static readonly Dictionary<Type, FieldInfo> OidFields = new Dictionary<Type, FieldInfo>();
        
        private readonly IReflectionService _reflectionService;

        public EnrichWithOidTrigger()
        {
            _reflectionService = DependencyContainer.Resolve<IReflectionService>();
        }

        public override void AfterSelect(object @object, OID oid)
        {
            var type = @object.GetType();
            var oidField = OidFields.GetOrAdd(type, SearchOidSupportableField);

            if (oidField == null)
                return;

            if (oidField.FieldType == typeof (OID))
                oidField.SetValue(@object, oid);
            else
                oidField.SetValue(@object, oid.ObjectId);
        }

        private FieldInfo SearchOidSupportableField(Type type)
        {
            var fields = _reflectionService.GetFields(type);

            return (from fieldInfo in fields
                    let attributes = fieldInfo.GetCustomAttributes(true)
                    let hasAttribute = attributes.OfType<OIDAttribute>().Any()
                    let isOidSupportedType = fieldInfo.FieldType == typeof (OID) || fieldInfo.FieldType == typeof (long)
                    where hasAttribute && isOidSupportedType
                    select fieldInfo).FirstOrDefault();
        }
    }
}