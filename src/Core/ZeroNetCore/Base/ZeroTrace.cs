using System;
using System.Linq;
using Agebull.Common.Logging;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///   控制台扩展
    /// </summary>
    public static class ZeroTrace
    {
        class ConsoleMessage
        {
            public string Title { get; set; }

            public object[] Messages { get; set; }

            public int Type { get; set; }
        }

        //private static readonly SyncQueue<ConsoleMessage> ConsoleMessages = new SyncQueue<ConsoleMessage>();

        //private static Thread thread;

        internal static void Initialize()
        {
            //Console.CursorVisible = false;
            //thread = new Thread(Show)
            //{
            //    IsBackground = true,
            //    Priority = ThreadPriority.AboveNormal
            //};
            //thread.Start();
        }

        //internal static void Shutdown()
        //{
        //    //thread?.Abort();
        //}
        //static void Show()
        //{
        //    while (ZeroApplication.IsAlive)
        //    {
        //        if (!ConsoleMessages.StartProcess(out var message))
        //            continue;
        //        Write(message);
        //        ConsoleMessages.EndProcess();
        //    }
        //    Console.WriteLine();
        //    thread = null;
        //}
        static object lock_obj = new object();
        private static void Write(ConsoleMessage message)
        {
            lock (lock_obj)
                switch (message.Type)
                {
                    case 0:
                        Console.WriteLine(message.Title);
                        break;
                    default:
                        Console.WriteLine(message.Messages.LinkToString($"[{message.Title}]  ", " > "));
                        break;
                }
            //if (message.Title == null)
            //    message.Title = "???";
            //int childNewLine = 1;
            //switch (message.Type)
            //{
            //    case 0:
            //        Console.WriteLine();
            //        Console.Write(message.Title);
            //        break;
            //    case 1:
            //        if (Console.CursorLeft != 0)
            //            Console.WriteLine();
            //        Console.ForegroundColor = ConsoleColor.Blue;
            //        Console.Write($"[{message.Title}] ");
            //        Console.ForegroundColor = ConsoleColor.White;
            //        childNewLine = 0;
            //        break;
            //    case 2:
            //        Console.WriteLine();
            //        Console.ForegroundColor = ConsoleColor.Green;
            //        Console.Write($"[{message.Title}] ");
            //        Console.ForegroundColor = ConsoleColor.White;
            //        break;
            //    case 3:
            //        Console.WriteLine();
            //        Console.ForegroundColor = ConsoleColor.Red;
            //        Console.Write($"[{message.Title}] ");
            //        Console.ForegroundColor = ConsoleColor.White;
            //        break;
            //}

            //if (message.Messages != null && message.Messages.Length > 0)
            //{
            //    foreach (var line in message.Messages)
            //    {
            //        if (childNewLine == 1)
            //            childNewLine = 2;
            //        else if (childNewLine == 2)
            //        {
            //            Console.ForegroundColor = ConsoleColor.DarkGreen;
            //            Console.Write(" > ");
            //            Console.ForegroundColor = ConsoleColor.White;
            //        }

            //        Console.Write(line);
            //    }
            //}

            //if (message.Type == 1)
            //    Console.CursorLeft = 0;
        }

        //private static void ShowTrace(ConsoleMessage message)
        //{
        //    switch (message.Type)
        //    {
        //        case 0:
        //            LogRecorder.Trace(message.Title);
        //            break;
        //        default:
        //            LogRecorder.Trace(message.Messages.LinkToString(message.Title, " > "));
        //            break;
        //    }
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        static void Push(ConsoleMessage message)
        {
            if (ZeroApplication.IsDisposed)
                return;
            //if (ZeroApplication.IsAlive)
            //    ConsoleMessages.Push(message);
            //else
            {
                Write(message);
            }

            //ShowTrace(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message)
        {
            Push(new ConsoleMessage { Title = message });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteLoop(string title, params object[] messages)
        {
            Push(new ConsoleMessage
            {
                Type = 1,
                Title = title,
                Messages = messages
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteInfo(string title, params object[] messages)
        {
            Push(new ConsoleMessage
            {
                Type = 2,
                Title = title,
                Messages = messages
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteError(string title, params object[] messages)
        {
            LogRecorder.Error("{0} : {1}", title, messages.LinkToString(" > "));
            Push(new ConsoleMessage
            {
                Type = 3,
                Title = title,
                Messages = messages
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        /// <param name="exception"></param>
        public static void WriteException(string title, Exception exception, params object[] messages)
        {
            var msgs = messages.ToList();
            msgs.Insert(0, "Exception");
            msgs.Add("\r\n");
            msgs.Add(exception);
            LogRecorder.Exception(exception, "{0} : {1}", title, messages.LinkToString(" > "));
            Push(new ConsoleMessage
            {
                Type = 3,
                Title = title,
                Messages = msgs.ToArray()
            });
        }

    }
}