using System;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Microsoft.Extensions.DependencyInjection;
using ZeroNet.Http.Route;

namespace PerformanceService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello ZeroNet");

            StationProgram.RegisteStation<PerformanceCounter>();
            StationProgram.RegisteStation<RuntimeWaring>();
            StationProgram.Initialize();
            StationProgram.RunConsole();
        }
    }
}
