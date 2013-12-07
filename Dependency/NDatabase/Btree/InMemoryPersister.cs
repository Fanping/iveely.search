namespace NDatabase.Btree
{
    /// <summary>
    ///   In memory persister
    /// </summary>
    internal sealed class InMemoryPersister : IBTreePersister
    {
        #region IBTreePersister Members

        public IBTreeNode LoadNodeById(object id)
        {
            return null;
        }

        public void SaveNode(IBTreeNode node)
        {
        }
        
        public void Close()
        {
        }

        public void DeleteNode(IBTreeNode parent)
        {
        }

        public IBTree LoadBTree(object id)
        {
            return null;
        }

        public void SaveBTree(IBTree tree)
        {
        }

        public void SetBTree(IBTree tree)
        {
        }

        public void Flush()
        {
        }

        #endregion
    }
}
