using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using ZeroNet.Http.Route;

namespace RpcTest
{
    class HtttpTester
    {
        public void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ZeroApplication.Shutdown();
        }

        long count = 0, error = 0;
        //DateTime start = DateTime.Now;
        double tm;
        public void Counter()
        {
            Console.WriteLine();
            long pre = 0;
            DateTime dn = DateTime.Now;
            while (ZeroApplication.IsAlive)
            {
                Thread.Sleep(500);
                var now = count;
                var e = error;
                ZeroTrace.WriteLoop("Run", $"{now:D8}|{e}  {(int)(now / tm):D5}/ms|{(int)(now / (DateTime.Now - dn).TotalMilliseconds):D5}/ms Last:{ now - pre}                           ");
                pre = now;
            }
            Console.WriteLine($"{count:D8}|{error}  {(int)(count / tm):D5}/ms|{(int)(count / (DateTime.Now - dn).Milliseconds):D5}/ms Last:{ count - pre}                            ");
        }
        public void TestTask()
        {
            HttpApiCaller caller = new HttpApiCaller("http://192.168.240.132:5000/");
            ZeroTrace.WriteInfo($"Test::{Task.CurrentId}", caller.Host);
            while (ZeroApplication.IsAlive)
            {
                if (!ZeroApplication.InRun )
                {
                    Thread.Sleep(1000);
                    continue;
                }
                DateTime s = DateTime.Now;
                caller.CreateRequest("Test/api/login");
                caller.GetResult().Wait();
                Interlocked.Exchange(ref tm, (DateTime.Now - s).TotalMilliseconds);
                if (caller.Status != WebExceptionStatus.Success)
                    Interlocked.Increment(ref error);
                Interlocked.Increment(ref count);

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
        }
    }
}