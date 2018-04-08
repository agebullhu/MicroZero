using System;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using ZeroNet.Http.Route;

namespace PerformanceService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello ZeroNet");

            StationProgram.RegisteStation(new PerformanceCounter());
            StationProgram.RegisteStation(new RuntimeWaring());
            StationProgram.Initialize();
            StationProgram.RunConsole();
        }
    }
}
