using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    internal class Program
    {
        //private static Recorder recorder;
        private static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Initialize();
            //IocHelper.Create<IApiCounter>().HookApi();
            SystemMonitor.StationEvent += SystemMonitor_StationEvent;
            Console.WriteLine(LogRecorder.LogMonitor );
            ZeroApplication.RunAwaite();
        }

        private static CancellationTokenSource cancel;
        private static void SystemMonitor_StationEvent(object sender, SystemMonitor.StationEventArgument e)
        {
            switch (e.EventName)
            {
                case "system_stop":
                    ZeroTrace.WriteInfo("RpcTest", "Test is start");
                    cancel.Cancel();
                    cancel.Dispose();
                    break;
                case "program_run":
                    ZeroTrace.WriteInfo("RpcTest", "Test is start");
                    Task.Factory.StartNew(StartTest);
                    break;
            }
        }

        static void StartTest()
        {
            Thread.Sleep(1000);
            ZeroTester tester;
            cancel = new CancellationTokenSource();
            //Task.Factory.StartNew(tester.Test);
            switch (ZeroApplication.Config.SpeedLimitModel)
            {
                default:
                    tester = new ZeroTester { Token = cancel.Token };
                    Task.Factory.StartNew(tester.TestSync, cancel.Token);
                    break;
                case SpeedLimitType.Single:
                    tester = new ZeroTester { Token = cancel.Token };
                    new Thread(tester.Test)
                    {
                        IsBackground = true
                    }.Start();
                    break;
                case SpeedLimitType.ThreadCount:
                    int max = (int)(Environment.ProcessorCount * ZeroApplication.Config.TaskCpuMultiple); if (max < 1)
                        max = 1;
                    for (int idx = 0; idx < max; idx++)
                    {
                        tester = new ZeroTester { Token = cancel.Token };
                        new Thread(tester.Test)
                        {
                            IsBackground = true
                        }.Start();
                    }

                    break;
            }
        }
    }
}
