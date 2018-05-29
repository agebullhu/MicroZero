using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.Log;
using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.Initialize();
            //LogRecorder.Initialize(new RemoteRecorder());
            LogRecorder.LogMonitor = true;
            //ApiCounter.Instance.HookApi();
            ZeroApplication.Discove();
            //Task.Factory.StartNew(Counter);
            ZeroApplication.RunAwaite();
            LogRecorder.Shutdown();
        }
    }
}
