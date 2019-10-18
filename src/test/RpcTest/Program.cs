using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
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
            ////定时过期
            //ApiClient.ApiPlan<Argument<long>, string>("PayCallback", "v1/order/timeout",
            //    new Argument<long> { Value = 10 },
            //    plan_date_type.minute,
            //    100,
            //    "订单超时处理");

            ApiClient.CallApi("MarkPoint", "v1/mk", "{}");
            //while (true)
            //{
            //    Console.ReadKey();

            //    var re= ZeroPublisher.DoPublish("UserMessage", "#", "{}");
            //    Console.WriteLine(re);
            //}
        }
    }

}
