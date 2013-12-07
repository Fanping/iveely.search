using System;

namespace NDatabase.Btree
{
    internal interface IBTreeSingleValuePerKey : IBTree
    {
        object Search(IComparable key);
    }
}