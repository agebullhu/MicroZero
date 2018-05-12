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
using Newtonsoft.Json;

namespace RpcTest
{
    internal class Program
    {
        //private static Recorder recorder;
        private static void Main(string[] args)
        {
            StationConsole.WriteLine("Hello ZeroNet");
            StationProgram.Config.DataFolder = @"c:\data";
            //StationProgram.RegisteStation(new RemoteLogStation());
            LogRecorder.Initialize(new RemoteRecorder());
            LogRecorder.LogByTask = true;
            LogRecorder.TraceToConsole = false;
            StationProgram.Launch();

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
            //recorder.Shutdown();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            LogRecorder.Shutdown();
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
                if (count - RemoteRecorder.DataCount > 9999)
                    Thread.Sleep(10);
                LogRecorder.Message($"Test{ Task.CurrentId}:{ random.Next()}");

                if (++count == long.MaxValue)
                    count = 0;
                if (count % 10000 != 0) continue;
                lock (lo)
                {
                    var tm = (DateTime.Now - start).TotalSeconds;
                    Console.CursorLeft = 0;
                    Console.CursorTop = row;
                    Console.Write($"{count- RemoteRecorder.DataCount:D8}| Sub:{RemoteLogStation.RecorderCount:D8}({(int)(RemoteLogStation.RecorderCount / tm):D5}/s)  Pub:{RemoteRecorder.PubCount:D8}({(int)(RemoteRecorder.PubCount / tm):D5}/s)   Pub:{RemoteRecorder.DataCount:D8}({(int)(RemoteRecorder.DataCount / tm):D5}/s)");
                }
            }
        }
    }
}
