using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;

namespace QueuePublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Initialize();
            ZeroApplication.Run();

            Test();

            ZeroApplication.Shutdown();
        }

        static void Test()
        {
            using (IocScope.CreateScope())
            {
                GlobalContext.Current.Request.RequestId = RandomOperate.Generate(8);
                try
                {
                    ApiClient.CallApi("WechatCallBack", "CallBack", "{}");
                }
                catch (Exception e)
                {
                    LogRecorderX.Exception(e);
                }
            }
        }
    }
}
