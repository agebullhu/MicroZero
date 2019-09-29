using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
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
            Task.Factory.StartNew(Test);
            //ZeroApplication.ZeroNetEvent += Tester.OnZeroEvent;
            ZeroApplication.ZeroNetEvent += OnZeroEvent;
            ZeroApplication.RunAwaite();
        }

        public static void OnZeroEvent(object sender, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.AppRun:
                    break;
            }
        }

        static void Test()
        {
            while (true)
            {
                Console.ReadKey();
                
                var re= ZeroPublisher.DoPublish("UserMessage", "#", "{}");
                Console.WriteLine(re);
            }
        }
    }

}
