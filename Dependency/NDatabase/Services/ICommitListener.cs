namespace NDatabase.Services
{
    internal interface ICommitListener
    {
        void BeforeCommit();

        void AfterCommit();
    }
}