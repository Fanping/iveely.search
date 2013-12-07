using NDatabase.Meta;

namespace NDatabase.Core.Query
{
    internal interface IQueryExecutionPlan
    {
        bool UseIndex();

        ClassInfoIndex GetIndex();

        string GetDetails();

        void Start();

        void End();
    }
}
