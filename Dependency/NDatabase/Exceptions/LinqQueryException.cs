using System;

namespace NDatabase.Exceptions
{
    /// <summary>
    /// NDatabase exception raised during processing linq query
    /// </summary>
    public sealed class LinqQueryException : OdbRuntimeException
    {
        internal LinqQueryException(string message)
            : base(NDatabaseError.BtreeError.AddParameter(message))
        {
        }

        internal LinqQueryException(string message, Exception cause)
            : base(NDatabaseError.BtreeError.AddParameter(message), cause)
        {
        }
    }
}