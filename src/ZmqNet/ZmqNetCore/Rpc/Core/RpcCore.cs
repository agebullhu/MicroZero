using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///     RPC核心
    /// </summary>
    public class RpcCore : RpcEnvironment
    {
        #region 单例
        /// <summary>
        /// 是否WEB环境
        /// </summary>
        public static bool IsWeb { get; set; }
        /// <summary>
        ///     全局单例
        /// </summary>
        public static RpcCore Singleton { get; private set; }
        /// <summary>
        /// 配置
        /// </summary>
        public RpcConfig Config { get; set; }

        /// <summary>
        ///     启动
        /// </summary>
        public static void Run()
        {
            if (Singleton != null)
            {
                ShutDown();
            }
            Singleton = new RpcCore();
            Singleton.init_net_command();
            Singleton.start_net_command();
        }

        /// <summary>
        ///     结束
        /// </summary>
        public static void ShutDown()
        {
            if (Singleton == null)
                return;
            lock (Singleton)
            {
                Singleton.close_net_command();
                Singleton.distory_net_command();
                Singleton = null;
                m_netState = ZmqNetStatus.None;
            }
        }

        #endregion


        #region 运行环境

        /// <summary>
        ///     请求消息泵
        /// </summary>
        public ZmqQuestionPump Request { get; private set; }

        /// <summary>
        ///     回应消息泵
        /// </summary>
        public ZmqAnswerPump Answer { get; private set; }

        /// <summary>
        ///     通知消息泵
        /// </summary>
        public ZmqNotifyPump Notify { get; private set; }


        /// <summary>
        ///     初始化网络命令环境
        /// </summary>
        /// <returns></returns>
        public ZmqNetStatus init_net_command()
        {
            Debug.Assert(m_netState == ZmqNetStatus.None, "初始化网络命令环境只能调用一次且为流程起始");
            //读取配置
            Console.WriteLine("读取网络命令配置...");
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!IsWeb)
            {
                var path = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "rpc.json");
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);
                    Config = JsonConvert.DeserializeObject<RpcConfig>(content);
                }
            }
            else
            {
                Config = new RpcConfig
                {
                    RequestUrl = ConfigurationManager.AppSettings["RpcRequestUrl"],
                    AnswerUrl = ConfigurationManager.AppSettings["RpcAnswerUrl"],
                    NotifyUrl = ConfigurationManager.AppSettings["RpcNotifyUrl"],
                    Token = ConfigurationManager.AppSettings["RpcClientKey"]
                };
            }

            Token = new byte[GUID_LEN];
            var bytes = Config.Token.ToAsciiBytes();
            //客户端用户标识
            for (var idx = 0; idx < GUID_LEN && idx < bytes.Length; idx++)
                Token[idx] = bytes[idx];
            Console.WriteLine("初始化网络命令环境...");
            //初始化客户端
            Request = new ZmqQuestionPump
            {
                ZmqAddress = Config.RequestUrl
            };
            Request.Initialize();
            Answer = new ZmqAnswerPump
            {
                ZmqAddress = Config.AnswerUrl
            };
            Answer.Initialize();
            if (EventWorkerContainer.CreateWorker != null)
            {
                Notify = new ZmqNotifyPump
                {
                    ZmqAddress = Config.NotifyUrl
                };
                Notify.Initialize();
            }

            Console.WriteLine("完成网络命令环境初始化");
            m_netState = ZmqNetStatus.Initialized;
            return m_netState;
        }

        /// <summary>
        ///     启动网络命令环境
        /// </summary>
        /// <returns></returns>
        public ZmqNetStatus start_net_command()
        {
            Console.WriteLine("正在启动网络命令环境...");
            Debug.Assert(m_netState == ZmqNetStatus.Initialized,
                "启动网络命令环境前必须已初始化网络命令环境,即调用init_net_command是start_net_command的前置条件");
            m_netState = ZmqNetStatus.Runing;

            //Thread thread = new Thread(TimeOutCheckTask)
            //{
            //    Priority = ThreadPriority.Lowest,
            //    IsBackground = true
            //};
            //thread.Start();
            Request.Run();
            Answer.Run();
            if (EventWorkerContainer.CreateWorker != null)
                Notify.Run();
            return m_netState;
        }

        /// <summary>
        ///     关闭网络命令环境
        /// </summary>
        public void close_net_command()
        {
            Console.WriteLine("正在关闭网络命令环境...");
            Debug.Assert(m_netState == ZmqNetStatus.Runing,
                "关闭网络命令环境前必须已启动网络命令环境,即调用start_net_command是close_net_command的前置条件");
            m_netState = ZmqNetStatus.Closing;
            while (m_commandThreadCount > 0)
                Thread.Sleep(50);
            m_netState = ZmqNetStatus.Closed;
            Console.WriteLine("网络命令环境已关闭");
        }

        /// <summary>
        ///     销毁网络命令环境
        /// </summary>
        public void distory_net_command()
        {
            Debug.Assert(m_netState == ZmqNetStatus.Closed,
                "销毁网络命令环境前必须已关闭网络命令环境,即调用close_net_command是distory_net_command的前置条件");
            Request.Dispose();
            Answer.Dispose();
            if (EventWorkerContainer.CreateWorker != null)
                Notify.Dispose();
            m_netState = ZmqNetStatus.None;
            Console.WriteLine("网络命令环境已销毁");
        }

        #endregion
    }
}