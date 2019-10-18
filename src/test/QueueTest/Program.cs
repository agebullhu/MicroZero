using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.MicroZero;
using ApiTest;


namespace QueueTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.RegistZeroObject<PayCallbackController>();
            ZeroApplication.Initialize();

            ZeroApplication.RunAwaite();
        }
    }
}
