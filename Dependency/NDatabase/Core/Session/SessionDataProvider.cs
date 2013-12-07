using System;
using NDatabase.Meta;
using NDatabase.Meta.Introspector;

namespace NDatabase.Core.Session
{
    internal sealed class SessionDataProvider : IObjectIntrospectionDataProvider
    {
        private ISession _session;

        public SessionDataProvider(ISession session)
        {
            _session = session;
        }

        public ClassInfo GetClassInfo(Type type)
        {
            var odbType = OdbType.GetFromClass(type);
            if (odbType.IsNative() && !odbType.IsEnum())
                return null;

            var metaModel = _session.GetMetaModel();
            if (metaModel.ExistClass(type))
                return metaModel.GetClassInfo(type, true);

            var classInfoList = ClassIntrospector.Introspect(type, true);

            _session.GetObjectWriter().AddClasses(classInfoList);

            return classInfoList.GetMainClassInfo();
        }

        public void Clear()
        {
            _session = null;
        }

        public NonNativeObjectInfo EnrichWithOid(NonNativeObjectInfo nnoi, object o)
        {
            var cache = _session.GetCache();

            var oid = cache.GetOid(o);
            if (oid != null)
            {
                nnoi.SetOid(oid);
                // Sets some values to the new header to keep track of the infos when
                // executing NDatabase without closing it, just committing.
                var objectInfoHeader = cache.GetObjectInfoHeaderByOid(oid, true);
                nnoi.GetHeader().SetObjectVersion(objectInfoHeader.GetObjectVersion());
                nnoi.GetHeader().SetUpdateDate(objectInfoHeader.GetUpdateDate());
                nnoi.GetHeader().SetCreationDate(objectInfoHeader.GetCreationDate());
            }

            return nnoi;
        }
    }
}