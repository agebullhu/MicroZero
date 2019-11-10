using System;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.Common.OAuth;
using Agebull.EntityModel.Common;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;


namespace ApiTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(TestController).Assembly);
            ZeroApplication.Initialize();
            ZeroApplication.ZeroNetEvent += OnZeroEvent;
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
            GlobalContext.SetUser(new LoginUserInfo { UserId = 1 });
            while (true)
            {
                Console.WriteLine("°´¼ü¿ªÊ¼");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                    return;
                try
                {
                    var res = ZeroPublisher.Publish("UserMessage", "test");
                    //var res = ApiClient.CallApi("ApiTest", "v1/test");

                    Console.WriteLine(JsonHelper.SerializeObject(res));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }

}
