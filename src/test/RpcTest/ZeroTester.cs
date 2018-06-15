using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    internal class ZeroTester
    {
        public CancellationToken Token;
        private static long _count;
        private static long _blError;
        private static long _netError;
        private static long _exError;
        private static long _tmError;
        private static long _runTime;
        private static DateTime _start;

        private readonly string _station;
        private readonly string _api;
        private readonly string _arg;

        public ZeroTester()
        {
            var sec = ConfigurationManager.Get("ApiTest");
            if (sec.IsEmpty)
                throw new Exception("ÕÒ²»µ½ApiTestÅäÖÃ½Ú");
            _station = sec["Station"];
            _api = sec["Api"];
            _arg= sec["Argument"]; 
        }

        public void Async()
        {
            DateTime s = DateTime.Now;
            Interlocked.Increment(ref waitCount);
            ApiClient client = new ApiClient
            {
                Station = _station,
                Commmand = _api,
                Argument = _arg
            };
            client.CallCommand();
            switch (client.State)
            {
                case ZeroOperatorStateType.NetError:
                    Interlocked.Increment(ref _netError);
                    break;
                case ZeroOperatorStateType.Exception:
                    Interlocked.Increment(ref _exError);
                    break;
            }
            var sp = (DateTime.Now - s);
            if (sp.TotalMilliseconds > 500)
                Interlocked.Increment(ref _tmError);

            Interlocked.Add(ref _runTime, sp.Ticks);
            if (ApiContext.Current.LastError != 0)
                Interlocked.Increment(ref _blError);
            Interlocked.Increment(ref _count);
            Interlocked.Decrement(ref waitCount);
        }
        private int waitCount;

        public void Test()
        {
            ZeroTrace.WriteInfo("RpcTest", "Tester.Test", Task.CurrentId, "Start");
            _start = DateTime.Now;
            while (!Token.IsCancellationRequested && ZeroApplication.InRun)
            {
                if (waitCount > ZeroApplication.Config.MaxWait)
                {
                    Thread.Sleep(100);
                    continue;
                }

                Async();
            }

            Count();
            ZeroTrace.WriteInfo("RpcTest", "Tester.Test", Task.CurrentId,"Close");
        }
        public void TestSync()
        {
            ZeroTrace.WriteInfo("RpcTest", "Tester.TestSync", Task.CurrentId, "Start");
            _start = DateTime.Now;
            while (!Token.IsCancellationRequested && ZeroApplication.InRun)
            {
                if (waitCount > ZeroApplication.Config.MaxWait)
                {
                    Thread.Sleep(100);
                    continue;
                }
                Task.Factory.StartNew(Async);
            }
            Count();
            ZeroTrace.WriteInfo("RpcTest", "Tester.TestSync", Task.CurrentId, "Close");
        }

        public static void Count()
        {
            TimeSpan ts = TimeSpan.FromTicks(_runTime);
            ZeroTrace.WriteInfo("Count", $"{_count:D8} | {_netError:D8} | {_tmError:D8} | {_blError:D8}  {ts.TotalMilliseconds / _count}ms | {_count / (DateTime.Now - _start).TotalSeconds}/s");
        }
    }
}