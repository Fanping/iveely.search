/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using log4net;

namespace Iveely.Framework.Log
{
    public class Logger
    {
        //private static ILog _log = LogManager.GetLogger("IveelySE");

        //public static void CreateLogger(string name)
        //{
        //    _log = LogManager.GetLogger(name);
        //}

        public static void Debug(object message)
        {
            LogHelper.Debug(message);
        }

        public static void Debug(object message, Exception exception)
        {
            LogHelper.Debug(message, exception);
        }

        public static void Info(object message)
        {
            LogHelper.Info(message);
        }

        public static void Info(object message, Exception exception)
        {
            LogHelper.Info(message, exception);
        }

        public static void Error(object message)
        {
            LogHelper.Error(message);
        }

        public static void Error(object message, Exception exception)
        {
            LogHelper.Error(message, exception);
        }

        public static void Warn(object message)
        {
            LogHelper.Warn(message);
        }

        public static void Warn(object message, Exception exception)
        {
            LogHelper.Warn(message, exception);
        }
    }
}
