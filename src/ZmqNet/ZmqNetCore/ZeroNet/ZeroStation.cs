using System;
using System.Threading;
using System.Threading.Tasks;
using Gboxt.Common.DataModel;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点
    /// </summary>
    public abstract class ZeroStation
    {
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
        public abstract int StationType { get; }
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
        /// 运行状态
        /// </summary>
        public StationState RunState;

        /// <summary>
        /// 正在侦听的状态开关
        /// </summary>
        protected bool InPoll { get; set; }

        /// <summary>
        /// 心跳的存活状态开关
        /// </summary>
        protected bool InHeart { get; set; }
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
            if (station.RunState >= StationState.Start && station.RunState <= StationState.Failed)
            {
                return false;
            }
            if (station.Config == null)
            {
                station.Config = StationProgram.GetConfig(station.StationName, out _);
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
                //            StationProgram.WriteError($"【{station.StationName}】type no supper,failed!");
                //            return;
                //    }
                //    StationProgram.WriteError($"【{station.StationName}】not find,try install...");
                //    StationProgram.InstallStation(station.StationName, type);
                //    return;
                //}
                if (station.Config == null)
                {
                    station.RunState = StationState.ConfigError;
                    StationConsole.WriteError($"【{station.StationName}】config cann`t get,failed!");
                    return false;
                }
            }
            return station.Run();
        }
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected virtual void OnTaskStop()
        {
            while (InPoll)
                Thread.Sleep(50);
            if (StationProgram.State == StationState.Run && RunState == StationState.Failed)
            {
                StationConsole.WriteInfo($"【{StationName}】restart...");
                Console.CursorLeft = 0;
                StationConsole.WriteInfo("                       ");
                for (int i = 1; i <= 3; i++)
                {
                    Console.CursorLeft = 0;
                    Console.Write($"{i}s");
                    Thread.Sleep(1000);
                }
                RunState = StationState.None;
                Run();
                return;
            }
            if (RunState == StationState.Closing)
                RunState = StationState.Closed;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            //未运行状态
            if (RunState < StationState.Start || RunState > StationState.Failed)
                return true;
            StationConsole.WriteInfo($"{StationName} closing....");
            RunState = StationState.Closing;
            do
            {
                Thread.Sleep(20);
            } while (RunState != StationState.Closed);

            StationConsole.WriteInfo($"{StationName} closed");
            return true;
        }

    }
}