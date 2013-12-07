using System;
using NDatabase.Api.Triggers;

namespace NDatabase.Triggers
{
    internal sealed class TriggerManager<T> : ITriggerManager where T : class
    {
        private readonly ITriggersEngine _storageEngine;
        private readonly Type _underlyingType;

        public TriggerManager(ITriggersEngine storageEngine)
        {
            _storageEngine = storageEngine;
            _underlyingType = typeof (T);
        }

        #region ITriggerManager Members

        public void AddUpdateTrigger(UpdateTrigger trigger)
        {
            _storageEngine.AddUpdateTriggerFor(_underlyingType, trigger);
        }

        public void AddInsertTrigger(InsertTrigger trigger)
        {
            _storageEngine.AddInsertTriggerFor(_underlyingType, trigger);
        }

        public void AddDeleteTrigger(DeleteTrigger trigger)
        {
            _storageEngine.AddDeleteTriggerFor(_underlyingType, trigger);
        }

        public void AddSelectTrigger(SelectTrigger trigger)
        {
            _storageEngine.AddSelectTriggerFor(_underlyingType, trigger);
        }

        #endregion
    }
}
