using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using ZeroMQ;

namespace RpcTest
{
    class ZeroTester
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
            long pre = 0, now,err;
            double tl;
            DateTime start = DateTime.Now;
            while (ZeroApplication.IsAlive)
            {
                Thread.Sleep(1000);
                now = count;
                err = error;
                tl = (DateTime.Now - start).TotalSeconds;


                ZeroTrace.WriteLoop("Run", $"{now:D8}|{err}  {(int)(now / tm):D5}/ms|{(now / tl)}/ms Last:{ now - pre}                           ");
                pre = now;
                
            }
            now = count;
            err = error;
            tl = (DateTime.Now - start).TotalMilliseconds;
            ZeroTrace.WriteLoop("End", $"{now:D8}|{err}  {(int)(now / tm):D5}/ms|{(int)(now / tl)}/ms Last:{ now - pre}                           ");
            
        }

        public void TestOnce()
        {
            ApiClient.Call("Test", "api/login", "{}");
        }

        public void TestTask()
        {
            //ZeroTrace.WriteInfo($"Test::{Task.CurrentId}",ZeroApplication.Configs["Test"].RequestAddress);
            while (ZeroApplication.IsAlive)
            {
                if (!ZeroApplication.CanDo)
                {
                    Console.Write(".");
                    Thread.Sleep(1000);
                    continue;
                }
                DateTime s = DateTime.Now;
                ApiClient.Call("Test", "api/login", "{}");
                Interlocked.Exchange(ref tm, (DateTime.Now - s).TotalMilliseconds);
                if (ApiContext.Current.LastError != 0)
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