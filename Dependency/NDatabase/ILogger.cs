namespace NDatabase
{
    /// <summary>
    /// Base interface for creacting custom logger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log message with warn level
        /// </summary>
        /// <param name="message">Mssage to log</param>
        void Warning(string message);

        /// <summary>
        /// Log message with debug level
        /// </summary>
        /// <param name="message">Mssage to log</param>
        void Debug(string message);

        /// <summary>
        /// Log message with info level
        /// </summary>
        /// <param name="message">Mssage to log</param>
        void Info(string message);

        /// <summary>
        /// Log message with error level
        /// </summary>
        /// <param name="message">Mssage to log</param>
        void Error(string message);
    }
}