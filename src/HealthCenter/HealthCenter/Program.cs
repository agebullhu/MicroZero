using System;
using Agebull.ZeroNet.Core;
using ZeroNet.Http.Route;

namespace PerformanceService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello ZeroNet");

            ZeroApplication.Initialize();
            ZeroApplication.Discove();
            ZeroApplication.RunAwaite();
        }
    }
}
