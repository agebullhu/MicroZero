using System;
using System.Threading;
using System.Threading.Tasks;
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
        public static bool HeartReady()
        {
            return HeartReady("SystemManage", ZeroApplication.Config.RealName);
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
        public static bool HeartLeft(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && HeartCommand(ZeroByteCommand.HeartLeft, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public static bool HeartReady(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && HeartCommand(ZeroByteCommand.HeartReady, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public static bool HeartJoin(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && HeartCommand(ZeroByteCommand.HeartJoin, station, realName, ZeroApplication.Config.LocalIpAddress);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public static bool Heartbeat(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && HeartCommand(ZeroByteCommand.HeartPitpat, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        private static bool HeartCommand(byte commmand, params string[] args)
        {
            Task.Factory.StartNew(()=> ByteCommand(commmand, args)).Wait();
            return true;
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
            var re = CallCommand("host", "*");
            if (!re.InteractiveSuccess)
            {
                ZeroTrace.WriteError("LoadConfig", "Network failed");
                return false;
            }
            if (!re.TryGetValue(ZeroFrameType.TextValue, out var json))
            {
                ZeroTrace.WriteError("LoadAllConfig", "Empty");
                return false;
            }
            return ZeroApplication.Config.FlushConfigs(json);
        }


        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        internal static StationConfig LoadConfig(string stationName)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                return null;
            }
            var result = CallCommand("host", stationName);
            switch (result.State)
            {
                case ZeroOperatorStateType.Ok:
                    var json = result.GetValue(ZeroFrameType.TextValue);
                    if (json == null || json[0] != '{')
                    {
                        ZeroTrace.WriteError("GetConfig", stationName, "not a json", json);
                        return null;
                    }
                    try
                    {
                        return JsonConvert.DeserializeObject<StationConfig>(json);
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e, json);
                        ZeroTrace.WriteError("GetConfig", stationName, "not a json", json);
                        return null;
                    }
                case ZeroOperatorStateType.NoFind:
                    ZeroTrace.WriteError("GetConfig", stationName, "NoFind");
                    return null;
                default:
                    ZeroTrace.WriteError("GetConfig", stationName, result.State.ToString());
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

        private static ZSocket _socket;

        private static readonly object  LockObj = new object();
        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="description"></param>
        /// <param name="args">请求参数</param>
        /// <returns></returns>
        private static ZeroResultData<string> CallCommand(byte[] description, params string[] args)
        {
            lock (LockObj)
            {
                if (_socket == null)
                    _socket = ZSocket.CreateRequestSocket(ZeroApplication.Config.ZeroManageAddress);
                try
                {
                    var result = _socket.SendTo(description, args);
                    if (!result.InteractiveSuccess)
                    {
                        _socket.TryClose();
                        _socket = null;
                        return result;
                    }

                    result = _socket.ReceiveString();
                    if (result.InteractiveSuccess)
                        return result;
                    _socket.TryClose();
                    _socket = null;
                    return result;
                }
                catch (Exception e)
                {
                    _socket?.TryClose();
                    _socket = null;
                    return new ZeroResultData<string>
                    {
                        InteractiveSuccess = false,
                        Exception = e
                    };
                }
            }
        }
        

        #endregion

        /// <summary>
        /// 关闭仅有的一个连接
        /// </summary>
        internal static void Destroy()
        {
            _socket?.Close();
            _socket = null;
        }
    }
}