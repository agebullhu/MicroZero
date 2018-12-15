using System;
using System.Threading;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace Agebull.ZeroNet
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.WorkModel = ZeroWorkModel.Bridge;
            ZeroApplication.CheckOption();
            ZeroApplication.Initialize();
            BridgeService.Instance.Run();
            Console.WriteLine("enter q to close");
            while (Console.ReadKey().Key != ConsoleKey.Q)
                Console.WriteLine("enter q to close");

            BridgeService.Instance.State = StationState.Closing;
            while(BridgeService.Instance.State == StationState.Closing)
                Thread.Sleep(100);
            Console.WriteLine("bye");
        }
    }
}
