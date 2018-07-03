using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class SystemManager : ZeroManageCommand
    {
        #region 实例

        /// <summary>
        /// 构造
        /// </summary>
        public static SystemManager CreateInstance()
        {
           return Instance = new SystemManager();
        }

        /// <summary>
        /// 构造
        /// </summary>
        private SystemManager()
        {
            ManageAddress = ZeroApplication.Config.ZeroManageAddress;
        }
        /// <summary>
        /// 单例
        /// </summary>
        public static SystemManager Instance { get; set; }

        #endregion

        #region 心跳

        /// <summary>
        ///     连接到
        /// </summary>
        internal  bool PingCenter()
        {
            return ByteCommand(ZeroByteCommand.Ping);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        internal  bool HeartLeft()
        {
            return HeartLeft("SystemManage", ZeroApplication.Config.RealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public  bool HeartReady()
        {
            return HeartReady("SystemManage", ZeroApplication.Config.RealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        internal  bool HeartJoin()
        {
            return HeartJoin("SystemManage", ZeroApplication.Config.RealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        internal  bool Heartbeat()
        {
            return Heartbeat("SystemManage", ZeroApplication.Config.RealName);
        }
        /// <summary>
        ///     连接到
        /// </summary>
        public  bool HeartLeft(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && HeartCommand(ZeroByteCommand.HeartLeft, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public  bool HeartReady(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && HeartCommand(ZeroByteCommand.HeartReady, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public  bool HeartJoin(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && HeartCommand(ZeroByteCommand.HeartJoin, station, realName, ZeroApplication.Config.LocalIpAddress);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public  bool Heartbeat(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && HeartCommand(ZeroByteCommand.HeartPitpat, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        private  bool HeartCommand(byte commmand, params string[] args)
        {
            Task.Factory.StartNew(() => ByteCommand(commmand, args)).Wait();
            return true;
        }

        #endregion

        #region 系统支持

        /// <summary>
        /// 尝试安装站点
        /// </summary>
        /// <param name="station"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public  bool TryInstall(string station, string type)
        {
            if (ZeroApplication.Config.TryGetConfig(station, out _))
                return true;
            ZeroTrace.WriteInfo(station, "No find,try install ...");
            var r = CallCommand("install", type, station, station);
            if (!r.InteractiveSuccess || r.State == ZeroOperatorStateType.NotSupport)
            {
                ZeroTrace.WriteError(station, "Test install failed");
                return false;
            }

            if (r.State != ZeroOperatorStateType.Ok && r.TryGetValue(ZeroFrameType.Status, out var json))
            {
                ZeroApplication.Config.UpdateConfig(station, json, out _);
            }
            ZeroTrace.WriteInfo(station, "Is install ,try start it ...");
            r = CallCommand("start", station);
            if (!r.InteractiveSuccess && r.State != ZeroOperatorStateType.Ok && r.State != ZeroOperatorStateType.Runing)
            {
                ZeroTrace.WriteError(station, "Can't start station");
                return false;
            }
            LoadConfig(station);
            ZeroTrace.WriteInfo(station, "Station runing");
            return true;
        }

        /// <summary>
        ///     上传文档
        /// </summary>
        /// <returns></returns>
        public bool UploadDocument()
        {
            bool success = true;
            foreach (var doc in ZeroApplication.Config.Documents.Values)
            {
                var result = CallCommand("doc", doc.Name, JsonConvert.SerializeObject(doc));
                if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                {
                    ZeroTrace.WriteError("UploadDocument", result);
                    success = false;
                }
            }
            return success;
        }

        /// <summary>
        ///     下载文档
        /// </summary>
        /// <returns></returns>
        public bool LoadDocument(string name,out StationDocument doc)
        {
            ZeroResultData result;
            try
            {
                result = CallCommand("doc", name);
                if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                {
                    ZeroTrace.WriteError("LoadDocument", result);
                    doc = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("LoadDocument", e);
                doc = null;
                return false;
            }
            if (!result.TryGetValue(ZeroFrameType.Status, out var json))
            {
                ZeroTrace.WriteError("LoadDocument", "Empty");
                doc = null;
                return false;
            }
            try
            {
                doc = JsonConvert.DeserializeObject<StationDocument>(json);
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("LoadDocument", e, json);
                doc = null;
                return false;
            }
        }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public  bool LoadAllConfig()
        {
            var result = CallCommand("host", "*");
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
            {
                ZeroTrace.WriteError("LoadConfig", result);
                return false;
            }
            if (!result.TryGetValue(ZeroFrameType.Status, out var json))
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
        internal  StationConfig LoadConfig(string stationName)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                ZeroTrace.WriteError("LoadConfig", "No ready");
                return null;
            }
            var result = CallCommand("host", stationName);
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
            {
                ZeroTrace.WriteError("LoadConfig", result);
                return null;
            }

            var json = result.GetValue(ZeroFrameType.Status);
            if (json == null || json[0] != '{')
            {
                ZeroTrace.WriteError("LoadConfig", stationName, "not a json", json);
                return null;
            }

            if (ZeroApplication.Config.UpdateConfig(stationName, json, out var config))
            {
                return config;
            }
            return null;
        }


        #endregion
    }
}