using System;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.ZeroNet.Core;
using ApiTest;
using ZeroMQ;
using ZeroNet.Http.Gateway;


namespace QueueTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.RegistZeroObject<WechatCallBackQueue>();
            ZeroApplication.Initialize();

            var senparcStartup = new SenparcStartup
            {
                Configuration = ConfigurationManager.Root
            };
            senparcStartup.ConfigureServices(IocHelper.ServiceCollection);
            ZeroApplication.RunAwaite();
        }
    }
}
