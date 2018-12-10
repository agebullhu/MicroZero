using System;
using Agebull.ZeroNet.Core;
using ApiTest;

namespace QueueTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.RegistZeroObject<TestQueue>();
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }
    }
}
