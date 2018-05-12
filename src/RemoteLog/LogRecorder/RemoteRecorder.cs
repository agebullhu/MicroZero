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
    public sealed class RemoteRecorder : ILogRecorder
    {
        #region Override


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
        ///     启动
        /// </summary>
        void ILogRecorder.Initialize()
        {
            _state = 0;
            Task.Factory.StartNew(SendTask);
        }
        /// <inheritdoc />
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
        public static readonly PublishQueue Items = new PublishQueue();

        /// <summary>
        ///     运行状态
        /// </summary>
        private static int _state = -1;

        /// <summary>
        /// 超时
        /// </summary>
        private static readonly TimeSpan TimeWaite = new TimeSpan(0, 0, 0, 3);

        /// <summary>
        /// 广播内容说明
        /// </summary>
        private static readonly byte[] Description = new byte[] { (byte)2, ZeroFrameType.Publisher, ZeroFrameType.Argument, ZeroFrameType.End };//ZeroFrameType.SubTitle, 
        /// <summary>
        /// 广播总数
        /// </summary>
        public static long PubCount { get; private set; }
        /// <summary>
        /// 广播总数
        /// </summary>
        public static long DataCount { get; private set; }

        #endregion
        private bool InitSocket()
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
                if (!Items.StartProcess(out var title, out var items, 100))
                {
                    Thread.Sleep(5);
                    continue;
                }
                string result;
                bool success;
                try
                {
                    _socket.SendMoreFrame(title);
                    _socket.SendMoreFrame(Description);
                    _socket.SendMoreFrame(StationProgram.Config.StationName);
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
            if (_socket != null)
            {
                _socket.Disconnect(_config.OutAddress);
                _socket.Close();
            }
            _state = 4;
        }

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

                _socket.Options.DisableTimeWait = true;
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
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
