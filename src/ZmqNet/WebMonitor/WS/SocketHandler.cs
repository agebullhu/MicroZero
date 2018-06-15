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
                    continue;
                }

                if (handler.Subscriber.Count == 0)
                {
                    if (handler.Send(outgoing).Result)
                        Handlers.Remove(handler);
                    continue;
                }

                foreach (var sub in handler.Subscriber)
                {
                    if (sub != "" && sub.Length > title.Length && title.Substring(0, sub.Length) != sub)
                        continue;
                    if (!handler.Send(outgoing).Result)
                        Handlers.Remove(handler);
                    break;
                }
            }
        }

        public List<string> Subscriber { get; } = new List<string>();



        public static List<WebNotify> Handlers = new List<WebNotify>();

        public const int BufferSize = 4096;
        readonly WebSocket socket;

        static WebNotify()
        {
            SystemMonitor.ZeroNetEvent += SystemMonitor_StationEvent;
        }

        private static void SystemMonitor_StationEvent(object sender, SystemMonitor.ZeroNetEventArgument e)
        {
            if (e.Event != ZeroNetEventType.CenterStationState)
                return;
            e.EventConfig.CheckValue(e.NewConfig);
            Publish("config", JsonConvert.SerializeObject(e.EventConfig));
        }

        WebNotify(WebSocket socket)
        {
            this.socket = socket;
        }

        private async Task EchoLoop()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);
            while (this.socket.State == WebSocketState.Open)
            {
                try
                {
                    string value;
                    using (var mem = new MemoryStream())
                    {
                        var incoming = await this.socket.ReceiveAsync(seg, CancellationToken.None);
                        if (!incoming.EndOfMessage)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message too big", CancellationToken.None);
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
                    if (string.IsNullOrEmpty(value))
                        continue;
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
                catch (WebSocketException)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
       async Task<bool> Send(ArraySegment<byte> array)
        {
            try
            {
                await this.socket.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;
            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var notify = new WebNotify(socket);
            Handlers.Add(notify);
            await notify.EchoLoop();
            Handlers.Remove(notify);
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
            app.Use(Acceptor);
        }
    }
}