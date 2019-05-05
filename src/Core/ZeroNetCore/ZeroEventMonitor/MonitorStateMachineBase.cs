using System;
using Agebull.MicroZero.ApiDocuments;
using Agebull.Common.Logging;
using Newtonsoft.Json;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Agebull.MicroZero.ZeroManagemant.StateMachine
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class MonitorStateMachineBase : IDisposable
    {
        #region CenterEvent

        internal static string ZeroCenterIdentity;

        internal static void center_start(string identity, string content)
        {
            if (identity == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = identity;
            ZeroTrace.SystemLog("ZeroCenter", "center_start", $"{identity}:{ZeroApplication.ZeroCenterState}:{ZeroCenterIdentity}");
            if (ZeroApplication.ZeroCenterState >= ZeroCenterState.Failed)
            {
                ZeroApplication.JoinCenter();
            }
            else
            {
                SystemManager.Instance.LoadAllConfig();
            }
        }

        internal static void center_closing(string identity, string content)
        {
            var id = $"*{identity}";
            if (id == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = id;
            ZeroTrace.SystemLog("ZeroCenter", "center_closing", $"{identity}:{ZeroApplication.ZeroCenterState}:{ZeroCenterIdentity}");
            if (ZeroApplication.ZeroCenterState < ZeroCenterState.Closing)
            {
                ZeroApplication.ZeroCenterState = ZeroCenterState.Closing;
                ZeroApplication.RaiseEvent(ZeroNetEventType.CenterSystemClosing);
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
            }
        }

        internal static void center_stop(string identity, string content)
        {
            var id = $"#{identity}";
            if (id == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = id;
            ZeroTrace.SystemLog("ZeroCenter", "center_stop", $"{identity}:{ZeroApplication.ZeroCenterState}:{ZeroCenterIdentity}");
            if (ZeroApplication.ZeroCenterState < ZeroCenterState.Closing)
            {
                ZeroApplication.ZeroCenterState = ZeroCenterState.Closed;
                ZeroApplication.RaiseEvent(ZeroNetEventType.CenterSystemStop);
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
            }
        }

        #endregion

        #region StationEvent

        /// <summary>
        /// 站点心跳
        /// </summary>
        internal static void worker_sound_off()
        {
            if (ZeroApplication.ApplicationState != StationState.Run || ZeroApplication.ZeroCenterState != ZeroCenterState.Run)
                return;
            SystemManager.Instance.Heartbeat();
        }

        /// <summary>
        /// 站点动态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        internal static void station_trends(string name, string content)
        {
            try
            {
                ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationTrends, name, content, null);
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
                ZeroTrace.WriteException(name, e, "station_state", content);
            }
        }
        internal static void station_update(string name, string content)
        {
            ZeroTrace.SystemLog(name, "station_update", content);
            if (ZeroApplication.Config.UpdateConfig(name, content, out var config))
            {
                ZeroApplication.OnStationStateChanged(config);
                ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationUpdate, name, content, config);
            }
        }
        internal static void station_install(string name, string content)
        {
            ZeroTrace.SystemLog(name, "station_install", content);
            if (ZeroApplication.Config.UpdateConfig(name, content, out var config))
            {
                ZeroApplication.OnStationStateChanged(config);
                ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationInstall, name, content, config);
            }
        }


        internal static void station_join(string name, string content)
        {
            ZeroTrace.SystemLog(name, "station_join", content);
            if (ZeroApplication.Config.UpdateConfig(name, content, out var config))
            {
                ZeroApplication.OnStationStateChanged(config);
                ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationJoin, name, content, config);
            }
        }
        internal static void ChangeStationState(string name, ZeroCenterState state, ZeroNetEventType eventType)
        {
            if (ZeroApplication.ApplicationState != StationState.Run || ZeroApplication.ZeroCenterState != ZeroCenterState.Run)
                return;
            if (!ZeroApplication.Config.TryGetConfig(name, out var config) || !config.ChangedState(state))
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(eventType, name, null, config);
        }


        internal static void station_uninstall(string name)
        {
            ZeroTrace.SystemLog(name, "station_uninstall");
            ChangeStationState(name, ZeroCenterState.Remove, ZeroNetEventType.CenterStationRemove);
        }


        internal static void station_closing(string name)
        {
            ZeroTrace.SystemLog(name, "station_closing");
            ChangeStationState(name, ZeroCenterState.Closed, ZeroNetEventType.CenterStationClosing);
        }


        internal static void station_resume(string name)
        {
            ZeroTrace.SystemLog(name, "station_resume");
            ChangeStationState(name, ZeroCenterState.Run, ZeroNetEventType.CenterStationResume);
        }

        internal static void station_pause(string name)
        {
            ZeroTrace.SystemLog(name, "station_pause");
            ChangeStationState(name, ZeroCenterState.Pause, ZeroNetEventType.CenterStationPause);
        }

        internal static void station_left(string name)
        {
            ZeroTrace.SystemLog(name,"station_left");
            ChangeStationState(name, ZeroCenterState.Closed, ZeroNetEventType.CenterStationLeft);
        }

        internal static void station_stop(string name)
        {
            ZeroTrace.SystemLog(name,"station_stop");
            ChangeStationState(name, ZeroCenterState.Stop, ZeroNetEventType.CenterStationStop);
        }



        internal static void station_document(string name, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.SystemLog(name, "station_document");
            var doc = JsonConvert.DeserializeObject<StationDocument>(content);
            if (ZeroApplication.Config.Documents.ContainsKey(name))
            {
                if (!ZeroApplication.Config.Documents[name].IsLocal)
                    ZeroApplication.Config.Documents[name] = doc;
            }
            else
            {
                ZeroApplication.Config.Documents.Add(name, doc);
            }
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationDocument, name, content, config);
        }



        #endregion

        #region ClientEvent



        internal static void client_join(string name, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.SystemLog(name, "client_join", content);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterClientJoin, name, content,  config);
        }

        internal static void client_left(string name, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.SystemLog(name, "client_left", content);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterClientLeft, name, content, config);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get; internal set; }

        void IDisposable.Dispose()
        {
            IsDisposed = true;
        }

        #endregion
    }
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释