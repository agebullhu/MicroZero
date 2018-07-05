using Agebull.Common.Ioc;
using Agebull.ZeroNet.Core;
using Microsoft.Extensions.DependencyInjection;

namespace RpcTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            IocHelper.ServiceCollection.AddSingleton<Tester, HttpTester>();
            ZeroApplication.Initialize();
            ZeroApplication.ZeroNetEvent += Tester.OnZeroEvent;
            ZeroApplication.RunAwaite();
        }
    }

}
