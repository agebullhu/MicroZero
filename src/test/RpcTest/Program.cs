using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    internal class Program
    {
        //private static Recorder recorder;
        private static void Main(string[] args)
        {
            ZeroApplication.AppName = "RpcTest";
            ZeroApplication.RegistZeroObject(new ApiProxy()
            {
                StationName ="Test"
            });
            ZeroApplication.Initialize();
            var tester = new ZeroTester();
            //Task.Factory.StartNew(tester.Test);
            switch (ZeroApplication.Config.SpeedLimitModel)
            {
                default:
                    tester = new ZeroTester();
                    Task.Factory.StartNew(tester.TestSync);
                    break;
                case SpeedLimitType.Single:
                    tester = new ZeroTester();
                    new Thread(tester.Test)
                    {
                        IsBackground = true
                    }.Start();
                    break;
                case SpeedLimitType.ThreadCount:
                    int cnt = 0;
                    while (++cnt <= ZeroApplication.Config.TaskCpuMultiple * Environment.ProcessorCount)
                    {
                        tester = new ZeroTester();
                        Task.Factory.StartNew(tester.Test);
                    }

                    break;
            }
            ZeroApplication.RunAwaite();
        }
    }
}
