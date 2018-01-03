using Agebull.Zmq.Rpc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gboxt.Common.DataModel;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Yizuan.Service.Api;

namespace RpcTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello {name}");
            Task.Factory.StartNew(sub);
            Task.Factory.StartNew(pool);
            do
            {
                Console.Write("请输入：");
                var cmd = Console.ReadLine();
                //if (cmd == "exist")
                //{
                //    runing = false;
                //    semaphore.Release();
                //    break;
                //}
                var words = cmd.Split(new char[] {' ', '\t', '\r', '\n'});
                if (words.Length == 0)
                {
                    Console.WriteLine("请输入正确命令");
                    continue;
                }
                Queue.Enqueue(new CommandAgument
                {
                    Commmand = words[0],
                    Argument = words.Length == 1 ? null : words[1]
                });
                semaphore.Release();
                semaphore.WaitOne();
            } while (runing);
            //RouteRpc.Run();
            //string msg;
            //do
            //{
            //    msg = Console.ReadLine();
            //    Call(msg);
            //} while (msg != "bye");
            //RouteRpc.ShutDown();
            semaphore.WaitOne();
            //Console.ReadKey();
        }
        static string name = RandomOperate.Generate(10);
        static Semaphore semaphore =new Semaphore(0,1);
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
                request.Connect("tcp://127.0.0.1:20181");
                
                runing = true;
                while (runing)
                {
                    
                    if (!semaphore.WaitOne(new TimeSpan(0, 0, 1)))
                        continue;
                    if (!runing)
                    {
                        semaphore.Release();
                        break;
                    }
                    var arg = Queue.Dequeue();
                    request.SendMoreFrame(arg.Commmand ?? "");
                    request.SendMoreFrameEmpty();
                    request.SendFrame(arg.Argument ?? "");
                    bool more = true;
                    string result = request.ReceiveFrameString(out more);
                    Console.Write("返回：" + result);
                    while (more)
                    {
                        result = request.ReceiveFrameString(out more);
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
            runing = false;
        }

        static void sub()
        {
            try
            {
                var subscriber = new SubscriberSocket();
                subscriber.Options.Identity = name.ToAsciiBytes();
                subscriber.Connect("tcp://127.0.0.1:20183");
                subscriber.Subscribe("");
                runing = true;
                while (runing)
                {
                    bool more = true;
                    string result;
                    if(!subscriber.TryReceiveFrameString(new TimeSpan(0, 0, 10), out result, out more))
                        continue;

                    Console.Write("通知：" + result);
                    if (result == "exit")
                        runing = false;
                    while (more)
                    {
                        result = subscriber.ReceiveFrameString(out more);
                        Console.Write(result);
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private static string Message
        {
            set { Console.WriteLine(value); }
        }

        /// <summary>
        ///     请求命令
        /// </summary>
        /// <returns></returns>
        public static object Call(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return "NoCall";
            object result = null;
            var cmd = new CommandArgument
            {
                userToken = ApiContext.MyServiceKey,
                commandName = command,
                requestId = RandomOperate.Generate(12),
                cmdId = RpcEnvironment.NET_COMMAND_CALL
            };
            cmd.RequestStateChanged += OnRequestStateChanged;
            Message = "Start";
            RouteRpc.Singleton.Request.request_net_cmmmand(cmd);
            Message = "End";
            return result;
        }

        
        /// <summary>
        ///     消息处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="arg">最新接收的参数（如果这不是响应状态则与发送者相同）</param>
        public static void OnRequestStateChanged(CommandArgument sender, CommandArgument arg)
        {
            switch (arg.cmdState)
            {
                case RpcEnvironment.NET_COMMAND_STATE_SENDING:
                    Message = "正在发送请求";
                    return;
                case RpcEnvironment.NET_COMMAND_STATE_UNKNOW_DATA:
                    Message = "正在接收远程数据接";
                    return;
                case RpcEnvironment.NET_COMMAND_STATE_WAITING:
                    Message = "服务器正在处理请求";
                    return;
                case RpcEnvironment.NET_COMMAND_STATE_DATA:
                    Message = "远程数据推送";
                    return;
                case RpcEnvironment.NET_COMMAND_STATE_UNKNOW:
                    Message = "服务器未正确响应";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_CRC_ERROR:
                    Message = "CRC校验错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_NETERROR:
                    Message = "发生网络错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_RETRY_MAX:
                    Message = "超过最大错误重试次数";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_LOGICAL_ERROR:
                    Message = "系统内部错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_SERVER_UNKNOW:
                    Message = "服务器内部错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_TIME_OUT:
                    Message = "请求超时";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_DATA_REPEAT:
                    Message = "应废弃的重复请求";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_ARGUMENT_INVALID:
                    Message = "参数数错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_SUCCEED:
                    Message = "执行成功";
                    break;
            }
            sender.RequestStateChanged -= OnRequestStateChanged;
            RpcEnvironment.RemoveCacheCommand(arg);

            sender.OnEnd?.Invoke(arg);
        }

    }

}
