using System;
using Agebull.ZeroNet.Core;

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
