using System;
using System.Collections.Generic;
using NDatabase.Meta;

namespace NDatabase.Services
{
    internal interface IMetaModelCompabilityChecker
    {
        /// <summary>
        ///     Receive the current class info (loaded from current classes present on runtime and check against the persisted meta model
        /// </summary>
        bool Check(IDictionary<Type, ClassInfo> currentCIs, IMetaModelService metaModelService);
    }
}