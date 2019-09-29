using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;

namespace RpcTest
{

    internal abstract class Tester
    {
        public CancellationToken Token => Cancel.Token;
        public CancellationTokenSource Cancel { get; set; }

        public long SuCount;
        public long ExCount;
        public long BugError;
        public long NetError;
        public long ExError;
        public long TmError;
        public double Max = double.NaN;
        public double Min = double.NaN;
        public double Last = 0;
        public long WkError;
        public long RunTime;
        public DateTime Start;
        public int WaitCount;
        public static readonly int Qps;

        public static readonly string Api;
        public static readonly string Arg;
        public static readonly string Host;

        public static readonly string Station;

        static Tester()
        {
            var sec = ConfigurationManager.Get("ApiTest");
            if (sec.IsEmpty)
                throw new Exception("’“≤ªµΩApiTest≈‰÷√Ω⁄");
            Station = sec["Station"];
            Api = sec["Api"];
            Arg = sec["Argument"];
            Host = sec["HttpRouteAddress"];
            Qps = sec.GetInt("Qps", 100);
        }

        public abstract bool Init();

        public static void OnZeroEvent(object sender, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.AppRun:
                    ZeroTrace.SystemLog("RpcTest", "Test is start");
                    var test = IocHelper.Create<Tester>();
                    test.StartTest();
                    break;
                case ZeroNetEventType.AppStop:
                    ZeroTrace.SystemLog("RpcTest", "Test is stop");
                    test = IocHelper.Create<Tester>();
                    test.Cancel?.Cancel();
                    break;
            }
        }

        private void StartTest()
        {
            WaitToEnd();
            if (!Init())
                return;
            Cancel = new CancellationTokenSource();
            new Thread(Test).Start();
            //Start = DateTime.Now;
            //var option = ZeroApplication.GetClientOption(Station);
            //switch (option.SpeedLimitModel)
            //{
            //    case SpeedLimitType.Single:
            //        new Thread(Test).Start();
            //        break;
            //    case SpeedLimitType.ThreadCount:
            //        int max = (int)(Environment.ProcessorCount * option.TaskCpuMultiple);
            //        if (max < 1)
            //            max = 1;
            //        for (int idx = 0; idx < max; idx++)
            //        {
            //            new Thread(Test).Start();
            //        }
            //        break;
            //    default:
            //        Task.Factory.StartNew(TestSync, Cancel.Token);
            //        break;
            //}

            //new Thread(Test1).Start();
        }

        void Async()
        {
            using (IocScope.CreateScope())
            {
                DateTime s = DateTime.Now;
                DoAsync();

                Interlocked.Decrement(ref WaitCount);

                var sp = (DateTime.Now - s);
                Last = sp.TotalMilliseconds;
                Interlocked.Add(ref RunTime, sp.Ticks);
                if (sp.TotalMilliseconds > Max || double.IsNaN(Max))
                    Max = sp.TotalMilliseconds;
                if (sp.TotalMilliseconds < Min || double.IsNaN(Min))
                    Min = sp.TotalMilliseconds;
                if (sp.TotalMilliseconds > 1000)
                    Interlocked.Increment(ref TmError);

                if ((Interlocked.Increment(ref ExCount) % 100) == 0)
                    Count();
            }
        }
        protected abstract void DoAsync();

        private int testerCount;

        void WaitToEnd()
        {
            while (Cancel != null)
                Thread.Sleep(10);
        }

        void OnTestStar()
        {
            Interlocked.Increment(ref testerCount);
        }
        void OnTestEnd()
        {
            if (Interlocked.Decrement(ref testerCount) == 0)
            {
                Cancel.Dispose();
                Cancel = null;
            }
        }

        public void Test()
        {
            ZeroTrace.SystemLog("RpcTest", "Test is runing");
            int sleep = 1000 / Qps;
            TaskScheduler s = TaskScheduler.Default;
            
            Start = DateTime.Now;
            while (!Token.IsCancellationRequested && ZeroApplication.CanDo)
            {
                if(WaitCount >= Qps)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                for (int i = 0; i < Qps; i++)
                {
                    Thread.Sleep(sleep);
                    Interlocked.Increment(ref WaitCount);
                    var task = new Task(Async, TaskCreationOptions.PreferFairness);
                    task.Start(s);
                }
                Count();
            }
        }

        public void TestSync()
        {
            Thread.Sleep(100);
            ZeroTrace.SystemLog("RpcTest", "Tester.TestSync", Task.CurrentId, "Start");

            OnTestStar();
            while (!Token.IsCancellationRequested && ZeroApplication.CanDo)
            {
                if (WaitCount > ZeroApplication.Config.MaxWait)
                {
                    Thread.Sleep(10);
                    continue;
                }
                Task.Factory.StartNew(Async, CancellationToken.None);
            }
            Count();
            OnTestEnd();
        }

        public void Count()
        {
            TimeSpan ts = TimeSpan.FromTicks(RunTime);
            var to = ts.TotalMilliseconds / ExCount;
            if (to < 0.0001F)
                to = double.NaN;
            var avg = ExCount / (DateTime.Now - Start).TotalSeconds;
            if (avg < 0.0001F)
                avg = double.NaN;
            GC.Collect();
            ZeroTrace.SystemLog(
                $"[Count] {ExCount} [Wait] {ApiProxy.Instance?.WaitCount}/{WaitCount} Last:{Last:F3}ms | Max:{Max:F3}ms - Min:{Min:F3}ms | {to:F3}ms/qps | {avg:F3} /s",
                $"[Error] net:{NetError:D8} | worker:{WkError:D8} | time out:{TmError:D8} | bug:{BugError:D8}");
        }

        #region old

        public void Test1()
        {
            Thread.Sleep(100);
            ZeroTrace.SystemLog("RpcTest", "Tester.Test", Task.CurrentId, "Start");
            Start = DateTime.Now;
            OnTestStar();
            while (!Token.IsCancellationRequested && ZeroApplication.CanDo)
            {
                Thread.Sleep(10);
                Async();
            }
            Count();
            OnTestEnd();
        }

        public void TestSignle()
        {
            Thread.Sleep(3000);
            ZeroTrace.SystemLog("RpcTest", "Tester.Test", Task.CurrentId, "Start");
            Start = DateTime.Now;
            OnTestStar();
            while (!Token.IsCancellationRequested && ZeroApplication.CanDo)
            {
                if (WaitCount > ZeroApplication.Config.MaxWait)
                {
                    Thread.Sleep(10);
                    continue;
                }
                Async();
            }
            Count();
            OnTestEnd();
        }
        #endregion
    }
}