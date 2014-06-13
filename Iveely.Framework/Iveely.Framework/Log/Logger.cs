/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using log4net;

namespace Iveely.Framework.Log
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// 输出调试
        /// </summary>
        /// <param name="message">调试消息</param>
        public static void Debug(object message)
        {
            LogHelper.Debug(message);
        }

        /// <summary>
        /// 输出调试
        /// </summary>
        /// <param name="message">调试消息</param>
        /// <param name="exception">调试异常</param>
        public static void Debug(object message, Exception exception)
        {
            LogHelper.Debug(message, exception);
        }

        /// <summary>
        /// 输出信息
        /// </summary>
        /// <param name="message">信息消息</param>
        public static void Info(object message)
        {
            LogHelper.Info(message);
        }

        /// <summary>
        /// 输出信息
        /// </summary>
        /// <param name="message">信息消息</param>
        /// <param name="exception">异常信息</param>
        public static void Info(object message, Exception exception)
        {
            LogHelper.Info(message, exception);
        }

        /// <summary>
        /// 输出错误
        /// </summary>
        /// <param name="message">错误消息</param>
        public static void Error(object message)
        {
            LogHelper.Error(message);
        }

        /// <summary>
        /// 输出错误
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="exception">错误异常</param>
        public static void Error(object message, Exception exception)
        {
            LogHelper.Error(message, exception);
        }

        /// <summary>
        /// 输出警告
        /// </summary>
        /// <param name="message">警告消息</param>
        public static void Warn(object message)
        {
            LogHelper.Warn(message);
        }

        /// <summary>
        /// 输出警告
        /// </summary>
        /// <param name="message">警告消息</param>
        /// <param name="exception">警告异常</param>
        public static void Warn(object message, Exception exception)
        {
            LogHelper.Warn(message, exception);
        }
    }
}
