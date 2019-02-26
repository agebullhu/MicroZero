using Agebull.Common.ApiDocuments;
using Newtonsoft.Json;
using System;
using Agebull.Common.Rpc;

namespace Agebull.ZeroNet.Core.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class SystemManager : HeartManager
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
        /// 地址错误的情况
        /// </summary>
        /// <returns></returns>
        protected sealed override string GetAddress()
        {
            return ZeroApplication.Config.ZeroManageAddress;
        }

        /// <summary>
        /// 单例
        /// </summary>
        public static SystemManager Instance { get; set; }

        #endregion

        #region 系统支持

        /// <summary>
        ///     连接到
        /// </summary>
        public bool PingCenter()
        {
            return ByteCommand(ZeroByteCommand.Ping);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartLeft()
        {
            return HeartLeft("SystemManage", GlobalContext.ServiceRealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartReady()
        {
            return HeartReady("SystemManage", GlobalContext.ServiceRealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartJoin()
        {
            return HeartJoin("SystemManage", GlobalContext.ServiceRealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool Heartbeat()
        {
            return Heartbeat("SystemManage", GlobalContext.ServiceRealName);
        }
        /// <summary>
        /// 尝试安装站点
        /// </summary>
        /// <param name="station"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool TryInstall(string station, string type)
        {
            if (ZeroApplication.Config.TryGetConfig(station, out _))
                return true;
            ZeroTrace.SystemLog(station, "No find,try install ...");
            var r = CallCommand("install", type, station, station, station);
            if (!r.InteractiveSuccess)
            {
                ZeroTrace.WriteError(station, "Install failed.");
                return false;
            }

            if (r.State != ZeroOperatorStateType.Ok && r.State != ZeroOperatorStateType.Failed)
            {
                ZeroTrace.WriteError(station, "Install failed.please check name or type.");
                return false;
            }
            ZeroTrace.SystemLog(station, "Install successfully,try start it ...");
            r = CallCommand("start", station);
            if (!r.InteractiveSuccess && r.State != ZeroOperatorStateType.Ok && r.State != ZeroOperatorStateType.Runing)
            {
                ZeroTrace.WriteError(station, "Can't start station");
                return false;
            }
            ZeroTrace.SystemLog(station, "Station runing");
            return true;
        }

        /// <summary>
        /// 尝试安装站点
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public bool TryStart(string station)
        {
            if (!ZeroApplication.Config.TryGetConfig(station, out var config))
                return false;
            ZeroTrace.SystemLog(station, "Try start it ...");
            var r = CallCommand("start", station);
            if (!r.InteractiveSuccess && r.State != ZeroOperatorStateType.Ok && r.State != ZeroOperatorStateType.Runing)
            {
                ZeroTrace.WriteError(station, "Can't start station");
                return false;
            }
            ZeroTrace.SystemLog(station, "Station runing");
            return true;
        }

        //上传文档是否已执行过
        private bool _documentIsUpload;
        /// <summary>
        ///     上传文档
        /// </summary>
        /// <returns></returns>
        public bool UploadDocument()
        {
            if (_documentIsUpload)
                return true;
            bool success = true;
            foreach (var doc in ZeroApplication.Config.Documents.Values)
            {
                if (!doc.IsLocal)
                    continue;
                var result = CallCommand("doc", doc.Name, JsonConvert.SerializeObject(doc));
                if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                {
                    ZeroTrace.WriteError("UploadDocument", result);
                    success = false;
                }
            }
            _documentIsUpload = success;
            return success;
        }

        /// <summary>
        ///     下载文档
        /// </summary>
        /// <returns></returns>
        public bool LoadDocument(string name, out StationDocument doc)
        {
            ZeroResult result;
            try
            {
                result = CallCommand("doc", name);
                if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                {
                    ZeroTrace.WriteError("LoadDocument", name, result.State, result);
                    doc = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("LoadDocument", e, name);
                doc = null;
                return false;
            }
            if (!result.TryGetValue(ZeroFrameType.Status, out var json))
            {
                ZeroTrace.WriteError("LoadDocument", name, "Empty");
                doc = null;
                return false;
            }
            try
            {
                doc = JsonConvert.DeserializeObject<StationDocument>(json);
                //ZeroTrace.SystemLog("LoadDocument", name,"success");
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("LoadDocument", e, name, json);
                doc = null;
                return false;
            }
        }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public bool LoadAllConfig()
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
            ZeroTrace.SystemLog("LoadAllConfig", json);
            return ZeroApplication.Config.FlushConfigs(json);
        }


        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public StationConfig LoadConfig(string stationName)
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

            return !ZeroApplication.Config.UpdateConfig(stationName, json, out var config) ? null : config;
        }


        #endregion
    }
}