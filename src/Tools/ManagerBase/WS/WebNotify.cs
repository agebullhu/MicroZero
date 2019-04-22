using System;
using System.Threading;
using Agebull.ZeroNet.Core;
using NetMQ;
using NetMQ.WebSockets;
using Newtonsoft.Json;

namespace WebMonitor
{
    public class WebNotify : IZeroObject
    {
        //private NetMQPoller _poller;
        //private WsRouter _router;
        private WsPublisher _publisher;

        private static WebNotify _instance;

        public string Name => "Zero Monitor";

        /// <summary>
        ///     运行状态
        /// </summary>
        private int _state;

        /// <summary>
        ///     运行状态
        /// </summary>
        public int State
        {
            get => _state;
            set => Interlocked.Exchange(ref _state, value);
        }

        void IZeroObject.OnZeroInitialize()
        {
            State = StationState.Initialized;
        }

        void IZeroObject.OnZeroStart()
        {
            Dispose();
            State = StationState.Start;
            _instance = this;
            _isDispose = false;
            //_router = new WsRouter();
            //_router.ReceiveReady += OnRouterReceiveReady;
            _publisher = new WsPublisher();
            SystemMonitor.StationEvent += SystemMonitor_StationEvent;

            State = StationState.Run;
            //_router.Bind("ws://*:80");
            _publisher.Bind("ws://*:6001");
            ZeroTrace.WriteInfo(Name, "ws://*:6001");

            //_poller = new NetMQPoller { _router };
            //_poller.RunAsync();
        }

        void IZeroObject.OnZeroEnd()
        {
            Dispose();
            State = StationState.Closed;
        }

        void IZeroObject.OnZeroDistory()
        {
            State = StationState.Destroy;
        }

        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
        }
        private void SystemMonitor_StationEvent(object sender, SystemMonitor.StationEventArgument e)
        {
            if (e.EventName != "station_state")
                return;
            Publish("config", JsonHelper.SerializeObject(e.EventConfig));
        }

        void OnRouterReceiveReady(object sender, WsSocketEventArgs eventArgs)
        {
            var identity = eventArgs.Socket.ReceiveFrameBytes();
            string message = eventArgs.Socket.ReceiveFrameString();

            eventArgs.Socket.SendMoreFrame(identity).SendFrame("OK");

            _publisher.SendMoreFrame("chat").SendFrame(message.ToUpper());
        }

        /// <summary>
        /// 广播消息到Socket
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="message"></param>
        public static void Publish(string theme, string message)
        {
            if(_instance != null && _instance. State == StationState.Run)
                _instance._publisher?.SendMoreFrame(theme).SendFrame(message.ToUtf8Bytes());
        }

        #region IDisposable Support

        private bool _isDispose = true;

        ~WebNotify()
        {
            Dispose();
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            if (_isDispose)
                return;
            _isDispose = true;
            //_router.ReceiveReady -= OnRouterReceiveReady;
            SystemMonitor.StationEvent -= SystemMonitor_StationEvent;
            //_poller.Dispose();
            //_router.Dispose();
            _publisher.Dispose();
            //_poller = null;
            //_router = null;
            _publisher = null;
        }
        #endregion

    }
}