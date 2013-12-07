namespace NDatabase.Exceptions
{
    /// <summary>
    /// Exception raised when error in BTrees will appear (validation error)
    /// </summary>
    public sealed class BTreeNodeValidationException : OdbRuntimeException
    {
        internal BTreeNodeValidationException(string message, System.Exception cause)
            : base(NDatabaseError.BtreeValidationError.AddParameter(message), cause)
        {
        }

        internal BTreeNodeValidationException(string message)
            : base(NDatabaseError.BtreeValidationError.AddParameter(message))
        {
        }
    }
}