using Agebull.ZeroNet.Core;
using RpcTest;
using System;
using ZeroMQ;

namespace ZFrameTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ZContext.Initialize();
            Tester.StartTest();
            ZContext.Destroy();
        }
    }
}
