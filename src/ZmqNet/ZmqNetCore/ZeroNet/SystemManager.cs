using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class SystemManager
    {
        #region 系统运行

        /// <summary>
        /// 系统状态
        /// </summary>
        public static StationState State { get; internal set; }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static void Run()
        {
            if (ZeroApplication.State >= StationState.Closing)
                return;
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

            HeartJoin();

            SystemMonitor.RaiseEvent(ZeroApplication.Config, "program_run");
            StationConsole.WriteInfo("System Manage Run...");
        }

        #endregion

        #region 心跳

        /// <summary>
        ///     连接到
        /// </summary>
        private static bool PingCenter()
        {
            return ByteCommand(ZeroByteCommand.Ping);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        internal static bool HeartLeft()
        {
            return HeartLeft("SystemManage", ZeroApplication.Config.RealName);
        }


        /// <summary>
        ///     连接到
        /// </summary>
        internal static bool HeartJoin()
        {
            return HeartJoin("SystemManage", ZeroApplication.Config.RealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        internal static bool Heartbeat()
        {
            return Heartbeat("SystemManage", ZeroApplication.Config.RealName);
        }
        /// <summary>
        ///     连接到
        /// </summary>
        internal static bool HeartLeft(string station, string realName)
        {
            return ByteCommand(ZeroByteCommand.HeartLeft, station, realName);
        }


        /// <summary>
        ///     连接到
        /// </summary>
        internal static bool HeartJoin(string station, string realName)
        {
            return ByteCommand(ZeroByteCommand.HeartJoin, station, realName, ZeroApplication.Address);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        internal static bool Heartbeat(string station, string realName)
        {
            return ByteCommand(ZeroByteCommand.HeartPitpat, station, realName);
        }

        /// <summary>
        ///     执行管理命令(快捷命令，命令在说明符号的第二个字节中)
        /// </summary>
        /// <param name="commmand"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool ByteCommand(byte commmand, params string[] args)
        {
            byte[] description = new byte[4 + args.Length];
            description[0] = (byte)(args.Length);
            description[1] = commmand;
            int idx = 2;
            for (var index = 0; index < args.Length; index++)
            {
                description[idx++] = ZeroFrameType.Argument;
            }
            description[idx] = ZeroFrameType.End;
            return CallCommand(description,args).InteractiveSuccess;
        }
        #endregion

        #region 命令支持


        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static bool LoadAllConfig()
        {
            ZeroResultData<string> re;
            int trycnt = 0;
            while (true)
            {
                re = CallCommand("Host", "*");
                if (re.InteractiveSuccess)
                {
                    break;
                }

                if (++trycnt > 5)
                {
                    StationConsole.WriteError("无法读取站点*配置");
                    return false;
                }
                Thread.Sleep(10);
            }

            if (!re.TryGetValue(ZeroFrameType.TextValue, out var json))
            {
                StationConsole.WriteError("服务器无站点数据");
                return false;
            }
            try
            {
                var configs = JsonConvert.DeserializeObject<List<StationConfig>>(json);
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
                StationConsole.WriteError($"服务器站点数据异常{e.Message}\r\n{json}");
                return false;
            }
        }


        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig GetConfig(string stationName, out ZeroCommandStatus status)
        {
            var result = CallCommand("host", stationName);
            switch (result.State)
            {
                case ZeroStateType.Ok:
                    var json = result.GetValue(ZeroFrameType.TextValue);
                    if (json == null || json[0] != '{')
                    {
                        status = ZeroCommandStatus.ValueError;
                        return null;
                    }
                    try
                    {
                        status = ZeroCommandStatus.Success;
                        return JsonConvert.DeserializeObject<StationConfig>(json);
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e, json);
                        StationConsole.WriteError($"取配置时：{e.Message}\r\n{json}");
                        status = ZeroCommandStatus.ValueError;
                        return null;
                    }
                case ZeroStateType.NoFind:
                    StationConsole.WriteError($"[{stationName}]未安装");
                    status = ZeroCommandStatus.NoFind;
                    return null;
                default:
                    StationConsole.WriteError($"[{stationName}]无法获取配置");
                    status = ZeroCommandStatus.Error;
                    return null;
            }

        }


        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="args">请求参数,第一个必须为命令名称</param>
        /// <returns></returns>
        public static ZeroResultData<string> CallCommand(params string[] args)
        {
            byte[] description = new byte[3 + args.Length];
            description[0] = (byte)(args.Length);
            description[2] = ZeroFrameType.Command;
            int idx = 3;
            for (var index = 1; index < args.Length; index++)
            {
                description[idx++] = ZeroFrameType.Argument;
            }
            description[idx] = ZeroFrameType.End;
            return CallCommand(description,args);
        }
        private static int _id;
        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="description"></param>
        /// <param name="args">请求参数</param>
        /// <returns></returns>
        private static ZeroResultData<string> CallCommand(byte[] description, params string[] args)
        {
            using (var request = new RequestSocket())
            {
                request.Options.Identity = ZeroIdentityHelper.ToZeroIdentity((++_id).ToString("X"));
                request.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                request.Connect(ZeroApplication.ZeroManageAddress);

                var result = SendCommand(request, description, args);
                if (!result.InteractiveSuccess)
                {
                    request.Disconnect(ZeroApplication.ZeroManageAddress);
                    return result;
                }

                result = request.ReceiveString();
                request.Disconnect(ZeroApplication.ZeroManageAddress);
                return result;
            }
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static ZeroResultData<string> SendCommand(NetMQSocket request, byte[] description, params string[] args)
        {
            var result = new ZeroResultData<string>();
            TimeSpan timeout = new TimeSpan(0, 0, 0, 1);
            try
            {
                if (!request.TrySendFrame(timeout, description, args.Length > 0))
                {
                    result.State = ZeroStateType.LocalSendError;
                    return result;
                }

                if (args.Length <= 0)
                {
                    result.InteractiveSuccess = true;
                    result.State = ZeroStateType.Ok;
                    return result;
                }

                var i = 0;
                for (; i < args.Length - 1; i++)
                {
                    if (request.TrySendFrame(timeout, args[i] ?? "", true))
                        continue;
                    result.State = ZeroStateType.LocalSendError;
                    return result;
                }

                if (request.TrySendFrame(timeout, args[i] ?? ""))
                {
                    result.InteractiveSuccess = true;
                    result.State = ZeroStateType.Ok;
                    return result;
                }
                result.State = ZeroStateType.LocalSendError;
                return result;
            }
            catch (Exception e)
            {
                StationConsole.WriteError(e.Message);
                result.State = ZeroStateType.LocalSendError;
                result.Exception = e;
                return result;
            }
        }

        #endregion
    }
}