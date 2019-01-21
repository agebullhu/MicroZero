using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebMonitor
{
    /// <summary>
    /// 表示一个WebSocket连接
    /// </summary>
    public class WebSocketClient : IDisposable
    {
        public readonly List<string> Subscriber = new List<string>();
        public const int BufferSize = 4096;
        private readonly WebSocket _socket;
        public readonly string Classify;
        private readonly List<WebSocketClient> _clients;


        internal WebSocketClient(WebSocket socket, string path, List<WebSocketClient> clients)
        {
            Classify = path.Trim('\\', '/', ' ');
            _socket = socket;
            _clients = clients;
        }


        internal async Task EchoLoop()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);
            while (_socket.State == WebSocketState.Open)
            {
                try
                {
                    string value;
                    using (var mem = new MemoryStream())
                    {
                        var incoming = await _socket.ReceiveAsync(seg, CancellationToken.None);
                        if (!incoming.EndOfMessage)
                        {
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
            Dispose();
        }
        internal void Send(ArraySegment<byte> title, ArraySegment<byte> array)
        {
            if (isDisposed)
                return;
            try
            {
                //await this.socket.SendAsync(title, WebSocketMessageType.Text, true, CancellationToken.None);
                _socket.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                Dispose();
            }
        }

        private bool isDisposed;
        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            _clients.Remove(this);
            try
            {
                _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭", CancellationToken.None);
            }
            catch
            {
                // ignored
            }
        }
    }
}