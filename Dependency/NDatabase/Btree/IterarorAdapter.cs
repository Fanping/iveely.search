using System.Collections;

namespace NDatabase.Btree
{
    internal abstract class IterarorAdapter : IEnumerator
    {
        #region IEnumerator Members

        public object Current
        {
            get { return GetCurrent(); }
        }

        public abstract bool MoveNext();
        public abstract void Reset();

        #endregion

        protected abstract object GetCurrent();
    }
}