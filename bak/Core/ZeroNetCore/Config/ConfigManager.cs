using Agebull.MicroZero.ApiDocuments;
using Newtonsoft.Json;
using System;


namespace Agebull.MicroZero.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class ConfigManager : ZSimpleCommand
    {
        #region 实例

        /// <summary>
        ///   服务中心
        /// </summary>
        public ZeroItem Center { get; }


        /// <summary>
        /// 构造
        /// </summary>
        public ConfigManager(ZeroItem center)
        {
            Center = center;
            ManageAddress = center.ManageAddress;
            ServiceKey = center.ServiceKey.ToZeroBytes();
        }
        #endregion

        #region 系统支持


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
            if (!ZeroApplication.Config.TryGetConfig(station, out _))
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

        /// <summary>
        ///     上传文档
        /// </summary>
        /// <returns></returns>
        public bool UploadDocuments()
        {
            bool success = true;
            foreach (var doc in ZeroApplication.Config.Documents.Values)
            {
                if (!doc.IsLocal)
                    continue;
                var result = CallCommand("doc", doc.Name, JsonHelper.SerializeObject(doc));
                if (result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok)
                    continue;
                ZeroTrace.WriteError("UploadDocument", result);
                success = false;
            }
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
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("LoadDocument", e, name);
                doc = null;
                return false;
            }
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
            {
                ZeroTrace.WriteError("LoadDocument", name, result.State);
                doc = null;
                return false;
            }
            if (!result.TryGetString(ZeroFrameType.Status, out var json))
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
        public static bool LoadAllConfig()
        {
            var item = ZeroApplication.Config.Master;
            var cm = new ConfigManager(item);
            var json = cm.LoadGroupConfig();
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }
            if(!ZeroApplication.Config.FlushConfigs(ZeroApplication.Config.Master, json))
            {
                return false;
            }
            for (int i = 1; i < ZeroApplication.Config.ZeroGroup.Count; i++)
            {
                item = ZeroApplication.Config.ZeroGroup[i];
                cm = new ConfigManager(item);
                json = cm.LoadGroupConfig();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    ZeroApplication.Config.FlushConfigs(item, json);
                }
            }
            ZeroApplication.RaiseEvent(ZeroNetEventType.ConfigUpdate);
            return true;
        }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public string LoadGroupConfig()
        {
            var result = CallCommand("host", "*");
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
            {
                ZeroTrace.WriteError("LoadConfig", result);
                return null;
            }
            if (!result.TryGetString(ZeroFrameType.Status, out var json))
            {
                ZeroTrace.WriteError("LoadAllConfig", "Empty");
                return null;
            }
            ZeroTrace.SystemLog("LoadAllConfig", ManageAddress, json);
            return json;
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

            if (result.TryGetString(ZeroFrameType.Status, out var json) || json[0] != '{')
            {
                ZeroTrace.WriteError("LoadConfig", stationName, "not a json", json);
                return null;
            }

            return !ZeroApplication.Config.UpdateConfig(Center, stationName, json, out var config) ? null : config;
        }


        #endregion
    }
}