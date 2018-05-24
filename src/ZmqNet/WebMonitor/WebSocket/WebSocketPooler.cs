using System;
using System.Globalization;
using Agebull.ZeroNet.Core;
using NetMQ;
using NetMQ.WebSockets;
using Newtonsoft.Json;

namespace WebMonitor
{
    public class WebSocketPooler : IDisposable
    {
        private NetMQPoller _poller;
        private WsRouter _router;
        private WsPublisher _publisher;

        public static WebSocketPooler Instance = new WebSocketPooler();

        public WebSocketPooler()
        {
            _router = new WsRouter();
            _router.ReceiveReady += OnRouterReceiveReady;
            _publisher = new WsPublisher();
            SystemMonitor.StationEvent += SystemMonitor_StationEvent;
        }

        private void SystemMonitor_StationEvent(object sender, SystemMonitor.StationEventArgument e)
        {
            if (e.EventName != "station_state") return;
            e.EventConfig.Description = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var json = JsonConvert.SerializeObject(e.EventConfig);
            Publish("config", json);
        }

        void OnRouterReceiveReady(object sender, WsSocketEventArgs eventArgs)
        {
            var identity = eventArgs.Socket.ReceiveFrameBytes();
            string message = eventArgs.Socket.ReceiveFrameString();

            eventArgs.Socket.SendMoreFrame(identity).SendFrame("OK");

            _publisher.SendMoreFrame("chat").SendFrame(message.ToUpper());
        }
        public void Pool()
        {
            _router.Bind("ws://*:80");
            _publisher.Bind("ws://*:81");
            
            _poller = new NetMQPoller { _router };
            _poller.RunAsync();
        }
        /// <summary>
        /// 广播消息到Socket
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="message"></param>
        public void Publish(string theme,string message)
        {
            if (_publisher == null)
                return;
            _publisher.SendMoreFrame(theme).SendFrame(message.ToUtf8Bytes());
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;
            Instance = null;
            _poller?.Dispose();
            _router?.Dispose();
            _publisher?.Dispose();
            _poller = null;
            _router = null;
            _publisher = null;
            disposedValue = true;
        }

        ~WebSocketPooler()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
        }
        #endregion
    }
}