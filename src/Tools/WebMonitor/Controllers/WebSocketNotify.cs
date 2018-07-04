using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using ZeroNet.Http.Route;

namespace WebMonitor
{

    public class WebSocketNotify
    {
        public static void Publish(string classify, string title, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (!Handlers.TryGetValue(classify, out var list))
                return;
            var array = list.ToArray();
            Task.Factory.StartNew(() => PublishAsync(array, title, value));
        }

        static async void PublishAsync(WebSocketNotify[] list, string title, string value)
        {
            var tbuffer = title.ToUtf8Bytes();
            var title_a = new ArraySegment<byte>(tbuffer, 0, tbuffer.Length);
            var buffer = value.ToUtf8Bytes();
            var value_a = new ArraySegment<byte>(buffer, 0, buffer.Length);
            foreach (var handler in list)
            {
                try
                {
                    if (handler.Subscriber.Count == 0)
                    {
                        await handler.Send(title_a, value_a);
                        continue;
                    }

                    foreach (var sub in handler.Subscriber)
                    {
                        if (sub != title)
                            continue;
                        await handler.Send(title_a, value_a);
                        break;
                    }
                }
                catch
                {
                }
            }
        }

        public List<string> Subscriber { get; } = new List<string>();



        public static Dictionary<string, List<WebSocketNotify>> Handlers = new Dictionary<string, List<WebSocketNotify>>();

        public const int BufferSize = 4096;
        readonly WebSocket socket;
        public readonly string Classify;


        WebSocketNotify(WebSocket socket, string path)
        {
            Classify = path.Trim('\\', '/', ' ');
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
                    if (string.IsNullOrEmpty(value) || value.Length <= 1)
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
        async Task<bool> Send(ArraySegment<byte> title, ArraySegment<byte> array)
        {
            try
            {
                //await this.socket.SendAsync(title, WebSocketMessageType.Text, true, CancellationToken.None);
                await this.socket.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            catch
            {
                await this.socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "异常", CancellationToken.None);
                return false;
            }
        }

        private static async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest ||!hc.Request.PathBase.HasValue)
                return;

            var classify = hc.Request.PathBase.Value.Trim('\\', '/', ' ');
            if (!Handlers.TryGetValue(classify, out var list))
            {
                return;
            }

            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var notify = new WebSocketNotify(socket, classify);
            list.Add(notify);
            await notify.EchoLoop();
            list.Remove(notify);
        }

        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>  
        public static void Binding(IApplicationBuilder app)
        {
            Binding(app, "config");
            Binding(app, "status");
            Binding(app, "kline");
            Binding(app, "api");
            Binding(app, "plan_notify");
            StationCounter.Start();
        }

        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>
        /// <param name="classify"></param>  
        public static void Binding(IApplicationBuilder app, string classify)
        {
            Handlers.Add(classify, new List<WebSocketNotify>());
            app.Map($"/{classify}", Map);
        }

        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>  
        public static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Acceptor);
        }
    }
}