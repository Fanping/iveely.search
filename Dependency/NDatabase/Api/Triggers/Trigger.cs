namespace NDatabase.Api.Triggers
{
    /// <summary>
    ///   A simple base class for all triggers.
    /// </summary>
    public abstract class Trigger
    {
        /// <summary>
        /// Access to NDatabase interface connected with created trigger
        /// </summary>
        public IOdbForTrigger Odb { get; internal set; }
    }
}
