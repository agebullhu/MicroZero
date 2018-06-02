using System.Threading;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;

namespace RpcTest
{
    internal class Program
    {
        //private static Recorder recorder;
        private static void Main(string[] args)
        {
            ZeroApplication.AppName = "RpcTest";
            ZeroApplication.Initialize();
            switch (ZeroApplication.Config.SpeedLimitModel)
            {
                default:
                    var tester = new ZeroTester();
                    Task.Factory.StartNew(tester.TestSync);
                    break;
                case SpeedLimitType.Single:
                    tester = new ZeroTester();
                    new Thread(tester.TestSync)
                    {
                        IsBackground = true
                    }.Start();
                    break;
                case SpeedLimitType.ThreadCount:
                    int cnt = 0;
                    while (++cnt <= ZeroApplication.Config.MaxWait)
                    {
                        tester = new ZeroTester();
                        Task.Factory.StartNew(tester.TestAsync);
                    }

                    break;
            }
            ZeroApplication.RunAwaite();
        }
    }
}
