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
            s = DateTime.Now;

            for (int i = 0; i < 16; i++)
                Task.Factory.StartNew(DoTest);
            Console.ReadKey();

            ZeroApplication.Shutdown();
        }
        static DateTime s;
        static int idx = 0;
        static void DoTest()
        {
            while(idx < 10000000)
            {
                Test();
            }
        }

        static void Test()
        {
            //using (IocScope.CreateScope())
            {
                GlobalContext.Current.Request.RequestId = RandomOperate.Generate(8);
                try
                {
                    var result = ApiClient.CallApi("PayCallback", "CallBack", "{}");
                    if (!result.Success)
                        Console.WriteLine(result.Status.ClientMessage);
                }
                catch (Exception e)
                {
                    LogRecorderX.Exception(e);
                }
            }
            if (Interlocked.Add(ref idx, 1) % 100 == 0)
            {
                var sc = (DateTime.Now - s).TotalMilliseconds;
                Console.WriteLine($"{idx} : {sc}ms : { idx / sc * 1000} / s");
            }
        }
    }
}
