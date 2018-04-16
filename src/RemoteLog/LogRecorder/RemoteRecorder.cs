using System;
using System.Configuration;
using System.IO;
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
    public sealed class RemoteRecorder : ILogRecorder
    {
        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        void ILogRecorder.RecordLog(RecordInfo info)
        {
            using (LogRecordingScope.CreateScope())
            {
                if (StationProgram.State < StationState.Closing)
                {
                    Items.Push(new PublishItem
                    {
                        Station = StationProgram.Config.StationName,
                        Title = "Record",
                        SubTitle = info.TypeName,
                        Content = JsonConvert.SerializeObject(info)
                    });
                }
                else
                {
                    Common.Logging.LogRecorder.BaseRecorder.RecordLog(info);
                }
            }
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
        void ILogRecorder.Initialize()
        {
            _state = 0;
            Task.Factory.StartNew(SendTask);
        }
        /// <summary>
        ///   停止
        /// </summary>
        void ILogRecorder.Shutdown()
        {
            _state = 3;
            while (_state == 4)
                Thread.Sleep(3);
            _state = 5;
        }

        /// <summary>
        /// 广播总数
        /// </summary>
        public static long PubCount { get; private set; }
        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask()
        {
            while (StationProgram.State != StationState.Run)
            {
                Thread.Sleep(100);
            }

            if (CreateSocket() != ZeroCommandStatus.Success)
            {
                _state = 6;
                return;
            }

            var ts = new TimeSpan(0, 0, 0, 3);
            _state = 2;
            byte[] description = new byte[5];
            description[0] = 3;
            description[1] = ZeroFrameType.Publisher;
            description[2] = ZeroFrameType.SubTitle;
            description[3] = ZeroFrameType.Argument;
            description[4] = ZeroFrameType.End;
            while (StationProgram.State == StationState.Run)
            {
                if (!Items.StartProcess(out var item, 100))
                    continue;
                string result;
                bool success = false;
                try
                {
                    _socket.SendMoreFrame(item.Title);
                    _socket.SendMoreFrame(description);
                    _socket.SendMoreFrame(StationProgram.Config.StationName);
                    _socket.SendMoreFrame(item.SubTitle);
                    _socket.SendFrame(item.Content);
                    success = _socket.TryReceiveFrameString(ts,out result);
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
                if (!success || result != ZeroHelper.zero_command_ok)
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
                _socket.Disconnect(config.OutAddress);
                _socket.Close();
            }
            _state = 4;
        }

        private StationConfig config;
        private RequestSocket _socket;
        /// <summary>
        ///     取得Socket对象
        /// </summary>
        /// <returns></returns>
        private ZeroCommandStatus CreateSocket()
        {
            try
            {
                if (config == null)
                {
                    config = StationProgram.GetConfig("RemoteLog", out var status);
                    if (status != ZeroCommandStatus.Success)
                        return status;

                }
                if (_socket != null)
                {
                    _socket.Disconnect(config.OutAddress);
                    _socket.Close();
                }

                string realName = $"{StationProgram.Config.StationName}-{RandomOperate.Generate(8)}";
                _socket = new RequestSocket();
                _socket.Options.Identity = realName.ToAsciiBytes();
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                _socket.Connect(config.OutAddress);
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
