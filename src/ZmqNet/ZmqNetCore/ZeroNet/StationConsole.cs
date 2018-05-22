using System;
using System.Threading.Tasks;

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
            Task.Factory.StartNew(() =>
            {
                lock (lock_obj)
                {
                    //Console.CursorLeft = 0;
                    Console.WriteLine(message);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteError(string message)
        {
            Task.Factory.StartNew(() =>
            {
                lock (lock_obj)
                {
                    //Console.CursorLeft = 0;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInfo(string message)
        {
            Task.Factory.StartNew(() =>
            {
                lock (lock_obj)
                {
                    //Console.CursorLeft = 0;
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            });
        }

    }
}