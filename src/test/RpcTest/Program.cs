using System;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    internal class Program
    {
        //private static Recorder recorder;
        private static void Main(string[] args)
        {
            ZeroApplication.Initialize();
            ZeroApplication.RunBySuccess();
            Console.CursorVisible = false;
            var tester = new ZeroTester();
            //var tester = new HtttpTester();
            do
            {
                tester.TestOnce();

            } while (Console.ReadKey().Key == ConsoleKey.A);


            Console.CancelKeyPress += tester.Console_CancelKeyPress;
            int cnt = 0;
            while (++cnt <= 4)
                Task.Factory.StartNew(tester.TestTask);
            Task.Factory.StartNew(tester.Counter).Wait();
            LogRecorder.Shutdown();
        }
    }
}
