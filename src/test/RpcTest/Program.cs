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
            ZeroApplication.RunAwaite();
        }
    }

    internal class ZeroApiTestDispatcher
    {
        private static CancellationTokenSource cancel;

        public static void SystemMonitor_StationEvent(object sender, SystemMonitor.ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.AppRun:
                    if (cancel != null)
                        break;
                    ZeroTrace.WriteInfo("RpcTest", "Test is start");
                    Task.Factory.StartNew(StartTest);
                    break;
                case ZeroNetEventType.AppStop:
                    ZeroTrace.WriteInfo("RpcTest", "Test is start");
                    cancel.Cancel();
                    cancel.Dispose();
                    cancel = null;
                    break;
            }
        }

        private static void StartTest()
        {
            cancel = new CancellationTokenSource();
            ZeroTrace.WriteInfo("LogMonitor", LogRecorder.LogMonitor);
            if (!ZeroApplication.Config.TryGetConfig("Test", out _))
            {
                ZeroTrace.WriteInfo("Test", "No find,try install ...");
                var result = SystemManager.CallCommand("install", "api", "Test", "Test");
                if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                {
                    ZeroTrace.WriteError("Test", "Test install failed");
                    return;
                }
                ZeroTrace.WriteInfo("Test", "Is install ,try start it ...");
                result = SystemManager.CallCommand("start", "Test");
                if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                {
                    ZeroTrace.WriteError("Test", "Can't start station");
                    return;
                }
                ZeroTrace.WriteError("Test", "Station runing");
            }
            Thread.Sleep(1000);
            ZeroTester tester;
            //Task.Factory.StartNew(tester.Test);
            switch (ZeroApplication.Config.SpeedLimitModel)
            {
                default:
                    tester = new ZeroTester { Token = cancel.Token };
                    Task.Factory.StartNew(tester.TestSync);
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
