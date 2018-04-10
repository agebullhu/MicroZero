using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.LogRecorder;
using Agebull.ZeroNet.LogService;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            //LogRecorder.Initialize(new RemoteRecorder());
            StationProgram.WriteLine("Hello ZeroNet");
            StationProgram.Config.DataFolder = @"c:\data";
            StationProgram.RegisteStation(new RemoteLogRecorder());
            StationProgram.Initialize();
            for (int i = 0; i < 4; i++)
            {
                Task.Factory.StartNew(TestTask);
            }
            StationProgram.RunConsole();


            ConsoleKeyInfo key;
            do
            {
                //LogRecorder.BeginMonitor("test");
                //LogRecorder.BeginStepMonitor("f");
                //LogRecorder.EndStepMonitor();
                //LogRecorder.EndMonitor();
                key = Console.ReadKey();
            } while (key.Modifiers != ConsoleModifiers.Control || key.Key != ConsoleKey.C);
        }


        static void TestTask()
        {
            StationProgram.WriteLine($"Test{Task.CurrentId}");
            Random random = new Random((int)DateTime.Now.Ticks % int.MaxValue);
            var recorder=new RemoteRecorder();
            while (true)
            {
                Thread.Sleep(1);//random.Next(1, 5)
                recorder.RecordLog(new RecordInfo
                {
                    gID = Guid.NewGuid(),
                    Type = LogType.Trace,
                    TypeName="Trace",
                    Message= $"Test{Task.CurrentId}"
                });
            }
        }
    }
}
