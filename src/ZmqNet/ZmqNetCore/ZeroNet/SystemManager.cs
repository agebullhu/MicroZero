using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class SystemManager
    {
        /// <summary>
        /// 系统状态
        /// </summary>
        public static StationState State {get; internal set; }
        /// <summary>
        ///     执行管理命令
        /// </summary>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static bool Request(string commmand, string argument)
        {
            if (State != StationState.Run)
                return false;
            var result = ZeroApplication.ZeroManageAddress.RequestNet(commmand, argument);
            if (String.IsNullOrWhiteSpace(result))
            {
                StationConsole.WriteError($"【{commmand}】{argument}\r\n处理超时");
                return false;
            }

            StationConsole.WriteLine(result);
            return true;
        }

        /// <summary>
        ///     安装站点
        /// </summary>
        /// <returns></returns>
        public static StationConfig InstallStation(string stationName, string type)
        {
            StationConfig config;
            lock (ZeroApplication.Configs)
            {
                if (ZeroApplication.Configs.TryGetValue(stationName, out config))
                    return config;
            }

            if (State != StationState.Run)
                return null;
            StationConsole.WriteInfo($"【{stationName}】auto regist...");
            try
            {
                var result = ZeroApplication.ZeroManageAddress.RequestNet("install", type, stationName);

                switch (result)
                {
                    case null:
                        StationConsole.WriteError($"【{stationName}】auto regist failed");
                        return null;
                    case ZeroNetStatus.ZeroCommandNoSupport:
                        StationConsole.WriteError($"【{stationName}】auto regist failed:type no supper");
                        return null;
                    case ZeroNetStatus.ZeroCommandFailed:
                        StationConsole.WriteError($"【{stationName}】auto regist failed:config error");
                        return null;
                }
                config = JsonConvert.DeserializeObject<StationConfig>(result);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError($"【{stationName}】auto regist failed:{e.Message}");
                return null;
            }
            lock (ZeroApplication.Configs)
            {
                ZeroApplication.Configs.Add(stationName, config);
            }

            StationConsole.WriteError($"【{stationName}】auto regist succeed");
            return config;
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static void Run()
        {
            State = StationState.Start;
            StationConsole.WriteInfo($"System Manage({ZeroApplication.ZeroManageAddress}) Start...");
            
            if (!PingCenter())
            {
                State = StationState.Failed;
                StationConsole.WriteError("ZeroCenter can`t connection.");
                return;
            }
            if (!LoadAllConfig())
            {
                State = StationState.Failed;
                StationConsole.WriteError("ZeroCenter configs can`t load.");
                return;
            }
            State = StationState.Run;
            Thread.Sleep(50);
            ZeroApplication.State = StationState.Run;
            if (ZeroApplication.Stations.Count > 0)
            {
                foreach (var station in ZeroApplication.Stations.Values)
                    ZeroStation.Run(station);
            }

            SystemMonitor.RaiseEvent(ZeroApplication.Config,"program_run");
            StationConsole.WriteInfo("System Manage Run...");
        }

        /// <summary>
        ///     连接到
        /// </summary>
        private static bool PingCenter()
        {
            try
            {
                return ZeroApplication.ZeroManageAddress.RequestNet("ping") != null;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError(e.Message);
                return false;
            }
        }
        static bool LoadAllConfig()
        {
            string result;
            int trycnt = 0;
            while (true)
            {
                result = ZeroApplication.ZeroManageAddress.RequestNet("Host", "*");
                if (result != null)
                {
                    break;
                }
                if (++trycnt > 5)
                    return false;
                Thread.Sleep(10);
            }
            try
            {
                var configs = JsonConvert.DeserializeObject<List<StationConfig>>(result);
                foreach (var config in configs)
                {
                    lock (ZeroApplication.Configs)
                    {
                        if (ZeroApplication.Configs.ContainsKey(config.StationName))
                            ZeroApplication.Configs[config.StationName].Copy(config);
                        else
                            ZeroApplication.Configs.Add(config.StationName, config);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError(e.Message);
                return false;
            }

        }
    }
}