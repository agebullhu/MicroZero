using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Logging;
using ZeroMQ;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class SystemManager
    {
        #region 心跳

        /// <summary>
        ///     连接到
        /// </summary>
        internal static bool PingCenter()
        {
            return ByteCommand(ZeroByteCommand.Ping) || ByteCommand(ZeroByteCommand.Ping);
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
            return ByteCommand(ZeroByteCommand.HeartJoin, station, realName, ZeroApplication.LocalIpAddress);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        internal static bool Heartbeat(string station, string realName)
        {
            return true;//ByteCommand(ZeroByteCommand.HeartPitpat, station, realName);
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
            return CallCommand(description, args).InteractiveSuccess;
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
                re = CallCommand("host", "*");
                if (re.InteractiveSuccess)
                {
                    break;
                }

                if (++trycnt > 5)
                {
                    ZeroTrace.WriteError("读取站点配置", "服务器无响应");
                    return false;
                }
                Thread.Sleep(10);
            }

            if (!re.TryGetValue(ZeroFrameType.TextValue, out var json))
            {
                ZeroTrace.WriteError("ZeroApplication", "LoadAllConfig", "Empty");
                return false;
            }
            ZeroTrace.WriteInfo("ZeroApplication", "LoadAllConfig", json);
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
                ZeroTrace.WriteError("读取站点配置", "Exception", json,e);
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
                case ZeroOperatorStateType.Ok:
                    var json = result.GetValue(ZeroFrameType.TextValue);
                    if (json == null || json[0] != '{')
                    {
                        ZeroTrace.WriteError("GetConfig", stationName,"not a json", json);
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
                        ZeroTrace.WriteError("GetConfig", stationName, "not a json", json);
                        status = ZeroCommandStatus.ValueError;
                        return null;
                    }
                case ZeroOperatorStateType.NoFind:
                    ZeroTrace.WriteError("GetConfig", stationName, "NoFind");
                    status = ZeroCommandStatus.NoFind;
                    return null;
                default:
                    ZeroTrace.WriteError("GetConfig", stationName, result.State.ToString());
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
            return CallCommand(description, args);
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
            var socket = ZeroHelper.CreateRequestSocket(ZeroApplication.ZeroManageAddress,
                ZeroIdentityHelper.ToZeroIdentity((++_id).ToString("X")));
            if (socket == null)
                return new ZeroResultData<string>
                {
                    State = ZeroOperatorStateType.Error,
                    ZmqErrorMessage = "con't creat socket"
                };
            using (socket)
            {
                var result = SendCommand(socket, description, args);
                if (!result.InteractiveSuccess)
                {
                    socket.CloseSocket();
                    return result;
                }

                result = socket.ReceiveString();
                socket.CloseSocket();
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
        static ZeroResultData<string> SendCommand(ZSocket request, byte[] description, params string[] args)
        {
            return request.Send(description, args);
        }

        #endregion
    }
}