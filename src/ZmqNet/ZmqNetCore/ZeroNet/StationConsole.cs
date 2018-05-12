using System;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public static class StationConsole
    {
        /// <summary>
        /// 锁对象
        /// </summary>
        public static object lock_obj = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message)
        {
            lock (lock_obj)
            {
                //Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteError(string message)
        {
            lock (lock_obj)
            {
                //Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInfo(string message)
        {
            lock (lock_obj)
            {
                //Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}