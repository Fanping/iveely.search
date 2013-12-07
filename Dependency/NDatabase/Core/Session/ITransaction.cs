namespace NDatabase.Core.Session
{
    internal interface ITransaction
    {
        /// <summary>
        ///   clear the transaction
        /// </summary>
        void Clear();

        void Rollback();

        /// <summary>
        ///   Execute the commit process of the transaction
        /// </summary>
        void Commit();

        void SetFsiToApplyWriteActions(IFileSystemInterface fsi);

        /// <summary>
        ///   The public method to add a write action to the transaction.
        /// </summary>
        /// <remarks>
        ///   The public method to add a write action to the transaction. If first checks if the new write action action can be appended to the current write action. It is done by checking the currentWritePositioninWA. If yes (position==currentPositioninWA, just append the WA. If not, adds the current one to the transaction and creates a new one (as current)
        /// </remarks>
        /// <param name="position"> </param>
        /// <param name="bytes"> </param>
        void ManageWriteAction(long position, byte[] bytes);

        /// <returns> Returns the numberOfWriteActions. </returns>
        int GetNumberOfWriteActions();

        /// <summary>
        ///   Set the write position (position in main database file).
        /// </summary>
        /// <remarks>
        ///   Set the write position (position in main database file). This is used to know if the next write can be appended to the previous one (in the same current Write Action) or not.
        /// </remarks>
        /// <param name="position"> </param>
        void SetWritePosition(long position);

        /// <summary>
        ///   Reset the transaction
        /// </summary>
        void Reset();
    }
}
