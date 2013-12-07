using NDatabase.Api;

namespace NDatabase.Meta
{
    /// <summary>
    ///   Class keep track of object pointers and number of objects of a class info for a specific zone. 
    ///   For example, to keep track of first committed and last committed object position.
    /// </summary>
    internal class CIZoneInfo
    {
        protected long NbObjects;

        internal CIZoneInfo()
        {
            First = null;
            Last = null;
            NbObjects = 0;
        }

        internal OID First { get; set; }
        internal OID Last { get; set; }

        public override string ToString()
        {
            var nbObjects = NbObjects.ToString();
            return string.Format("(first={0},last={1},nb={2})", First, Last, nbObjects);
        }

        internal void Reset()
        {
            First = null;
            Last = null;
            NbObjects = 0;
        }

        internal void SetBasedOn(CIZoneInfo zoneInfo)
        {
            NbObjects = zoneInfo.NbObjects;
            First = zoneInfo.First;
            Last = zoneInfo.Last;
        }

        internal virtual void DecreaseNbObjects()
        {
            if (NbObjects == 0)
                return;

            NbObjects--;
        }

        internal void IncreaseNbObjects()
        {
            NbObjects++;
        }

        internal virtual long GetNumberbOfObjects()
        {
            return NbObjects;
        }

        internal bool HasObjects()
        {
            return NbObjects != 0;
        }

        internal virtual void SetNbObjects(long nb)
        {
            NbObjects = nb;
        }
    }
}
