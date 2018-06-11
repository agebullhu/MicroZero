using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace RpcTest
{
    class ZeroTester
    {
        public CancellationToken Token;
        static long _count, _blError, _netError, _exError, _tmError;
        static long _runTime;
        static DateTime _start;

        //System.Collections.Generic.Dictionary< int ,int >

        public void Async()
        {
            DateTime s = DateTime.Now;
            Interlocked.Increment(ref waitCount);
            ApiClient client = new ApiClient
            {
                Station = "Test",
                Commmand = "api/login",
                Argument = "{}"
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

        private readonly string json = JsonConvert.SerializeObject(new
        {
            MobilePhone = "15618965007",
            UserPassword = "123456A",
            VerificationCode = "9999"
        });
        public void Test()
        {
            _start = DateTime.Now;
            while (ZeroApplication.IsAlive)
            {
                if (waitCount > ZeroApplication.Config.MaxWait)
                {
                    Thread.Sleep(100);
                    continue;
                }

                Async();
            }

            Count();
        }
        public void TestSync()
        {
            _start = DateTime.Now;
            while (!Token.IsCancellationRequested && ZeroApplication.IsAlive)
            {
                if (waitCount > ZeroApplication.Config.MaxWait)
                {
                    Thread.Sleep(100);
                    continue;
                }
                Task.Factory.StartNew(Async, Token);
            }
            Count();
        }

        public static void Count()
        {
            TimeSpan ts = TimeSpan.FromTicks(_runTime);
            ZeroTrace.WriteInfo("Count", $"{_count:D8} | {_netError:D8} | {_tmError:D8} | {_blError:D8}  {ts.TotalMilliseconds / _count}ms | {_count / (DateTime.Now - _start).TotalSeconds}/s");
        }
    }
}