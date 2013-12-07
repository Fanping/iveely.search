namespace NDatabase.Meta
{
    /// <summary>
    ///   Used for committed zone info.
    /// </summary>
    /// <remarks>
    ///   Used for committed zone info. It has one more attribute than the super class. It is used to keep track of committed deleted objects
    /// </remarks>
    internal sealed class CommittedCIZoneInfo : CIZoneInfo
    {
        private long _nbDeletedObjects;

        internal CommittedCIZoneInfo()
        {
            _nbDeletedObjects = 0;
        }

        internal override void DecreaseNbObjects()
        {
            _nbDeletedObjects++;
        }

        internal long GetNbDeletedObjects()
        {
            return _nbDeletedObjects;
        }

        internal void SetNbDeletedObjects(long nbDeletedObjects)
        {
            _nbDeletedObjects = nbDeletedObjects;
        }

        internal override long GetNumberbOfObjects()
        {
            return NbObjects - _nbDeletedObjects;
        }

        internal override void SetNbObjects(long nb)
        {
            NbObjects = nb;
            _nbDeletedObjects = 0;
        }

        internal void SetNbObjects(CommittedCIZoneInfo cizi)
        {
            NbObjects = cizi.NbObjects;
            _nbDeletedObjects = cizi._nbDeletedObjects;
        }

        public override string ToString()
        {
            return string.Concat("(first=" + First, ",last=" + Last, ",nb=", NbObjects.ToString(), "-",
                                 _nbDeletedObjects.ToString(), ")");
        }
    }
}
