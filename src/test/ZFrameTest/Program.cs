using System.Threading.Tasks;
using Agebull.MicroZero;
using RpcTest;
using ZeroMQ;

namespace ZFrameTest
{
    class Program
    {
        static async Task Main()
        {
            ZContext.Initialize();
            await Tester.StartTest();
            ZContext.Destroy();

            //ZeroApplication.CheckOption();
            //ZeroApplication.RegistZeroObject<HisSummarySubscribe>();
            //ZeroApplication.Initialize();
            //ZeroApplication.RunAwaite();
        }
    }
}
