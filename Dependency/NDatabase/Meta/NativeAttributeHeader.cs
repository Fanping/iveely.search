namespace NDatabase.Meta
{
    /// <summary>
    ///   A class that contain basic information about a native object
    /// </summary>
    internal sealed class NativeAttributeHeader
    {
        private bool _isNull;
        private int _odbTypeId;

        public bool IsNull()
        {
            return _isNull;
        }

        public void SetNull(bool isNull)
        {
            _isNull = isNull;
        }

        public int GetOdbTypeId()
        {
            return _odbTypeId;
        }

        public void SetOdbTypeId(int odbTypeId)
        {
            _odbTypeId = odbTypeId;
        }
    }
}
