using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Gboxt.Common.DataModel;
using NetMQ;
using NetMQ.Sockets;

namespace ZmqNet.Rpc.Core.ZeroNet
{

    /// <summary>
    /// Api站点
    /// </summary>
    public class ApiStation
    {
        RequestSocket socket;
        /// <summary>
        /// 站点名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 实例名称
        /// </summary>
        private string _realName;

        /// <summary>
        /// 实例名称
        /// </summary>
        public string RealName => _realName ?? (_realName = StationName + "-" + RandomOperate.Generate(5));

        /// <summary>
        /// 站点配置
        /// </summary>
        public StationConfig Config { get; set; }


        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="station"></param>
        public static void Run(ApiStation station)
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
        public Func<List<string>, string> ExecFunc { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string ExecCommand(List<string> args)
        {
            if (ExecFunc == null)
            {
                return "{\"Result\":false,\"Message\":\"NoSupper\",\"ErrorCode\":0}";
            }
            try
            {
                return ExecFunc(args);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return "{\"Result\":false,\"Message\":\"InnerError\",\"ErrorCode\":-3}";
            }
        }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void OnTaskStop(Task task)
        {
            if (StationProgram.State != StationState.Run)
                return;
            while (inHeart || inPoll)
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
            bool restart = false;
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
        public bool Run()
        {
            lock(this)
            {
                if (RunState != StationState.Run)
                    RunState = StationState.Start;
                if (socket != null)
                    return false;
            }
            inPoll = false;
            inHeart = false;
            
            socket = new RequestSocket();
            try
            {
                socket.Options.Identity = RealName.ToAsciiBytes();
                socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
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
            try
            {
                var word = socket.Request("@", "");
                if (word != "wecome")
                {
                    RunState = StationState.Failed;
                    StationProgram.WriteLine($"【{StationName}】proto error");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                RunState = StationState.Failed;
                CloseSocket();
                StationProgram.WriteLine($"【{StationName}】request error =>{e.Message}");
                return false;
            }

            RunState = StationState.Run;

            var task1 = Task.Factory.StartNew(PollTask);
            task1.ContinueWith(OnTaskStop);

            var task2 = Task.Factory.StartNew(HeartbeatTask);
            task2.ContinueWith(OnTaskStop);

            while (!inHeart || !inPoll)
                Thread.Sleep(50);
            StationProgram.WriteLine($"【{StationName}:{RealName}】runing...");
            return true;
        }


        private bool inHeart, inPoll;
        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        private void HeartbeatTask()
        {
            inHeart = true;
            int errorCount = 0;
            StationProgram.WriteLine($"【{StationName}】heartbeat start");
            try
            {
                while (RunState == StationState.Run)
                {
                    Thread.Sleep(5000);
                    string result = Config.HeartAddress.RequestNet("@", RealName);
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        if (++errorCount > 3)
                        {
                            RunState = StationState.Failed;
                        }
                        StationProgram.WriteLine($"【{StationName}】heartbeat error{errorCount}...");
                    }
                    else if(errorCount > 0)
                    {
                        errorCount = 0;
                        StationProgram.WriteLine($"【{StationName}】heartbeat resume...");
                    }
                }
                //退出
                Config.HeartAddress.RequestNet("-", RealName);
            }
            catch (Exception e)
            {
                StationProgram.WriteLine($"【{StationName}】heartbeat error{e.Message}...");
                RunState = StationState.Failed;
                LogRecorder.Exception(e);
            }
            StationProgram.WriteLine($"【{StationName}】heartbeat stop");
            inHeart = false;
        }
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void PollTask()
        {
            socket.SendString("*","*");
            inPoll = true;
            StationProgram.WriteLine($"【{StationName}】poll start");
            //var timeout = new TimeSpan(0, 0, 5);
            try
            {
                while (RunState == StationState.Run)
                {
                    //if (!socket.Poll(timeout))
                    //    continue;
                    //if (!socket.HasIn)
                    //    continue;
                    //string caller = socket.ReceiveFrameString();
                    //string command = socket.ReceiveFrameString();
                    //string argument = socket.ReceiveFrameString();
                    //string response = ExecCommand(command, argument);
                    //socket.SendMoreFrame(caller);
                    List<string> arg;
                    if (!socket.ReceiveString(out arg, 0))
                    {
                        continue;
                    }
                    string response = ExecCommand(arg);
                    socket.SendMoreFrame(arg[0]);
                    socket.SendFrame(response);
                    StationProgram.WriteLine($"【{StationName}】call {arg.LinkToString(",")}=>{response}");
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
        /// <summary>
        /// 运行状态
        /// </summary>
        public StationState RunState;

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            if (RunState == StationState.Run)
            {
                RunState = StationState.Closing;
                Thread.Sleep(100);
            }
            RunState = StationState.Closed;
            return true;
        }
    }
}