using System;

namespace NDatabase.Btree
{
    internal interface IKeyAndValue
    {
        IComparable GetKey();

        object GetValue();
    }
}