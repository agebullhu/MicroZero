using System;
using System.Threading;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Gboxt.Common.DataModel.ZeroNet
{
    /// <summary>
    /// 实体事件代理,实现网络广播功能
    /// </summary>
    public class EntityEventProxy : IEntityEventProxy
    {
        /// <summary>状态修改事件</summary>
        /// <param name="database">数据库</param>
        /// <param name="entity">实体</param>
        /// <param name="type">状态类型</param>
        /// <param name="value">对应实体</param>
        void IEntityEventProxy.OnStatusChanged(string database, string entity, DataOperatorType type, string value)
        {
            Items.Push(new EntityEventItem
            {
                DbName = database,
                EntityName = entity,
                EvenType = type,
                Value = value
            });
        }


        #region Field
        /// <summary>
        /// 配置
        /// </summary>
        private StationConfig _config;
        /// <summary>
        /// 连接对象
        /// </summary>
        private RequestSocket _socket;

        /// <summary>
        /// 请求队列
        /// </summary>
        public static readonly EntityEventQueue Items = new EntityEventQueue();

        /// <summary>
        ///     运行状态
        /// </summary>
        private static StationState _state;

        /// <summary>
        /// 超时
        /// </summary>
        private static readonly TimeSpan TimeWaite = new TimeSpan(0, 0, 0, 3);

        /// <summary>
        /// 广播内容说明
        /// </summary>
        private static readonly byte[] Description = new byte[]
        {
            2,
            ZeroFrameType.Publisher,
            ZeroFrameType.Argument,
            ZeroFrameType.End
        };//ZeroFrameType.SubTitle, 
        #endregion

        #region Task

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask()
        {
            _state = StationState.Run;
            while (ZeroApplication.State < StationState.Closing && _state == StationState.Run)
            {
                if (ZeroApplication.State != StationState.Run)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (!Items.StartProcess(out var database, out var items, 100))
                {
                    Thread.Sleep(10);
                    continue;
                }
                if (_socket == null && InitSocket())
                {
                    Thread.Sleep(10);
                    continue;
                }
                string result;
                bool success;
                try
                {
                    _socket.SendMoreFrame(database);
                    _socket.SendMoreFrame(Description);
                    _socket.SendMoreFrame(ZeroApplication.Config.StationName);
                    _socket.SendFrame(JsonConvert.SerializeObject(items));
                    success = _socket.TryReceiveFrameString(TimeWaite, out result);
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "实体事件发送失败");
                    CreateSocket();
                    continue;
                }
                if (!success || result != ZeroNetStatus.ZeroCommandOk)
                {
                    LogRecorder.Error("实体事件发送失败");
                    continue;
                }
                Items.EndProcess();
            }
            _state = StationState.Closed;
            _socket.CloseSocket(_config.RequestAddress);
        }


        #endregion

        #region Socket

        
        private bool InitSocket()
        {
            _config = ZeroApplication.GetConfig("EntityEvent", out var status);
            if (status != ZeroCommandStatus.Success || _config == null)
            {
                return false;
            }
            return CreateSocket() == ZeroCommandStatus.Success;
        }
        /// <summary>
        ///     取得Socket对象
        /// </summary>
        /// <returns></returns>
        private ZeroCommandStatus CreateSocket()
        {
            try
            {
                _socket.CloseSocket(_config.RequestAddress);

                _socket = new RequestSocket();
                _socket.Options.Identity = ZeroApplication.Config.Identity;
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 10);
                _socket.Options.ReconnectIntervalMax = new TimeSpan(0, 0, 0, 0, 500);
                _socket.Options.TcpKeepalive = true;
                _socket.Options.TcpKeepaliveIdle = new TimeSpan(0, 1, 0);
                _socket.Connect(_config.RequestAddress);
                return ZeroCommandStatus.Success;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return ZeroCommandStatus.Exception;
            }
        }

        #endregion
    }
}
