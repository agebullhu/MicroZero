using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Logging;
using NetMQ.Sockets;

namespace NetMQ.WebSockets
{
    public abstract class BaseShimHandler : IShimHandler
    {
        public int Id { get; set; }
        public StreamSocket Stream { get; set; }
        public NetMQPoller Poller { get; set; }

        internal Dictionary<byte[], WebSocketClient> Clients { get; } = new Dictionary<byte[], WebSocketClient>(new ByteArrayEqualityComparer());
        public PairSocket MessagesPipe { get; set; }

        /// <summary>
        /// µÿ÷∑
        /// </summary>
        public string Address { get; }

        protected BaseShimHandler(string addr,int id)
        {
            Id = id;
            Address = addr.Replace( "ws://","tcp://");
        }

        protected abstract void OnOutgoingMessage(NetMQMessage message);
        protected abstract void OnIncomingMessage(byte[] identity, NetMQMessage message);

        protected abstract void OnNewClient(byte[] identity);
        protected abstract void OnClientRemoved(byte[] identity);

        protected void WriteOutgoing(byte[] identity, byte[] message, bool more)
        {
            var outgoingData = Encode(message, more);

            Stream.SendMoreFrame(identity).SendFrame(outgoingData);
        }

        protected void WriteIngoing(NetMQMessage message)
        {
            MessagesPipe.SendMultipartMessage(message);
        }

        public void Close()
        {
            if (!isRuning)
                return;
            isRuning = true;
            Stream.Unbind(Address);
            MessagesPipe.Disconnect($"inproc://wsrouter-{Id}");

            Poller.Dispose();
            Monitor.Enter(this);
            try
            {
                Stream.Dispose();
                MessagesPipe.Dispose();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private bool isRuning;
        public void Run(PairSocket shim)
        {
            string shimAdd = $"inproc://wsrouter-{Id}";
            Monitor.Enter(this);
            try
            {
                isRuning = true;
                shim.SignalOK();

                shim.ReceiveReady += OnShimReady;

                MessagesPipe = new PairSocket();
                MessagesPipe.Connect(shimAdd);
                MessagesPipe.ReceiveReady += OnMessagePipeReady;

                Stream = new StreamSocket();
                Stream.Bind(Address);
                Stream.ReceiveReady += OnStreamReady;

                Poller = new NetMQPoller
                {
                    MessagesPipe,
                    shim,
                    Stream
                };
                MessagesPipe.SignalOK();
                Poller.Run();
                shim.Dispose();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void OnShimReady(object sender, NetMQSocketEventArgs e)
        {
            string command = e.Socket.ReceiveFrameString();

            switch (command)
            {
                case NetMQActor.EndShimMessage:
                    Poller.Stop();
                    break;
            }
        }

        private void OnStreamReady(object sender, NetMQSocketEventArgs e)
        {
            byte[] identity = Stream.ReceiveFrameBytes();

            if (!Clients.TryGetValue(identity, out var client))
            {
                client = new WebSocketClient(Stream, identity);
                client.IncomingMessage += OnIncomingMessage;
                Clients.Add(identity, client);

                OnNewClient(identity);
            }

            client.OnDataReady();

            if (client.State == WebSocketClientState.Closed)
            {
                Clients.Remove(identity);
                client.IncomingMessage -= OnIncomingMessage;
                OnClientRemoved(identity);
            }
        }

        private void OnIncomingMessage(object sender, NetMqMessageEventArgs e)
        {
            OnIncomingMessage(e.Identity, e.Message);
        }

        private void OnMessagePipeReady(object sender, NetMQSocketEventArgs e)
        {
            NetMQMessage request;
            try
            {
                request = MessagesPipe.ReceiveMultipartMessage();
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
                return;
            }
            OnOutgoingMessage(request);
        }

        private static byte[] Encode(byte[] data, bool more)
        {
            int frameSize = 2 + 1 + data.Length;
            int payloadStartIndex = 2;
            int payloadLength = data.Length + 1;

            if (payloadLength > 125)
            {
                frameSize += 2;
                payloadStartIndex += 2;

                if (payloadLength > 0xFFFF) // 2 bytes max value
                {
                    frameSize += 6;
                    payloadStartIndex += 6;
                }
            }

            byte[] outgoingData = new byte[frameSize];

            outgoingData[0] = (byte)0x82; // Binary and Final      

            // No mask
            outgoingData[1] = 0x00;

            if (payloadLength <= 125)
            {
                outgoingData[1] |= (byte)(payloadLength & 127);
            }
            else if (payloadLength <= 0xFFFF) // maximum size of short
            {
                outgoingData[1] |= 126;
                outgoingData[2] = (byte)((payloadLength >> 8) & 0xFF);
                outgoingData[3] = (byte)(payloadLength & 0xFF);
            }
            else
            {
                outgoingData[1] |= 127;
                outgoingData[2] = 0;
                outgoingData[3] = 0;
                outgoingData[4] = 0;
                outgoingData[5] = 0;
                outgoingData[6] = (byte)((payloadLength >> 24) & 0xFF);
                outgoingData[7] = (byte)((payloadLength >> 16) & 0xFF);
                outgoingData[8] = (byte)((payloadLength >> 8) & 0xFF);
                outgoingData[9] = (byte)(payloadLength & 0xFF);
            }

            // more byte
            outgoingData[payloadStartIndex] = (byte)(more ? 1 : 0);
            payloadStartIndex++;

            // payload
            Buffer.BlockCopy(data, 0, outgoingData, payloadStartIndex, data.Length);
            return outgoingData;
        }

    }
}