using System;
using System.Threading;
using NetMQ.Sockets;
using ZeroMQ;

namespace NetMQ.WebSockets
{
    /// <summary>
    /// WebSocket事件参数
    /// </summary>
    public class WsSocketEventArgs : EventArgs
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="wsSocket">WsSocket对象</param>
        public WsSocketEventArgs(WsSocket wsSocket)
        {
            Socket = wsSocket;
        }

        /// <summary>
        /// WsSocket对象
        /// </summary>
        public WsSocket Socket { get; }
    }
    /// <summary>
    /// WebSocket实现
    /// </summary>
    public class WsSocket : IDisposable, IOutgoingSocket, IReceivingSocket, ISocketPollable
    {
        #region 属性与字段

        /// <summary>
        /// 基础标识
        /// </summary>
        private static int _id = 0;
        /// <summary>
        /// 绑定命令
        /// </summary>
        internal const string BindCommand = "BIND";
        /// <summary>
        /// 绑定命令
        /// </summary>
        internal const string UnBindCommand = "UNBIND";
        /// <summary>
        /// 行为处理器
        /// </summary>
        private readonly NetMQActor _actor;
        /// <summary>
        /// 内部PairSocket对象
        /// </summary>
        private readonly PairSocket _messagesPipe;

        /// <summary>
        /// 内部Socket对象
        /// </summary>
        public NetMQSocket Socket => _messagesPipe;

        #endregion

        #region 构造与析构

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="shimCreator"></param>
        protected WsSocket(Func<int, IShimHandler> shimCreator)
        {
            int id = Interlocked.Increment(ref _id);

            _messagesPipe = new PairSocket();
            _messagesPipe.Bind($"inproc://wsrouter-{id}");
            _messagesPipe.ReceiveReady += OnMessagePipeReceiveReady;

            _actor = NetMQActor.Create(shimCreator(id));

            _messagesPipe.ReceiveSignal();
        }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets whether the object has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 地址绑定
        /// </summary>
        /// <param name="address"></param>
        public bool Bind(string address)
        {
            _actor.SendMoreFrame(BindCommand).SendFrame(address);

            byte[] bytes = _actor.ReceiveFrameBytes();
            int errorCode = BitConverter.ToInt32(bytes, 0);

            if (errorCode != 0)
            {
                Console.Error.WriteLine($"Bind {address} Failed");
                return false;
            }

            Address = address;
            return true;
        }

        /// <summary>
        /// 析构
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            if (Address != null)
            {
                _actor.SendMoreFrame(UnBindCommand).SendFrame(Address);
            }
            _actor.SendFrame(NetMQActor.EndShimMessage);
            
            _actor.Dispose();
            _messagesPipe.Options.Linger = TimeSpan.Zero;
            _messagesPipe.Dispose();
        }

        #endregion

        #region 事件

        /// <summary>
        /// 接收完成事件
        /// </summary>
        private void OnMessagePipeReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            ReceiveReady?.Invoke(this, new WsSocketEventArgs(this));
        }
        /// <summary>
        /// 接收完成事件
        /// </summary>
        public event EventHandler<WsSocketEventArgs> ReceiveReady;

        #endregion

        #region 基础收发方法

        /// <summary>
        /// 试图接收消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="timeout">超时设置</param>
        /// <returns></returns>
        public bool TryReceive(ref Msg msg, TimeSpan timeout)
        {
            return _messagesPipe.TryReceive(ref msg, timeout);
        }

        /// <summary>
        /// 试图发送消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="timeout">超时设置</param>
        /// <param name="more">是否还有后续</param>
        /// <returns></returns>
        public bool TrySend(ref Msg msg, TimeSpan timeout, bool more)
        {
            return _messagesPipe.TrySend(ref msg, timeout, more);
        }

        #endregion

    }
}