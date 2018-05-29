using System;
using System.Threading;
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
        private static readonly object LockObj = new object();

        private static bool newLine = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message)
        {
            Task.Factory.StartNew(() =>
            {
                if(Monitor.TryEnter(LockObj, 1000))
                {
                    if (Console.CursorLeft != 0)
                        Console.WriteLine();
                    Console.WriteLine(message);
                    newLine = true;
                    Monitor.Exit(LockObj);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteError(string title, params object[] messages)
        {
            Task.Factory.StartNew(() =>
            {
                if(Monitor.TryEnter(LockObj, 1000))
                {
                    if (Console.CursorLeft != 0)
                        Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"[{title}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (var message in messages)
                    {
                        Console.CursorLeft = title.Length + 3;
                        Console.WriteLine(message);
                    }
                    newLine = true;
                    Monitor.Exit(LockObj);
                }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void WriteException(string title, Exception exception, string message)
        {
            Task.Factory.StartNew(() =>
            {
                if(Monitor.TryEnter(LockObj, 1000))
                {
                    if (Console.CursorLeft != 0)
                        Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"[{title}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(message);
                    Console.CursorLeft = title.Length + 3;
                    Console.WriteLine(exception);
                    Monitor.Exit(LockObj);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void WriteException(string title, string message, Exception exception)
        {
            Task.Factory.StartNew(() =>
            {
                if(Monitor.TryEnter(LockObj, 1000))
                {
                    if (Console.CursorLeft != 0)
                        Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"[{title}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(message);
                    Console.CursorLeft = title.Length + 3;
                    Console.WriteLine(exception);
                    newLine = true;
                    Monitor.Exit(LockObj);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="exception"></param>
        public static void WriteException(string title, Exception exception)
        {
            Task.Factory.StartNew(() =>
            {
                if(Monitor.TryEnter(LockObj, 1000))
                {
                    if (Console.CursorLeft != 0)
                        Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"[{title}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("exception.Message");
                    Console.CursorLeft = title.Length + 3;
                    Console.WriteLine(exception);
                    newLine = true;
                    Monitor.Exit(LockObj);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteInfo(string title, params string[] messages)
        {
            Task.Factory.StartNew(() =>
            {
                if(Monitor.TryEnter(LockObj, 1000))
                {
                    if (Console.CursorLeft != 0)
                        Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"[{title}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (var message in messages)
                    {
                        Console.CursorLeft = title.Length + 3;
                        Console.WriteLine(message);
                    }
                    newLine = true;
                    Monitor.Exit(LockObj);
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public static void WriteLoop(string title, string message)
        {
            Task.Factory.StartNew(() =>
            {
                if(Monitor.TryEnter(LockObj, 1000))
                {
                    if (Console.CursorLeft != 0 || newLine)
                    {
                        Console.WriteLine();
                        newLine = false;
                    }
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"[{title}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(message);
                    Console.CursorLeft = 0;
                    Monitor.Exit(LockObj);
                }
            });
        }
    }
}