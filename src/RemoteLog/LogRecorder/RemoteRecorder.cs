using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.LogRecorder
{
    /// <summary>
    ///   远程记录器
    /// </summary>
    public sealed class RemoteRecorder : ILogRecorder
    {
        #region Override


        /// <inheritdoc />
        /// <summary>
        ///     启动
        /// </summary>
        void ILogRecorder.Initialize()
        {
            Common.Logging.LogRecorder.LogByTask = true;
            Common.Logging.LogRecorder.TraceToConsole = false;
            _state = 0;
            Task.Factory.StartNew(SendTask);
        }
        /// <inheritdoc />
        /// <summary>
        ///   停止
        /// </summary>
        void ILogRecorder.Shutdown()
        {
            if (_state != StationState.Run)
                return;
            _state = StationState.Closing;
            while (_state == StationState.Closed)
                Thread.Sleep(3);
            _state = StationState.Destroy;
        }


        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        void ILogRecorder.RecordLog(RecordInfo info)
        {
            info.User = $"{ApiContext.Customer.Account}({ApiContext.RequestContext.Ip}:{ApiContext.RequestContext.Port})";
            info.Machine = ZeroApplication.Config.RealName;
            using (LogRecordingScope.CreateScope())
            {
                if (ZeroApplication.State < StationState.Failed && ZeroApplication.State > StationState.Start)
                {
                    Items.Push(info);
                }
                else
                {
                    Common.Logging.LogRecorder.BaseRecorder.RecordLog(info);
                }
            }
        }

        #endregion

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
        public static readonly LogQueue Items = new LogQueue();

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
        /// <summary>
        /// 广播总数
        /// </summary>
        public static long PubCount { get; private set; }
        /// <summary>
        /// 广播总数
        /// </summary>
        public static long DataCount { get; private set; }

        #endregion

        #region Task

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask()
        {
            _identity = ZeroApplication.Config.ToZeroIdentity("LogRecorder");
            _state = StationState.Run;
            while (ZeroApplication.State < StationState.Closing && _state == StationState.Run)
            {
                if (ZeroApplication.State != StationState.Run)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (!Items.StartProcess(out var title, out var items, 100))
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
                    _socket.SendMoreFrame(title.ToString());
                    _socket.SendMoreFrame(Description);
                    _socket.SendMoreFrame(ZeroApplication.Config.StationName);
                    _socket.SendFrame(JsonConvert.SerializeObject(items));
                    success = _socket.TryReceiveFrameString(TimeWaite, out result);
                }
                catch (Exception e)
                {
                    Common.Logging.LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                    {
                        Type = LogType.Error,
                        Name = "RemoteLog",
                        Message = $"日志发送失败，\r\n异常为：\r\n{e}"
                    });
                    CreateSocket();
                    continue;
                }
                if (!success || result != ZeroNetStatus.ZeroCommandOk)
                {
                    Common.Logging.LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                    {
                        Type = LogType.Error,
                        Name = "RemoteLog",
                        Message = $"日志发送失败，\r\n异常为：\r\n{result}"
                    });
                    continue;
                }
                Items.EndProcess();
                PubCount += 1;
                DataCount += items.Count;
                if (DataCount == long.MaxValue)
                    DataCount = 0;
                if (PubCount == long.MaxValue)
                    PubCount = 0;
            }
            _state = StationState.Closed;
            _socket.CloseSocket(_config.RequestAddress);
        }


        #endregion

        #region Socket


        private byte[] _identity;

        private bool InitSocket()
        {
            _config = ZeroApplication.GetConfig("RemoteLog", out var status);
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
                _socket.Options.Identity = _identity;

                _socket.Options.DisableTimeWait = true;
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                _socket.Connect(_config.RequestAddress);
                return ZeroCommandStatus.Success;
            }
            catch (Exception e)
            {
                Common.Logging.LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                {
                    Type = LogType.Error,
                    RequestID = Guid.NewGuid().ToString(),
                    Name = "RemoteLog",
                    Message = $"Socket构造失败\r\n异常为：\r\n{e}"
                });
                return ZeroCommandStatus.Exception;
            }
        }

        #endregion
    }
}
