using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点
    /// </summary>
    public abstract class ZeroStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="type"></param>
        protected ZeroStation(int type)
        {
            StationType = type;
        }
        /// <summary>
        /// 调度器
        /// </summary>
        public const int StationTypeDispatcher = 1;
        /// <summary>
        /// 监视器
        /// </summary>
        public const int StationTypeMonitor = 2;
        /// <summary>
        /// API
        /// </summary>
        public const int StationTypeApi = 3;
        /// <summary>
        /// 投票器
        /// </summary>
        public const int StationTypeVote = 4;
        /// <summary>
        /// 广播
        /// </summary>
        public const int StationTypePublish = 5;

        /// <summary>
        /// 类型
        /// </summary>
        public int StationType { get; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        private string _realName;

        /// <summary>
        /// 节点名称
        /// </summary>
        protected string Name { get; set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        public string RealName => _realName ?? (_realName = ZeroIdentityHelper.CreateRealName(Config?.ShortName ?? StationName, Name));
        /// <summary>
        /// 实例名称
        /// </summary>
        public byte[] Identity => ZeroIdentityHelper.ToZeroIdentity(Config?.ShortName ?? StationName, Name);

        /// <summary>
        /// 站点配置
        /// </summary>
        public StationConfig Config { get; set; }

        /// <summary>
        ///     运行状态
        /// </summary>
        private int _state;

        /// <summary>
        ///     运行状态
        /// </summary>
        public int State
        {
            get => _state;
            set => Interlocked.Exchange(ref _state, value);
        }

        /// <summary>
        /// 正在侦听的状态开关
        /// </summary>
        protected bool InPoll { get; set; }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected abstract bool Run();

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="station"></param>
        public static bool Run(ZeroStation station)
        {
            //执行状态，会自动处理
            if (station.State >= StationState.Start && station.State <= StationState.Pause)
            {
                return false;
            }
            if (station.Config == null)
            {
                station.Config = ZeroApplication.GetConfig(station.StationName, out _);
                //if (station.Config == null)
                //{
                //    string type;
                //    switch (station.StationType)
                //    {
                //        case StationTypeApi:
                //            type = "api";
                //            break;
                //        case StationTypePublish:
                //            type = "pub";
                //            break;
                //        case StationTypeVote:
                //            type = "vote";
                //            break;
                //        default:
                //            StationProgram.WriteError($"[{station.StationName}]type no supper,failed!");
                //            return;
                //    }
                //    StationProgram.WriteError($"[{station.StationName}]not find,try install...");
                //    StationProgram.InstallStation(station.StationName, type);
                //    return;
                //}
                if (station.Config == null)
                {
                    station.State = StationState.ConfigError;
                    StationConsole.WriteError(station.StationName, "config can`t load");
                    return false;
                }
            }
            SystemMonitor.StationEvent += station.SystemMonitor_StationEvent;
            return station.Run();
        }

        private void SystemMonitor_StationEvent(object sender, SystemMonitor.StationEventArgument e)
        {
            Heartbeat(false);
        }
        /// <summary>
        /// 心跳
        /// </summary>
        protected void Heartbeat(bool left)
        {
            if (left)
                SystemManager.HeartLeft(Config.StationName, RealName);
            else
                SystemManager.Heartbeat(Config.StationName, RealName);
        }
        /// <summary>
        ///     尝试重启
        /// </summary>
        /// <returns></returns>
        protected bool TryRun()
        {
            StationConsole.WriteInfo(RealName, "restart...");
            Thread.Sleep(1000);
            Task.Factory.StartNew(Run);
            return true;
        }
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected virtual void OnTaskStop()
        {
            if (ZeroApplication.CanDo && State == StationState.Failed)
            {
                State = StationState.None;
                TryRun();
                return;
            }
            Heartbeat(true);
            State = StationState.Closed;
            StationConsole.WriteInfo(RealName, "end");
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            //未运行状态
            if (State < StationState.Start || State > StationState.Pause)
                return true;
            StationConsole.WriteInfo(StationName, "closing....");
            State = StationState.Closing;
            do
            {
                Thread.Sleep(20);
            } while (State != StationState.Closed);

            StationConsole.WriteInfo(StationName, "closed");
            return true;
        }

    }
}