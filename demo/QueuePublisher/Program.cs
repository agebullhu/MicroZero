using System;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Agebull.MicroZero;
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

            Console.WriteLine("press any key...");

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                    break;
                Test();
            }

            Console.WriteLine("bye");
            ZeroApplication.Shutdown();
        }

        static void Test()
        {
            using (IocScope.CreateScope())
            {
                GlobalContext.Current.Request.RequestId = RandomOperate.Generate(8);
                try
                {
                    var end =ApiClient.CallApi("QueueDemo", "v1/test", new Argument{Value = DateTime.Now.ToLongDateString()});
                    Console.WriteLine(JsonHelper.SerializeObject(end));
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                }
            }
        }
    }
}
