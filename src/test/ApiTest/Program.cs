using Agebull.Common.Ioc;
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
            ZeroApplication.AppName = "ApiTest";
            LogRecorder.LogMonitor = false;
            ZeroApplication.Initialize();
            ZeroApplication.Discove();
            IocHelper.Create<IApiCounter>()?.HookApi();
            ZeroApplication.RunAwaite();
        }
    }
}
