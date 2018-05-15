using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Base;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Gboxt.Common.DataModel;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace ZeroApi
{
    /// <summary>
    /// 数据事件广播代理
    /// </summary>
    public class ZeroEntityEventProxy : ScopeBase, IEntityEventProxy
    {
        private readonly RequestSocket _socket;
        private readonly StationConfig config_;

        /// <summary>
        /// 构造
        /// </summary>
        public ZeroEntityEventProxy()
        {
            config_ = ZeroApplication.GetConfig("EntityEvent", out var status);
            if (status == ZeroCommandStatus.NoFind)
            {
                return;
            }
            if (config_ == null)
                throw new Exception("无法拉取配置");
            _socket = new RequestSocket();
            CreateSocket();
        }

        private void CreateSocket()
        {
            try
            {
                _socket.Options.Identity = ZeroApplication.Config.Identity;
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                _socket.Connect(config_.RequestAddress);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError($"【EntityEvent】connect error =>{e.Message}");
                throw;
            }

        }

        private bool Request(string msg, ref int retry)
        {

            if (++retry > 5)
            {
                LogRecorder.Error($"数据事件服务(EntityEvent)无法连接!\r\n{msg}");
                StationConsole.WriteError($"数据事件服务(EntityEvent)无法连接!\r\n{msg}");
                return false;
            }
            try
            {
                lock (this)
                {
                    _socket.Request(msg);
                }
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError($"【EntityEvent】publish error =>{e.Message}");
                retry++;
                Thread.Sleep(10);
                CreateSocket();
                return Request(msg, ref retry);
            }
        }

        private void Request(object arg)
        {
            if (_socket == null)
                return;
            var retry = 0;
            Request(arg.ToString(), ref retry);
        }

        /// <summary>状态修改事件</summary>
        /// <param name="entityType">实体类型Id</param>
        /// <param name="type">状态类型</param>
        /// <param name="data">对应实体</param>
        void IEntityEventProxy.OnStatusChanged(int entityType, EntitySubsist type, NotificationObject data)
        {
            var msg = $@"
{ZeroApplication.Config.StationName}
{data.GetType().FullName}
{type}
{(JsonConvert.SerializeObject(data))}";
            Task.Factory.StartNew(Request, msg);
        }

        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
            _socket?.Dispose();
        }
    }
}