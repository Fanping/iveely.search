using System;

namespace log4net
{
    /// <summary>
    /// Log helper
    /// </summary>
    public class LogHelper
    {
        private static ILog
            log = LogManager.GetLogger("IveelySE");

        public static void CreateLogger(string name)
        {
            log = LogManager.GetLogger(name);
        }

        public static void Debug(object message)
        {
            log.Debug(message);
        }

        public static void Debug(object message, Exception exception)
        {
            log.Debug(message, exception);
        }

        public static void Info(object message)
        {
            log.Info(message);
        }

        public static void Info(object message, Exception exception)
        {
            log.Info(message, exception);
        }

        public static void Error(object message)
        {
            log.Error(message);
        }

        public static void Error(object message, Exception exception)
        {
            log.Error(message, exception);
        }

        public static void Warn(object message)
        {
            log.Warn(message);
        }

        public static void Warn(object message, Exception exception)
        {
            log.Warn(message, exception);
        }
    }
}
