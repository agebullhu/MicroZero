using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NetMQ.WebSockets
{
    internal enum WebSocketClientState
    {
        Closed,
        Handshake,
        Ready
    }

    internal class NetMqMessageEventArgs : EventArgs
    {
        public NetMqMessageEventArgs(byte[] identity, NetMQMessage message)
        {
            Identity = identity;
            Message = message;
        }

        public byte[] Identity { get; }
        public NetMQMessage Message { get; }
    }

    internal class WebSocketClient : IDisposable
    {
        private const string MagicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private Decoder _decoder;
        private readonly NetMQSocket _streamSocket;

        private NetMQMessage _outgoingMessage;

        internal WebSocketClient(NetMQSocket streamSocket, byte[] identity)
        {
            State = WebSocketClientState.Closed;
            _streamSocket = streamSocket;
            _outgoingMessage = null;

            Identity = identity;
        }

        public byte[] Identity { get; private set; }

        public WebSocketClientState State { get; private set; }

        public event EventHandler<NetMqMessageEventArgs> IncomingMessage;

        public void OnDataReady()
        {
            switch (State)
            {
                case WebSocketClientState.Closed:
                    State = WebSocketClientState.Handshake;
                    string clientHandshake = _streamSocket.ReceiveFrameString();

                    string[] lines = clientHandshake.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

                    string key;

                    if (ValidateClientHandshake(lines, out key))
                    {
                        string acceptKey = GenerateAcceptKey(key);

                        try
                        {
                            if (_streamSocket.TrySendFrame(Identity, Identity.Length, true) &&
                                _streamSocket.TrySendFrame("HTTP/1.1 101 Switching Protocols\r\n" +
                                                            "Upgrade: websocket\r\n" +
                                                            "Connection: Upgrade\r\n" +
                                                            "Sec-WebSocket-Accept: " + acceptKey + "\r\n" +
                                                            "Sec-WebSocket-Protocol: WSNetMQ\r\n\r\n"))
                            {
                                _decoder = new Decoder();
                                _decoder.Message += OnMessage;
                                State = WebSocketClientState.Ready;
                            }
                            else
                            {
                                State = WebSocketClientState.Closed;
                            }
                        }
                        catch (NetMQException)
                        {
                            State = WebSocketClientState.Closed;
                        }
                    }
                    else
                    {
                        State = WebSocketClientState.Closed;

                        if (_streamSocket.TrySendFrame(Identity, Identity.Length, true))
                            _streamSocket.TrySendFrame("HTTP/1.1 400 Bad Request\r\nSec-WebSocket-Version: 13\r\n");

                        // invalid request, close the socket and raise closed event
                        if (_streamSocket.TrySendFrame(Identity, Identity.Length, true))
                            _streamSocket.TrySendFrame("");
                    }

                    break;
                case WebSocketClientState.Ready:
                    byte[] message = _streamSocket.ReceiveFrameBytes();
                    _decoder.Process(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Opcode == OpcodeEnum.Close)
            {
                // send close command to the socket
                try
                {
                    if (_streamSocket.TrySendFrame(Identity, Identity.Length, true))
                        _streamSocket.TrySendFrame("");
                }
                catch (NetMQException)
                {
                }

                State = WebSocketClientState.Closed;
            }
            else if (e.Opcode == OpcodeEnum.Binary)
            {
                if (_outgoingMessage == null)
                {
                    _outgoingMessage = new NetMQMessage();
                }

                _outgoingMessage.Append(e.Payload);

                if (!e.More)
                {
                    IncomingMessage?.Invoke(this, new NetMqMessageEventArgs(Identity, _outgoingMessage));

                    _outgoingMessage = null;
                }
            }
            else if (e.Opcode == OpcodeEnum.Ping)
            {
                byte[] pong = new byte[2 + e.Payload.Length];
                pong[0] = 0x8A; // Pong and Final
                pong[1] = (byte) (e.Payload.Length & 127);
                Buffer.BlockCopy(e.Payload, 0, pong, 2, e.Payload.Length);

                if (_streamSocket.TrySendFrame(Identity, Identity.Length, true))
                    _streamSocket.TrySendFrame(pong);
            }
        }

        private bool ValidateClientHandshake(string[] lines, out string key)
        {
            key = null;

            // first line should be the GET
            if (lines.Length == 0 || !lines[0].StartsWith("GET"))
                return false;

            if (!lines.Any(l => l.StartsWith("Host:")))
                return false;

            // look for upgrade command
            if (!lines.Any(l => l.Trim().Equals("Upgrade: websocket", StringComparison.OrdinalIgnoreCase)))
                return false;

            if (!lines.Any(l =>
            {
                var lt = l.Trim();
                return lt.StartsWith("Connection: ", StringComparison.OrdinalIgnoreCase) && lt
                           .Split(',', ':')
                           .Any(p => p.Trim().Equals("Upgrade", StringComparison.OrdinalIgnoreCase));
            }))
                return false;

            if (!lines.Any(l => l.Trim().Equals("Sec-WebSocket-Version: 13", StringComparison.OrdinalIgnoreCase)))
                return false;

            // look for websocket key
            string keyLine =
                lines.FirstOrDefault(l => l.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(keyLine))
                return false;

            key = keyLine.Substring(keyLine.IndexOf(':') + 1).Trim();

            return true;
        }

        private string GenerateAcceptKey(string requestKey)
        {
            string data = requestKey + MagicString;

            using (SHA1Managed sha1Managed = new SHA1Managed())
            {
                byte[] hash = sha1Managed.ComputeHash(Encoding.ASCII.GetBytes(data));

                return Convert.ToBase64String(hash);
            }
        }

        public bool Send(byte[] message, bool dontWait, bool more)
        {
            int frameSize = 2 + 1 + message.Length;
            int payloadStartIndex = 2;
            int payloadLength = message.Length + 1;

            if (payloadLength > 125)
            {
                frameSize += 2;
                payloadStartIndex += 2;

                if (payloadLength > ushort.MaxValue)
                {
                    frameSize += 6;
                    payloadStartIndex += 6;
                }
            }

            byte[] frame = new byte[frameSize];

            frame[0] = (byte) 0x81; // Text and Final      

            // No mask
            frame[1] = 0x00;

            if (payloadLength <= 125)
            {
                frame[1] |= (byte) (payloadLength & 127);
            }
            else
            {
                // TODO: implement
            }

            // more byte
            frame[payloadStartIndex] = (byte) (more ? '1' : '0');
            payloadStartIndex++;

            // payload
            Buffer.BlockCopy(message, 0, frame, payloadStartIndex, message.Length);

            try
            {
                if (dontWait)
                {
                    return _streamSocket.TrySendFrame(Identity, Identity.Length, true) &&
                           _streamSocket.TrySendFrame(frame, frame.Length);
                }
                else
                {
                    _streamSocket.SendMoreFrame(Identity, Identity.Length);
                    _streamSocket.SendFrame(frame, frame.Length);

                    return true;
                }
            }
            catch (NetMQException exception)
            {
                State = WebSocketClientState.Closed;
                throw exception;
            }
        }

        public void Close()
        {
            // TODO: send close message     
            if (_streamSocket.TrySendFrame(Identity, Identity.Length, true))
                _streamSocket.TrySendFrame("");

            State = WebSocketClientState.Closed;
        }

        public void Dispose()
        {
            _decoder.Message -= OnMessage;
        }
    }
}