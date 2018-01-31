using System.Threading;
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
                StationProgram.WriteLine($"{station.StationName} not find,try install...");
                StationProgram.InstallApiStation(station.StationName);
                return;
            }
            station.Run();
        }
    }
}