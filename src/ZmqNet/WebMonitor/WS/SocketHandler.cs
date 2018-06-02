using System;
using System.Collections.Generic;
using System.IO;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebMonitor
{
    public class WebNotify
    {
        public static void Publish(string title, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            var tbuffer = title.ToUtf8Bytes();
            var toutgoing = new ArraySegment<byte>(tbuffer, 0, tbuffer.Length);
            var buffer = value.ToUtf8Bytes();
            var outgoing = new ArraySegment<byte>(buffer, 0, buffer.Length);
            foreach (var handler in Handlers.ToArray())
            {
                if (handler.socket.State != WebSocketState.Open)
                {
                    Handlers.Remove(handler);
                }
                else if (handler.Subscriber.Count == 0)
                {
                    handler.Send(toutgoing, outgoing);
                }
                else
                {
                    foreach (var sub in handler.Subscriber)
                    {
                        if (sub == "" || sub.Length <= title.Length || title.Substring(0, sub.Length) == sub)
                        {
                            handler.Send(toutgoing, outgoing);
                            break;
                        }
                    }
                }
            }
        }

        public List<string> Subscriber { get; } = new List<string>();



        public static List<WebNotify> Handlers = new List<WebNotify>();

        public const int BufferSize = 4096;
        readonly WebSocket socket;

        static WebNotify()
        {
            SystemMonitor.StationEvent += SystemMonitor_StationEvent;
        }

        private static void SystemMonitor_StationEvent(object sender, SystemMonitor.StationEventArgument e)
        {
            if (e.EventName != "station_state")
                return;
            Publish("config", JsonConvert.SerializeObject(e.EventConfig));
        }

        WebNotify(WebSocket socket)
        {
            this.socket = socket;
        }
        async Task EchoLoop()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);
            while (this.socket.State == WebSocketState.Open)
            {
                string value;
                using (var mem = new MemoryStream())
                {
                    var incoming = await this.socket.ReceiveAsync(seg, CancellationToken.None);
                    if (!incoming.EndOfMessage)
                    {
                        await this.socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "内容太大", CancellationToken.None);
                        break;
                    }
                    if (incoming.Count == 0)
                        continue;
                    mem.Write(seg.Array, 0, incoming.Count);
                    mem.Flush();
                    mem.Position = 0;
                    TextReader reader = new StreamReader(mem);
                    value = reader.ReadToEnd();


                }

                string title = value.Length == 0 ? "" : value.Substring(1);
                if (value[0] == '+')
                {
                    if (!Subscriber.Contains(title))
                        Subscriber.Add(title);
                }
                else if (value[0] == '-')
                {
                    Subscriber.Remove(title);
                }
            }
        }
        async void Send(ArraySegment<byte> title, ArraySegment<byte> array)
        {
            await this.socket.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        static async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;
            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var h = new WebNotify(socket);
            Handlers.Add(h);
            await h.EchoLoop();
        }
        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>  
        public static void Map(IApplicationBuilder app)
        {
            var option = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromHours(1),
                ReceiveBufferSize = 4096
            };
            app.UseWebSockets(option);
            app.Use(WebNotify.Acceptor);
        }
    }
}