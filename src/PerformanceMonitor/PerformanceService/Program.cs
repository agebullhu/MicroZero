using System;
using Agebull.ZeroNet.Core;
using ZeroNet.Http.Route;

namespace PerformanceService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello ZeroNet");

            StationProgram.RegisteApiStation(new SubStation
            {
                StationName = "PerformanceCounter",
                Subscribe= "RouteCounter",
                ExecFunc = RouteCounter.Record
            });

            StationProgram.RunConsole();
        }
    }
}
