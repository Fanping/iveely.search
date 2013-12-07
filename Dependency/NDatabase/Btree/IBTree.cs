using System;
using System.Collections;
using NDatabase.Api;

namespace NDatabase.Btree
{
    internal interface IBTree
    {
        void Insert(IComparable key, object value);

        void Split(IBTreeNode parent, IBTreeNode node2Split, int childIndex);

        object Delete(IComparable key, object value);

        int GetDegree();

        long GetSize();

        int GetHeight();

        IBTreeNode GetRoot();

        IBTreePersister GetPersister();

        void SetPersister(IBTreePersister persister);

        IBTreeNode BuildNode();

        object GetId();

        void SetId(object id);

        void Clear();

        IKeyAndValue GetBiggest(IBTreeNode node, bool delete);

        IKeyAndValue GetSmallest(IBTreeNode node, bool delete);

        IEnumerator Iterator<T>(OrderByConstants orderBy);
    }
}
