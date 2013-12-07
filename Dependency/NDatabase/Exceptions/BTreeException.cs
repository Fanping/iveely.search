namespace NDatabase.Exceptions
{
    /// <summary>
    /// Exception raised when error in BTrees will appear
    /// </summary>
    public class BTreeException : OdbRuntimeException
    {
        internal BTreeException(string message)
            : base(NDatabaseError.BtreeError.AddParameter(message))
        {
        }
    }
}
