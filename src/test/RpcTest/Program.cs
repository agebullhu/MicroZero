using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gboxt.Common.DataModel;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using ZmqNet.Rpc.Core.ZeroNet;
//using ZmqNet.Rpc.Core.ZeroNet;
namespace RpcTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello ZeroNet");
            //StationProgram.RegisteApiStation(new ApiStation
            //{
            //    StationName = "agebull",
            //    ExecFunc = ExecCommand
            //});
            //StationProgram.RunConsole();

            while (true)
            {
                Console.ReadKey();
                cnt = 0;
                Console.WriteLine("start...");
                DateTime start = DateTime.Now;

                time = 0.0;
                for (int i = 0; i < 128; i++)
                    tasks.Add(Task.Factory.StartNew(Heart).Id);
                //foreach (var task in tasks)
                //    task.Wait();
                double total;
                while (tasks.Count > 0)
                {
                    Thread.Sleep(1000);
                    total = (DateTime.Now - start).TotalMilliseconds;
                    Console.WriteLine($"{cnt}/{time}ms => {total} -- { total / cnt }ms -- { time / cnt }ms");
                }
                total = (DateTime.Now - start).TotalMilliseconds;
                Console.WriteLine($"***{cnt}/{time}ms => {total} -- { total / cnt }ms -- { time / cnt }ms");
            }
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static string ExecCommand(List<string> args)
        {
            switch (args[2])
            {
                case "info":
                    return "{\"Result\":true,\"ResultData\":\"***\"}";
            }
            return $"{{\"Result\":false,\"Caller\":\"{args[0]}\",\"Message\":\"{args[2]}\",\"ErrorCode\":0}}";
        }
        static List<int> tasks = new List<int>();
        private static volatile int cnt;
        private static double time;
        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        public static void Heart()
        {
            var request = new RequestSocket();
            try
            {
                request.Options.Identity = RandomOperate.Generate(8).ToAsciiBytes();
                request.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                request.Connect("tcp://10.5.202.234:20184"); //127.0.0.1  10.5.202.234
                for (int i = 0; i < 1024; i++)
                {
                    DateTime s = DateTime.Now;
                    if (!request.TrySendFrame(new TimeSpan(0, 0, 0, 0, 500), "test", true))
                    {
                        Console.WriteLine("*  1");
                        return;
                    }
                    if (!request.TrySendFrame(new TimeSpan(0, 0, 0, 0, 500), "{}"))
                    {
                        Console.WriteLine("*  3");
                        return;
                    }
                    bool more = true;
                    //收完消息
                    while (more)
                    {
                        string result;
                        if (request.TryReceiveFrameString(new TimeSpan(0, 0, 0, 500), out result, out more))
                        {
                            continue;
                        }
                        Console.WriteLine("*  4");
                        return;
                    }
                    time += (DateTime.Now - s).TotalMilliseconds;
                    cnt++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                request.Close();
                tasks.Remove(Task.CurrentId.Value);
            }
        }
    }

}
/*
 
        static string name = RandomOperate.Generate(10);
        static Semaphore semaphore = new Semaphore(0, 1);
        static Queue<CommandAgument> Queue = new Queue<CommandAgument>();
        private static bool runing = false;
        class CommandAgument
        {
            public string Commmand { get; set; }
            public string Argument { get; set; }
        }
        static void pool()
        {
            try
            {
                var request = new RequestSocket();

                request.Options.Identity = name.ToAsciiBytes();
                request.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                request.Connect("tcp://127.0.0.1:20181");
                var timeout = new TimeSpan(0, 0, 30);

                while (runing)
                {
                    if (!semaphore.WaitOne(timeout))
                        continue;
                    if (!runing)
                    {
                        semaphore.Release();
                        break;
                    }
                    if (Queue.Count == 0)
                        continue;
                    var arg = Queue.Dequeue();
                    request.TrySendFrame(timeout, arg.Commmand ?? "", true);
                    request.TrySendFrameEmpty(timeout, true);
                    request.TrySendFrame(timeout, arg.Argument ?? "");
                    bool more;
                    string result;
                    if (!request.TryReceiveFrameString(timeout, out result, out more))
                    {
                        Console.Write("处理超时");
                        semaphore.Release();
                        Task.Factory.StartNew(pool);
                        break;
                    }
                    Console.Write("返回：" + result);
                    while (more)
                    {
                        if (!request.TryReceiveFrameString(timeout, out result, out more))
                        {
                            Task.Factory.StartNew(pool);
                            Task.Factory.StartNew(pool);
                            break;
                        }
                        Console.Write(result);
                    }
                    Console.WriteLine();
                    semaphore.Release();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
*/
