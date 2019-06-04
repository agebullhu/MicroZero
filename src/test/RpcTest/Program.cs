using Agebull.Common.Ioc;
using Agebull.MicroZero;

namespace RpcTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            IocHelper.AddSingleton<Tester, HttpTester>();
            //ZeroApplication.Discove(typeof(Program).Assembly);
            ZeroApplication.Initialize();
            ZeroApplication.ZeroNetEvent += Tester.OnZeroEvent;
            ZeroApplication.RunAwaite();
        }
    }

}
