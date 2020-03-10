using System;
using System.Linq;
using Agebull.Common.Logging;

namespace Agebull.MicroZero
{
    /// <summary>
    ///   控制台扩展
    /// </summary>
    public static class ZeroTrace
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void SystemLog(string title, params object[] messages)
        {
            if (messages.Length == 0)
                LogRecorder.SystemLog(title);
            else
                LogRecorder.SystemLog("{0} : {1}", title, messages.LinkToString(" $ "));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteError(string title, params object[] messages)
        {
            LogRecorder.Error("{0} : {1}", title, messages.LinkToString(" * "));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        /// <param name="exception"></param>
        public static void WriteException(string title, Exception exception, params object[] messages)
        {
            LogRecorder.Exception(exception, "{0} : {1}", title, messages.LinkToString(" * "));
        }
    }
}