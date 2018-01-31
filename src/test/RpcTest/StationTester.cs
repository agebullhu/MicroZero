using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Gboxt.Common.DataModel;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Agebull.ZeroNet.Core;
namespace RpcTest
{
    class StationTester
    {
        public static void DoTest()
        {
            Console.WriteLine("Hello ZeroNet");
            //StationProgram.RegisteApiStation(new SubStation
            //{
            //    StationName = "BusinessMonitor",
            //    ExecFunc = ExecCommand
            //});
            //StationProgram.Run/*Console*/();
            StationProgram.RegisteApiStation(new SubStation
            {
                StationName = "EntityEvent",
                ExecFunc = ExecCommand
            });
            StationProgram.RunConsole();

            while (true)
            {
                Console.ReadKey();
                //MarkPoint();
                cnt = 0;
                Console.WriteLine("start...");
                DateTime start = DateTime.Now;

                time = 0.0;
                for (int i = 0; i < 256; i++)
                    tasks.Add(Task.Factory.StartNew(ApiTest).Id);
                //foreach (var task in tasks)
                //    task.Wait();
                double total;
                while (tasks.Count > 0)
                {
                    Thread.Sleep(1000);
                    total = (DateTime.Now - start).TotalMilliseconds;
                    Console.WriteLine($"{cnt}/{time}ms/{total}ms => {cnt / time * 1000}/s -- {cnt / total * 1000}/s -- { total / cnt }ms -- { time / cnt }ms");
                }
                total = (DateTime.Now - start).TotalMilliseconds;
                Console.WriteLine($"{cnt}/{time}ms/{total}ms => {cnt / time * 1000}/s -- {cnt / total * 1000}/s -- { total / cnt }ms -- { time / cnt }ms");
            }
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static void ExecCommand(string args)
        {
           // Console.WriteLine(args);
        }
        static List<int> tasks = new List<int>();
        private static volatile int cnt;
        private static double time;


        private static string station = "UserCenter";
        private static string api = "record/v1";
        private static string argument = JsonConvert.SerializeObject(new MarkPointRecord
        {
            ActionId = 15,
            TraceMark = "fy-baidu-fq-stfc-5",
            Value = "{\"UrlAddress\":\"toufang.html\"}",
            Os = "IOS",
            Browser = "MicroMessage"
        });
        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        public static void ApiTest()
        {
            var request = new RequestSocket();
            var config = StationProgram.GetConfig(station);
            try
            {
                int id = 1;
                request.Options.Identity = RandomOperate.Generate(8).ToAsciiBytes();
                request.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                request.Connect(config.OutAddress);
                //for (int i = 0; i < 1024; i++)//
                {
                    Console.WriteLine("request...");
                    DateTime s = DateTime.Now;
                    request.SendFrame(api, true);
                    request.SendFrame($"{++id}", true);
                    request.SendFrame("{}", true);
                    if (!request.TrySendFrame(new TimeSpan(0, 0, 0, 3), argument))
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
                            Console.WriteLine(result);
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
                //tasks.Remove(Task.CurrentId.Value);
            }
        }


    }

    /// <summary>
    /// 埋点记录
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class MarkPointRecord
    {
        /// <summary>
        /// 动作ID
        /// </summary>
        /// <remarks>
        /// 动作ID
        /// </remarks>
        [DataMember,JsonProperty, DisplayName(@"动作ID")]
        public int ActionId
        {
            get;
            set;
        }

        /// <summary>
        /// 浏览器
        /// </summary>
        /// <remarks>
        /// 微信中用Webchart，App中用Yizuanbao，其它浏览器用名称
        /// </remarks>
        [DataMember,JsonProperty, DisplayName(@"浏览器")]
        public string Browser
        {
            get;
            set;
        }

        /// <summary>
        /// 追踪码
        /// </summary>
        /// <remarks>
        /// 追踪码
        /// </remarks>
        [DataMember,JsonProperty, DisplayName(@"追踪码")]
        public string TraceMark
        {
            get;
            set;
        }

        /// <summary>
        /// 渠道码
        /// </summary>
        /// <remarks>
        /// 渠道码
        /// </remarks>
        [DataMember,JsonProperty, DisplayName(@"渠道码")]
        public string ChannalCode
        {
            get;
            set;
        }

        /// <summary>
        /// 值
        /// </summary>
        /// <remarks>
        /// 值
        /// </remarks>
        [DataMember, JsonProperty, DisplayName(@"值")]
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// 操作系统
        /// </summary>
        /// <remarks>
        /// 操作系统
        /// </remarks>
        [DataMember,JsonProperty, DisplayName(@"操作系统")]
        public string Os
        {
            get;
            set;
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
