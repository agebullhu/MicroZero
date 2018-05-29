using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using ZeroMQ;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Log
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
            LogRecorder.LogByTask = true;
            LogRecorder.TraceToConsole = false;
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
            while (_state != StationState.Closed)
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
                int state = ZeroApplication.ApplicationState;
                if (state < StationState.Closing && state > StationState.Start)
                {
                    Items.Push(info);
                }
                else
                {
                    LogRecorder.BaseRecorder.RecordLog(info);
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
        private ZSocket _socket;

        /// <summary>
        /// 请求队列
        /// </summary>
        public static readonly LogQueue Items = new LogQueue();

        /// <summary>
        ///     运行状态
        /// </summary>
        private static int _state;

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
            StationConsole.WriteInfo("RemoteLogcorder", "start...");
            while (ZeroApplication.ApplicationState != StationState.Run)
            {
                Thread.Sleep(100);
            }

            while (_socket == null && InitSocket())
            {
                Thread.Sleep(100);
            }
            StationConsole.WriteInfo("RemoteLogcorder", "run...");
            _state = StationState.Run;
            while (ZeroApplication.IsAlive && _state == StationState.Run)
            {
                if (!ZeroApplication.CanDo)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (!Items.StartProcess(out var title, out var items, 100))
                {
                    continue;
                }
                try
                {
                    if (!_socket.Publish(title.ToString(), ZeroApplication.Config.StationName,
                        JsonConvert.SerializeObject(items)))
                    {
                        LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                        {
                            Type = LogType.Error,
                            Name = "RemoteLog",
                            Message = "日志发送失败"
                        });
                        CreateSocket();
                        continue;
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                    {
                        Type = LogType.Error,
                        Name = "RemoteLog",
                        Message = $"日志发送失败，异常为：\r\n{e}"
                    });
                    CreateSocket();
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
            _socket.CloseSocket();
            _state = StationState.Closed;
        }


        #endregion

        #region Socket


        private bool InitSocket()
        {
            _config = ZeroApplication.GetConfig("RemoteLog", out var status);
            if (status != ZeroCommandStatus.Success || _config == null)
            {
                return false;
            }
            return CreateSocket();
        }
        /// <summary>
        ///     取得Socket对象
        /// </summary>
        /// <returns></returns>
        private bool CreateSocket()
        {
            _socket.CloseSocket();
            string real = ZeroIdentityHelper.CreateRealName(_config.ShortName ?? _config.StationName, RandomOperate.Generate(3)); ;
            var identity = real.ToAsciiBytes();
            StationConsole.WriteInfo("RemoteLogcorder", _config.RequestAddress);
            StationConsole.WriteInfo("RemoteLogcorder", real);
            _socket = ZeroHelper.CreateRequestSocket(_config.RequestAddress, identity);
            return _socket != null;
        }

        #endregion
    }
}
