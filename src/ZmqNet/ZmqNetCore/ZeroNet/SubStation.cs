using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 消息订阅站点
    /// </summary>
    public class SubStation: ZeroStation
    {
        /// <summary>
        /// 订阅主题
        /// </summary>
        public string Subscribe { get; set; } = "";

        private SubscriberSocket socket;
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="station"></param>
        public static void Run(SubStation station)
        {
            station.Close();
            station.Config = StationProgram.GetConfig(station.StationName);
            if (station.Config == null)
            {
                StationProgram.WriteLine($"{station.StationName} not find,try install...");
                StationProgram.InstallApiStation(station.StationName);
                return;
            }
            station.Run();
        }

        /// <summary>
        /// 命令处理方法 
        /// </summary>
        public Action<string> ExecFunc { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual void ExecCommand(string args)
        {
            ExecFunc?.Invoke(args);
        }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void OnTaskStop(Task task)
        {
            if (StationProgram.State != StationState.Run)
                return;
            while (inPoll)
                Thread.Sleep(500);
            OnStop();
        }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void OnStop()
        {
            if (StationProgram.State != StationState.Run)
            {
                return;
            }
            bool restart;
            lock (this)
            {
                restart = RunState == StationState.Failed || RunState == StationState.Start;
            }
            if (!restart)
                return;
            StationProgram.WriteLine($"【{StationName}】restart...");
            if (Run())
                return;
            Thread.Sleep(1000);
            OnStop();
        }
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        public sealed override bool Run()
        {
            lock (this)
            {
                if (RunState != StationState.Run)
                    RunState = StationState.Start;
                if (socket != null)
                    return false;
            }
            inPoll = false;

            socket = new SubscriberSocket();
            try
            {
                socket.Options.Identity = RealName.ToAsciiBytes();
                socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                socket.Subscribe(Subscribe);
                socket.Connect(Config.InnerAddress);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                RunState = StationState.Failed;
                CloseSocket();
                StationProgram.WriteLine($"【{StationName}】connect error =>{e.Message}");
                return false;
            }
            RunState = StationState.Run;
            var task1 = Task.Factory.StartNew(PollTask);
            task1.ContinueWith(OnTaskStop);
            while (!inPoll)
                Thread.Sleep(50);
            StationProgram.WriteLine($"【{StationName}:{RealName}】runing...");
            return true;
        }


        private bool inPoll;

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void PollTask()
        {
            inPoll = true;
            StationProgram.WriteLine($"【{StationName}】poll start");
            var timeout = new TimeSpan(0, 0, 5);
            try
            {
                while (RunState == StationState.Run)
                {
                    string arg;
                    if (!socket.TryReceiveFrameString(timeout, out arg))
                    {
                        continue;
                    }
                    ExecCommand(arg);
                }
            }
            catch (Exception e)
            {
                StationProgram.WriteLine($"【{StationName}】poll error{e.Message}...");
                LogRecorder.Exception(e);
                RunState = StationState.Failed;
            }
            inPoll = false;
            StationProgram.WriteLine($"【{StationName}】poll stop");

            CloseSocket();
        }

        void CloseSocket()
        {
            lock (this)
            {
                if (socket == null)
                    return;
                try
                {
                    socket.Disconnect(Config.InnerAddress);
                }
                catch (Exception)
                {
                    //LogRecorder.Exception(e);//一般是无法连接服务器而出错
                }
                socket.Close();
                socket = null;
            }
        }
    }
}