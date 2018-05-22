using System;
using System.Globalization;
using System.Text;
using Agebull.ZeroNet.Core;
using NetMQ;
using NetMQ.WebSockets;
using Newtonsoft.Json;

namespace WebMonitor
{
    public class WebSocketPooler : IDisposable
    {
        public NetMQPoller poller;
        private WSRouter router;
        private WSPublisher publisher;

        public static WebSocketPooler Instance = new WebSocketPooler();

        public WebSocketPooler()
        {
            router = new WSRouter();
            router.ReceiveReady += OnRouterReceiveReady;
            publisher = new WSPublisher();
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

            publisher.SendMoreFrame("chat").SendFrame(message.ToUpper());
        }
        public void Pool()
        {
            router.Bind("ws://*:80");
            publisher.Bind("ws://*:81");
            
            poller = new NetMQPoller { router };
            poller.RunAsync();
        }
        /// <summary>
        /// 广播消息到Socket
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="message"></param>
        public void Publish(string theme,string message)
        {
            if (publisher == null)
                return;
            publisher.SendMoreFrame(theme).SendFrame(message.ToUtf8Bytes());
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;
            Instance = null;
            poller?.Dispose();
            router?.Dispose();
            publisher?.Dispose();
            poller = null;
            router = null;
            publisher = null;
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