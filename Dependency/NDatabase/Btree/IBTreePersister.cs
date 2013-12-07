namespace NDatabase.Btree
{
    /// <summary>
    ///   Interface used to persist and load btree and btree node from a persistent layer
    /// </summary>
    internal interface IBTreePersister
    {
        IBTreeNode LoadNodeById(object id);

        void SaveNode(IBTreeNode node);

        void SaveBTree(IBTree tree);

        IBTree LoadBTree(object id);
        
        void Close();

        void DeleteNode(IBTreeNode parent);

        void SetBTree(IBTree tree);

        void Flush();
    }
}