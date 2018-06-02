using System;
using System.Threading;
using Agebull.Common;

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

        private static readonly SyncQueue<ConsoleMessage> ConsoleMessages = new SyncQueue<ConsoleMessage>();


        internal static void Initialize()
        {
            Console.CursorVisible = false;
            new Thread(Show)
            {
                IsBackground=true,
                Priority = ThreadPriority.AboveNormal
            }.Start();
        }

        static void Show()
        {
            while (ZeroApplication.ApplicationState < StationState.Destroy)
            {
                if (!ConsoleMessages.StartProcess(out var message))
                    continue;
                if (message.Title == null)
                    message.Title = "???";
                int childNewLine = 1;
                switch (message.Type)
                {
                    case 0:
                        Console.WriteLine();
                        Console.Write(message.Title);
                        break;
                    case 1:
                        if (Console.CursorLeft != 0)
                            Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"[{message.Title}] ");
                        Console.ForegroundColor = ConsoleColor.White;
                        childNewLine = 0;
                        break;
                    case 2:
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"[{message.Title}] ");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case 3:
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"[{message.Title}] ");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                if (ZeroApplication.ApplicationState == StationState.Destroy)
                    return;
                if (message.Messages != null && message.Messages.Length > 0)
                {
                    foreach (var line in message.Messages)
                    {
                        if (ZeroApplication.ApplicationState == StationState.Destroy)
                            return;
                        if (childNewLine ==2)
                            Console.WriteLine();
                        else if (childNewLine == 1)
                            childNewLine = 2;
                        Console.CursorLeft = message.Title.Length + 3;
                        Console.Write(line);
                    }
                }

                if (ZeroApplication.ApplicationState == StationState.Destroy)
                    return;
                if (message.Type == 1)
                    Console.CursorLeft = 0;
                ConsoleMessages.EndProcess();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message)
        {
            ConsoleMessages.Push(new ConsoleMessage { Title = message });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteLoop(string title, params object[] messages)
        {
            ConsoleMessages.Push(new ConsoleMessage
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
            ConsoleMessages.Push(new ConsoleMessage
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
            ConsoleMessages.Push(new ConsoleMessage
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
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void WriteException(string title, Exception exception, string message)
        {
            ConsoleMessages.Push(new ConsoleMessage
            {
                Type = 3,
                Title = title,
                Messages = new object[] { "Exception", message, exception }
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
            ConsoleMessages.Push(new ConsoleMessage
            {
                Type = 3,
                Title = title,
                Messages = new object[] { "Exception", message, exception }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="exception"></param>
        public static void WriteException(string title, Exception exception)
        {
            ConsoleMessages.Push(new ConsoleMessage
            {
                Type = 3,
                Title = title,
                Messages = new object[] { "Exception", exception }
            });
        }



    }
}