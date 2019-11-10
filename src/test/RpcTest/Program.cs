using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.EntityModel.Common;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;
using Microsoft.Extensions.DependencyInjection;

namespace RpcTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            IocHelper.ServiceCollection.AddSingleton<Tester, HttpTester>();
            //IocHelper.ServiceCollection.AddSingleton<Tester, ZeroTester>();
            //ZeroApplication.Discove(typeof(Program).Assembly);
            ZeroApplication.Initialize();

            ZeroApplication.ZeroNetEvent += Tester.OnZeroEvent;
            //ZeroApplication.ZeroNetEvent += OnZeroEvent;
            ZeroApplication.RunAwaite();
        }

        public static void OnZeroEvent(object sender, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.AppRun:
                    Task.Factory.StartNew(Test);
                    break;

            }
        }

        static void Test()
        {
            while(true)
            {
                Console.WriteLine("°´¼ü¿ªÊ¼");
                Console.ReadKey();
                var res = ZeroPublisher.Publish("UserMessage", GlobalContext.RequestInfo.Token, "Importer",
                       new ApiResult
                       {
                           Status = new OperatorStatus
                           {
                               GuideCode = "5",
                               ClientMessage = "11,22"
                           }
                       });

                Console.WriteLine(res);
            }
        }
    }

}
