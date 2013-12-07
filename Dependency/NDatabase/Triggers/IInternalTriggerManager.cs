using System;
using NDatabase.Api;
using NDatabase.Api.Triggers;
using NDatabase.Meta;

namespace NDatabase.Triggers
{
    internal interface IInternalTriggerManager
    {
        void ManageInsertTriggerBefore(Type type, object @object);

        void ManageInsertTriggerAfter(Type type, object @object, OID oid);

        void ManageUpdateTriggerBefore(Type type, NonNativeObjectInfo oldObjectRepresentation, object newObject,
                                       OID oid);

        void ManageUpdateTriggerAfter(Type type, NonNativeObjectInfo oldObjectRepresentation, object newObject,
                                      OID oid);

        void ManageDeleteTriggerBefore(Type type, object @object, OID oid);

        void ManageDeleteTriggerAfter(Type type, object @object, OID oid);

        void ManageSelectTriggerAfter(Type type, object @object, OID oid);

        void AddUpdateTriggerFor(Type type, UpdateTrigger trigger);

        void AddInsertTriggerFor(Type type, InsertTrigger trigger);

        void AddDeleteTriggerFor(Type type, DeleteTrigger trigger);

        void AddSelectTriggerFor(Type type, SelectTrigger trigger);
    }
}
