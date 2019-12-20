using System;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.Common.OAuth;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;

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
            GlobalContext.SetUser(new LoginUserInfo {UserId =1});
            while (true)
            {
                Console.WriteLine("按键开始");
                var key =Console.ReadKey();
                if(key.Key == ConsoleKey.Q)
                    return;
                //var res = ApiClient.CallApi("eShop", "v1/active/all");

                var res = ZeroPublisher.Publish("UserMessage", "test");
                Console.WriteLine(JsonHelper.SerializeObject(res));
            }
        }

        //static void Test()
        //{
        //    while(true)
        //    {
        //        Console.WriteLine("按键开始");
        //        Console.ReadKey();
        //        var res = ZeroPublisher.Publish("UserMessage", GlobalContext.RequestInfo.Token, "Importer",
        //               new ApiResult
        //               {
        //                   Status = new OperatorStatus
        //                   {
        //                       GuideCode = "5",
        //                       ClientMessage = "11,22"
        //                   }
        //               });

        //        Console.WriteLine(res);
        //    }
        //}
    }

}
