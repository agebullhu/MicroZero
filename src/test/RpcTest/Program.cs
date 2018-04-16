using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.LogRecorder;
using Agebull.ZeroNet.LogService;
using Agebull.ZeroNet.PubSub;
using NetMQ;
using NetMQ.Sockets;

namespace RpcTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            LogRecorder.Initialize(new RemoteRecorder());
            StationProgram.WriteLine("Hello ZeroNet");
            StationProgram.Config.DataFolder = @"c:\data";
            StationProgram.RegisteStation(new RemoteLogStation());
            StationProgram.Initialize();
            StationProgram.Run();

            Thread.Sleep(1000);
            start = DateTime.Now;
            count = 0;
            row = Console.CursorTop;
            for (int i = 0; i < 4; i++)
            {
                Task.Factory.StartNew(TestTask);
                //TestTask();
            }
            Console.TreatControlCAsInput = true;
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            } while (key.Modifiers != ConsoleModifiers.Control || key.Key != ConsoleKey.C);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            StationProgram.Exit();
        }

        private static long count = 0;
        static DateTime start;
        static int row;
        static object lo = new object();
        static void TestTask()
        {
            Random random = new Random((int)DateTime.Now.Ticks % int.MaxValue);

            while (StationProgram.State < StationState.Closing)
            {
                if (count - RemoteRecorder.PubCount > 999)
                    Thread.Sleep(1);
                LogRecorder.RecordMessage("Test", $"Test{ Task.CurrentId}:{ random.Next()}");

                if (++count == long.MaxValue)
                    count = 0;
                if (count % 10000 != 0) continue;
                lock (lo)
                {
                    var tm = (DateTime.Now - start).TotalSeconds;
                    Console.CursorLeft = 0;
                    Console.CursorTop = row;
                    Console.Write($"{count- RemoteRecorder.PubCount:D8}| Sub:{RemoteLogStation.RecorderCount:D8}({(int)(RemoteLogStation.RecorderCount / tm):D5}/s)  Pub:{RemoteRecorder.PubCount:D8}({(int)(RemoteRecorder.PubCount / tm):D5}/s)                 ");
                }
            }
        }
    }
}
