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
        public abstract bool Run();

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="station"></param>
        public static void Run(ZeroStation station)
        {
            station.Close();
            station.Config = StationProgram.GetConfig(station.StationName);
            if (station.Config == null)
            {
                StationProgram.WriteError($"{station.StationName} not find,try install...");
                StationProgram.InstallApiStation(station.StationName);
                return;
            }
            station.Run();
        }
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected void OnTaskStop(Task task)
        {
            while (InPoll)
                Thread.Sleep(50);
            if (StationProgram.State == StationState.Run && (RunState == StationState.Failed || RunState == StationState.Start))
            {
                StationProgram.WriteInfo($"【{StationName}】restart...");
                Console.CursorLeft = 0;
                StationProgram.WriteInfo("                       ");
                for (int i = 1; i <= 3; i++)
                {
                    Console.CursorLeft = 0;
                    Console.Write($"{i}s");
                    Thread.Sleep(1000);
                }
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
            if (RunState != StationState.Run)
                return true;
            StationProgram.WriteInfo($"{StationName} closing....");
            RunState = StationState.Closing;
            do
            {
                Thread.Sleep(20);
            } while (RunState != StationState.Closed);
            RunState = StationState.Closed;
            StationProgram.WriteInfo($"{StationName} closed");
            return true;
        }

    }
}