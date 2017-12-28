using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Agebull.Common.Logging;
using NetMQ;
using Yizuan.Service.Api;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///     ZMQ命令执行泵
    /// </summary>
    public abstract class ZmqCommandPump<TSocket> : IDisposable
        where TSocket : NetMQSocket
    {
        #region 基础流程
        /// <summary>
        /// 泵名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 运行起来
        /// </summary>
        public void Run()
        {
            m_state = 3;
            DoRun();
            m_state = 4;
        }

        /// <summary>
        /// 线程
        /// </summary>
        internal Thread thread;

        /// <summary>
        /// 启动泵任务
        /// </summary>
        protected virtual void DoRun()
        {
            if (thread != null)
                return;
            lock (this)
            {
                thread = new Thread(RunPump)
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                };
                thread.Start();
            }
        }

        /// <summary>
        /// 重启次数
        /// </summary>
        private int m_restart;

        /// <summary>
        /// 是否已析构
        /// </summary>
        public int RestartCount { get { return m_restart; } }
        /// <summary>
        /// 执行状态
        /// </summary>
        private int m_state;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get { return m_state > 0; } }


        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get { return m_state > 4; } }

        /// <summary>
        /// 状态 0 初始状态,1 正在初始化,2 完成初始化,3 开始运行,4 运行中,5 应该关闭 6 已关闭 7 正在析构 8 完成析构
        /// </summary>
        public int State { get { return m_state; } }

        public void SetClose()
        {
            m_state = 5;
        }

        /// <summary>
        ///     初始化客户端
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
                return;
            m_state = 1;
            m_queue = new Queue<CommandArgument>();
            m_mutex = new Semaphore(0, ushort.MaxValue);
            DoInitialize();
            m_state = 2;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected abstract void DoInitialize();

        /// <summary>
        ///     清理
        /// </summary>
        public void Dispose()
        {
            m_state = 7;
            if (m_mutex != null)
            {
                m_mutex.Dispose();
                m_mutex = null;
            }
            DoDispose();
            m_state = 8;
        }

        /// <summary>
        /// 清理
        /// </summary>
        protected virtual void DoDispose()
        {

        }

        #endregion

        #region 消息队列

        /// <summary>
        ///     C端的命令调用队列
        /// </summary>
        private Queue<CommandArgument> m_queue;

        /// <summary>
        ///     C端命令调用队列锁
        /// </summary>
        private Semaphore m_mutex;

        /// <summary>
        ///     取得队列数据
        /// </summary>
        protected CommandArgument Pop()
        {
            if (!m_mutex.WaitOne(1000))
            {
                return null;
            }
            CommandArgument mdMsg = null;
            if (m_queue.Count != 0)
            {
                mdMsg = m_queue.Dequeue();
            }
            return mdMsg;
        }

        /// <summary>
        ///     数据写入队列
        /// </summary>
        /// <param name="cmdMsg"></param>
        protected void Push(CommandArgument cmdMsg)
        {
            m_queue.Enqueue(cmdMsg);
            m_mutex.Release();
        }

        #endregion

        #region 命令请求

        /// <summary>
        /// 默认超时设置
        /// </summary>
        protected static readonly TimeSpan timeOut = new TimeSpan(0, 0, 10);

        /// <summary>
        /// ZMQ服务地址
        /// </summary>
        public string ZmqAddress { get; set; }


        /// <summary>
        /// 配置Socket对象
        /// </summary>
        /// <param name="socket"></param>
        protected abstract void OptionSocktet(TSocket socket);

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="socket"></param>
        protected virtual void OnConnected(TSocket socket)
        {

        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="socket"></param>
        protected virtual void OnDeConnected(TSocket socket)
        {

        }


        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="command">命令</param>
        /// <param name="argument"></param>
        /// <param name="getResult">取返回值吗</param>
        /// <returns>返回状态</returns>
        protected string SendCommand(TSocket socket, string command, string argument, bool getResult)
        {
            try
            {
                bool state = socket.TrySendFrame(timeOut, command.ToAsciiBytes(), true);
                if (!state)
                {
                    Console.WriteLine("服务器失联");
                    return "Failed";
                }
                state = socket.TrySendFrameEmpty(timeOut, true);
                if (!state)
                {
                    Console.WriteLine("服务器失联");
                    return "Failed";
                }
                state = socket.TrySendFrame(timeOut, argument);
                if (!state)
                {
                    Console.WriteLine("服务器失联");
                    return "Failed";
                }
                if (!getResult)
                    return "";
                bool more;
                string result = null;
                int retry = 0;
                do
                {
                    string re;
                    state = socket.TryReceiveFrameString(timeOut, out re, out more);
                    if (!state)
                    {
                        if (retry > 5)
                            break;
                        retry++;
                        more = true;
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(re))
                        result = re;
                } while (more);
                return result ?? "Empty";
            }
            catch (Exception ex)
            {
                return OnException(socket, ex);
            }
        }


        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="ex"></param>
        /// <returns>返回状态,其中-1会导致重连</returns>
        protected string OnException(TSocket socket, Exception ex)
        {
            if (ex.Message == "Req.XSend - cannot send another request")
            {
                bool more;
                string re;
                do
                {
                    bool state = socket.TryReceiveFrameString(timeOut, out re, out more);
                    if (!state)
                        break;
                    Console.WriteLine(re);
                } while (more);
            }
            LogRecorder.Exception(ex);
            Trace.WriteLine(ex, GetType().Name + "DoWork");
            return "Exception";
        }

        //private NetMQMonitor monitor;
        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>返回状态,其中-1会导致重连</returns>
        protected abstract int DoWork(TSocket socket);

        /// <summary>
        /// 生成SOCKET对象
        /// </summary>
        /// <remarks>版本不兼容导致改变</remarks>
        protected abstract TSocket CreateSocket();

        /// <summary>
        ///     执行处理泵
        /// </summary>
        /// <returns></returns>
        protected void RunPump()
        {
            //登记线程开始
            RpcEnvironment.set_command_thread_start();
            //monitor?.Stop();
            int state = 0;
            Console.WriteLine($"命令请求泵({ZmqAddress})正在启动{this.GetType().Name}");
            using (var socket = CreateSocket())
            {
                //monitor = new NetMQMonitor(socket, $"inproc://pump_{Guid.NewGuid()}.rep", SocketEvents.All);
                OptionSocktet(socket);
                socket.Connect(ZmqAddress);
                OnConnected(socket);

                Console.WriteLine($"命令请求泵({ZmqAddress})已启动{this.GetType().Name}");

                while (m_state == 4 && RpcEnvironment.NetState == ZmqNetStatus.Runing)
                {
                    state = DoWork(socket);
                    if (state == -1)
                    {
                        Console.WriteLine($"命令请求泵({ZmqAddress})正在错误状态{this.GetType().Name}");
                        break;
                    }
                }
                Console.WriteLine($"命令请求泵({ZmqAddress})正在关闭{this.GetType().Name}");
                try
                {
                    OnDeConnected(socket);
                }
                catch (Exception e)
                {
                }
                try
                {
                    socket.Disconnect(ZmqAddress);
                }
                catch (Exception e)
                {
                }
            }
            Console.WriteLine($"命令请求泵({ZmqAddress})已关闭{this.GetType().Name}");
            //登记线程关闭
            RpcEnvironment.set_command_thread_end();
            thread = null;
            OnTaskEnd(state);
        }

        /// <summary>
        /// 当任务结束时调用(基类实现为异常时重新执行)
        /// </summary>
        /// <param name="state">状态</param>
        protected virtual void OnTaskEnd(int state)
        {
            if (m_state == 5 || state != -1 || RpcEnvironment.NetState != ZmqNetStatus.Runing)
            {
                m_state = 6;
                return;
            }
            m_state = 6;
            m_restart++;
            DoRun();
        }
        #endregion
    }
}