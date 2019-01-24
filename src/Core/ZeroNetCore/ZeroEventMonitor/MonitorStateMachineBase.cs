using System;
using Agebull.Common.ApiDocuments;
using Agebull.Common.Logging;
using Newtonsoft.Json;
namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class MonitorStateMachineBase : IDisposable
    {
        #region CenterEvent

        protected string ZeroCenterIdentity;

        protected void center_start(string identity, string content)
        {
            if (identity == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = identity;
            ZeroTrace.SystemLog("center_start", $"{identity}:{ZeroApplication.ZeroCenterStatus}:{ZeroCenterIdentity}");
            if (ZeroApplication.ZeroCenterStatus >= ZeroCenterState.Failed)
            {
                ZeroApplication.JoinCenter();
            }
            else
            {
                SystemManager.Instance.LoadAllConfig();
            }
        }

        protected void center_closing(string identity, string content)
        {
            var id = $"*{identity}";
            if (id == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = id;
            ZeroTrace.SystemLog("center_closing", $"{identity}:{ZeroApplication.ZeroCenterStatus}:{ZeroCenterIdentity}");
            if (ZeroApplication.ZeroCenterStatus < ZeroCenterState.Closing)
            {
                ZeroApplication.ZeroCenterStatus = ZeroCenterState.Closing;
                ZeroApplication.RaiseEvent(ZeroNetEventType.CenterSystemClosing);
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
            }
        }

        protected void center_stop(string identity, string content)
        {
            var id = $"#{identity}";
            if (id == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = id;
            ZeroTrace.SystemLog("center_stop", $"{identity}:{ZeroApplication.ZeroCenterStatus}:{ZeroCenterIdentity}");
            if (ZeroApplication.ZeroCenterStatus < ZeroCenterState.Closing)
            {
                ZeroApplication.ZeroCenterStatus = ZeroCenterState.Closed;
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
        protected void worker_sound_off()
        {
            if (ZeroApplication.ApplicationState != StationState.Run || ZeroApplication.ZeroCenterStatus != ZeroCenterState.Run)
                return;
            SystemManager.Instance.Heartbeat();
        }

        protected void ChangeStationState(string name, ZeroCenterState state, ZeroNetEventType eventType)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            config.State = state;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(eventType, null, config);
        }

        protected void station_update(string name, string content)
        {
            ZeroTrace.SystemLog("station_update", name, content);
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationUpdate, content, config);
        }
        protected void station_install(string name, string content)
        {
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            ZeroTrace.SystemLog("station_install", name, content);
            config.State = ZeroCenterState.None;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationInstall, content, config);
        }

        protected void station_uninstall(string name)
        {
            ZeroTrace.SystemLog("station_uninstall", name);
            ChangeStationState(name, ZeroCenterState.Remove, ZeroNetEventType.CenterStationRemove);
        }


        protected void station_closing(string name)
        {
            ZeroTrace.SystemLog("station_closing", name);
            ChangeStationState(name, ZeroCenterState.Closing, ZeroNetEventType.CenterStationClosing);
        }


        protected void station_resume(string name)
        {
            ZeroTrace.SystemLog("station_resume", name);
            ChangeStationState(name, ZeroCenterState.Run, ZeroNetEventType.CenterStationResume);
        }

        protected void station_pause(string name)
        {
            ZeroTrace.SystemLog("station_pause", name);
            ChangeStationState(name, ZeroCenterState.Pause, ZeroNetEventType.CenterStationPause);
        }

        protected void station_left(string name)
        {
            ZeroTrace.SystemLog("station_left", name);
            ChangeStationState(name, ZeroCenterState.Closed, ZeroNetEventType.CenterStationLeft);
        }

        protected void station_stop(string name)
        {
            ZeroTrace.SystemLog("station_stop", name);
            ChangeStationState(name, ZeroCenterState.Stop, ZeroNetEventType.CenterStationLeft);
        }

        protected void station_join(string name, string content)
        {
            ZeroTrace.SystemLog("station_join", content);
            ChangeStationState(name, ZeroCenterState.Run, ZeroNetEventType.CenterStationJoin);
        }

        /// <summary>
        /// 站点心跳
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        protected void station_state(string name, string content)
        {
            //ZeroTrace.WriteInfo("station_state",name);
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            try
            {
                ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationState, content, config);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                ZeroTrace.WriteException("station_state", e, name, content);
            }
        }


        protected void station_document(string station, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(station, out var config))
                return;
            var doc = JsonConvert.DeserializeObject<StationDocument>(content);
            if (ZeroApplication.Config.Documents.ContainsKey(station))
            {
                if (!ZeroApplication.Config.Documents[station].IsLocal)
                    ZeroApplication.Config.Documents[station] = doc;
            }
            else
            {
                ZeroApplication.Config.Documents.Add(station, doc);
            }
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationDocument, station, config);
        }



        #endregion

        #region ClientEvent



        protected void client_join(string name, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterClientJoin, name, config);
        }

        protected void client_left(string name, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterClientLeft, name, config);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get; protected set; }

        void IDisposable.Dispose()
        {
            IsDisposed = true;
        }

        #endregion
    }
}