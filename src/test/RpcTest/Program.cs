using Agebull.Common.Ioc;
using Agebull.ZeroNet.Core;

namespace RpcTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            //IocHelper.AddSingleton<Tester,ZeroTester>();
            //ZeroApplication.Discove(typeof(Program).Assembly);
            ZeroApplication.Initialize();
            //ZeroApplication.ZeroNetEvent += Tester.OnZeroEvent;
            ZeroApplication.RunAwaite();
        }
    }

}
