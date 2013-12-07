using System.Collections.Generic;
using NDatabase.Meta;

namespace NDatabase.Services
{
    internal interface IMetaModelService
    {
        IEnumerable<ClassInfo> GetAllClasses();
    }
}