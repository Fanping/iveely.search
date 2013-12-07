using System;
using NDatabase.Api.Triggers;

namespace NDatabase.Triggers
{
    internal interface ITriggersEngine
    {
        void AddUpdateTriggerFor(Type type, UpdateTrigger trigger);
        void AddInsertTriggerFor(Type type, InsertTrigger trigger);
        void AddDeleteTriggerFor(Type type, DeleteTrigger trigger);
        void AddSelectTriggerFor(Type type, SelectTrigger trigger);
    }
}