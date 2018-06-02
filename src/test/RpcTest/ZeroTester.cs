using System;
using System.Threading;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    class ZeroTester
    {
        long _count, _error;
        double _runTime;
        private DateTime _start;
        public void Count()
        {
            var now = _count;
            var err = _error;
            ZeroTrace.WriteInfo("Count", $"{now:D8}|{err}  {(int)(now / _runTime):D5}/ms|{(int)(now / (DateTime.Now - _start).TotalMilliseconds)}/ms");
        }

        public void Sync()
        {
        }

        public void Async()
        {
            //var json = caller.GetResult().Result;
            //StationConsole.WriteInfo($"Test::{Task.CurrentId}", json);

            //Thread.Sleep(100000);
            //ApiCounter.Instance.Publish(new CountData
            //{
            //    Start = s,
            //    End = DateTime.Now,
            //    Machine = "Test",
            //    HostName = "Test",
            //    ApiName = "api/login",
            //    Status = caller.Status == WebExceptionStatus.Success ? OperatorStatus.Success : OperatorStatus.RemoteError,

            //});
        }
        private int waitCount;

        public void TestAsync()
        {
            _start = DateTime.Now;
            while (ZeroApplication.IsAlive)
            {
                if (!ZeroApplication.CanDo)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                DateTime s = DateTime.Now;
                Interlocked.Increment(ref waitCount);
                ApiClient.Call("Test", "api/login", "{}");
                DoCount(s);
            }
        }
        public void TestSync()
        {
            _start = DateTime.Now;
            while (ZeroApplication.IsAlive)
            {
                if (!ZeroApplication.CanDo)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (waitCount > ZeroApplication.Config.MaxWait)
                {
                    Thread.Sleep(10);
                    continue;
                }

                DateTime s = DateTime.Now;
                Interlocked.Increment(ref waitCount);
                var task = ApiClient.CallTask("Test", "api/login", "{}");
                task.ContinueWith(t => DoCount(s));
            }
        }

        void DoCount(DateTime s)
        {
            Interlocked.Decrement(ref waitCount);
            Interlocked.Exchange(ref _runTime, (DateTime.Now - s).TotalMilliseconds);
            if (ApiContext.Current.LastError != 0)
                Interlocked.Increment(ref _error);
            Interlocked.Increment(ref _count);
        }
    }
}