using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using ZeroMQ;

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
            //do
            //{
            //    tester.TestOnce();

            //} while (Console.ReadKey().Key == ConsoleKey.A);


            Console.CancelKeyPress += tester.Console_CancelKeyPress;
            int cnt = 0;
            while (++cnt <= 16)
                Task.Factory.StartNew(tester.TestTask);
            Task.Factory.StartNew(tester.Counter);

            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                //KeyValuePair<string, int>[] array;
                //lock (MemoryCheck.Alocs)
                //    array = MemoryCheck.Alocs.ToArray();
                //Console.Clear();
                //ZeroTrace.WriteInfo("Unmanage Money", MemoryCheck.AliveCount);
                //foreach (var kv in array)
                //{
                //    ZeroTrace.WriteInfo(kv.Key, kv.Value);
                //}
                //ZeroTrace.WriteInfo("Trace","End");
            }
        }
    }
}
