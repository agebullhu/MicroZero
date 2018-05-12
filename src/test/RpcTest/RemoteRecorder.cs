using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Gboxt.Common.DataModel;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.LogRecorder
{
    /// <summary>
    ///   远程记录器
    /// </summary>
    public sealed class Recorder
    {
        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        public void RecordLog(PublishItem info)
        {
            Send(info);
        }

        /// <summary>
        /// 请求队列
        /// </summary>
        public static readonly SyncQueue<PublishItem> Items = new SyncQueue<PublishItem>();

        /// <summary>
        ///     运行状态
        /// </summary>
        private static int _state = -1;

        /// <summary>
        ///     启动
        /// </summary>
        public void Initialize()
        {
            _state = 0;
            InitSocket();
            //Task.Factory.StartNew(SendTask);
        }
        /// <summary>
        ///   停止
        /// </summary>
        public void Shutdown()
        {
            _state = 3;

            if (_socket != null)
            {
                _socket.Disconnect(_config.OutAddress);
                _socket.Close();
                _socket.Dispose();
            }
            //while (_state == 4)
            //    Thread.Sleep(3);
            _state = 5;
        }

        private  readonly TimeSpan _timeWaite = new TimeSpan(0, 0, 0, 3);
        private  readonly byte[] _description = new byte[5]{(byte)3, ZeroFrameType.Publisher , ZeroFrameType.SubTitle, ZeroFrameType.Argument, ZeroFrameType.End };
        /// <summary>
        /// 广播总数
        /// </summary>
        public long PubCount { get; private set; }

        private  bool InitSocket()
        {
            int tryCnt = 0;
            while (true)
            {
                _config = StationProgram.GetConfig("RemoteLog", out var status);
                if (status == ZeroCommandStatus.Success || _config == null)
                {
                    break;
                }

                if (++tryCnt > 5)
                {
                    StationProgram.WriteError("无法获取RemoteLog配置");
                    return false;
                }

                Thread.Sleep(100);
            }
            return CreateSocket() == ZeroCommandStatus.Success;
        }

        public void Send(PublishItem item)
        {
            string result;
            bool success;
            try
            {
                _socket.SendMoreFrame(item.Title);
                _socket.SendMoreFrame(_description);
                _socket.SendMoreFrame(StationProgram.Config.StationName);
                _socket.SendMoreFrame(item.SubTitle);
                _socket.SendFrame(item.Content);
                success = _socket.TryReceiveFrameString(_timeWaite, out result);
            }
            catch (Exception e)
            {
                Common.Logging.LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                {
                    Type = LogType.Error,
                    RequestID = item.RequetId,
                    Name = "RemoteLog",
                    Message = $"日志发送失败，内容为：\r\n{item.Content}\r\n异常为：\r\n{e}"
                });
                CreateSocket();
                return;
            }
            if (!success || result != ZeroNetStatus.ZeroCommandOk)
            {
                Common.Logging.LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                {
                    Type = LogType.Warning,
                    RequestID = item.RequetId,
                    Name = "RemoteLog",
                    Message = $"日志发送失败，内容为：\r\n{item.Content}"
                });
                return;
            }
            PubCount++;
            if (PubCount == long.MaxValue)
                PubCount = 0;
        }
        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask()
        {
            while (StationProgram.State != StationState.Run)
            {
                Thread.Sleep(100);
            }

            if (!InitSocket())
            {
                _state = 6;
                return;
            }


            _state = 2;
            
            while (StationProgram.State == StationState.Run)
            {
                if (!Items.StartProcess(out var item, 100))
                    continue;
                string result;
                bool success;
                try
                {
                    _socket.SendMoreFrame(item.Title);
                    _socket.SendMoreFrame(_description);
                    _socket.SendMoreFrame(StationProgram.Config.StationName);
                    _socket.SendMoreFrame(item.SubTitle);
                    _socket.SendFrame(item.Content);
                    success = _socket.TryReceiveFrameString(_timeWaite, out result);
                }
                catch (Exception e)
                {
                    Common.Logging.LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                    {
                        Type = LogType.Error,
                        RequestID = item.RequetId,
                        Name = "RemoteLog",
                        Message = $"日志发送失败，内容为：\r\n{item.Content}\r\n异常为：\r\n{e}"
                    });
                    CreateSocket();
                    continue;
                }
                if (!success || result != ZeroNetStatus.ZeroCommandOk)
                {
                    Common.Logging.LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                    {
                        Type = LogType.Warning,
                        RequestID = item.RequetId,
                        Name = "RemoteLog",
                        Message = $"日志发送失败，内容为：\r\n{item.Content}"
                    });
                    continue;
                }
                Items.EndProcess();
                PubCount++;
                if (PubCount == long.MaxValue)
                    PubCount = 0;
            }
            if (_socket != null)
            {
                _socket.Disconnect(_config.OutAddress);
                _socket.Close();
            }
            _state = 4;
        }

        private StationConfig _config;
        private RequestSocket _socket;
        /// <summary>
        ///     取得Socket对象
        /// </summary>
        /// <returns></returns>
        private ZeroCommandStatus CreateSocket()
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Disconnect(_config.OutAddress);
                    _socket.Close();
                }

                string realName = $"{StationProgram.Config.StationName}-{RandomOperate.Generate(8)}";
                _socket = new RequestSocket();
                _socket.Options.Identity = realName.ToAsciiBytes();
                
                //_socket.Options.DisableTimeWait = true;
                //_socket.Options.SendHighWatermark = short.MaxValue;
                //_socket.Options.ReceiveHighWatermark = short.MaxValue;
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                //_socket.Options.TcpKeepalive = true;
                _socket.Connect(_config.OutAddress);
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
    }
}
