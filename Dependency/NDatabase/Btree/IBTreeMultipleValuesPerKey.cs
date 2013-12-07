using System;
using System.Collections;

namespace NDatabase.Btree
{
    internal interface IBTreeMultipleValuesPerKey : IBTree
    {
        IList Search(IComparable key);
    }
}