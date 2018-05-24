using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class StationConfig : SimpleConfig, IDisposable, IApiResultData
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember, JsonProperty("station_name")]
        public string StationName { get => _name; set => _name = value; }
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember, JsonProperty("short_name")]
        public string ShortName { get; set; }
        /// <summary>
        /// 站点别名
        /// </summary>
        [DataMember, JsonProperty("station_alias")]
        public List<string> StationAlias { get; set; }
        /// <summary>
        /// 站点类型
        /// </summary>
        [DataMember, JsonProperty("station_type")]
        public int StationType { get; set; }
        /// <summary>
        /// 入站端口
        /// </summary>
        [DataMember, JsonProperty("request_port")]
        public int RequestPort { get; set; }

        /// <summary>
        /// 入站地址
        /// </summary>
        [DataMember, JsonProperty]
        public string RequestAddress => ZeroIdentityHelper.GetRemoteAddress(StationName, RequestPort);

        /// <summary>
        /// 入站端口
        /// </summary>
        [DataMember, JsonProperty("worker_port")]
        public int WorkerPort { get; set; }

        /// <summary>
        /// 出站地址
        /// </summary>
        [DataMember, JsonProperty]
        public string WorkerAddress => ZeroIdentityHelper.GetRemoteAddress(StationName, WorkerPort);

        /// <summary>
        /// 请求入
        /// </summary>
        [DataMember, JsonProperty("request_in")]
        public long RequestIn { get; set; }
        /// <summary>
        /// 请求出
        /// </summary>
        [DataMember, JsonProperty("request_out")]
        public long RequestOut { get; set; }
        /// <summary>
        /// 请求错误
        /// </summary>
        [DataMember, JsonProperty("request_err")]
        public long RequestErr { get; set; }

        /// <summary>
        /// 调用回
        /// </summary>
        [DataMember, JsonProperty("worker_in")]
        public long WorkerIn { get; set; }
        /// <summary>
        /// 调用出
        /// </summary>
        [DataMember, JsonProperty("worker_out")]
        public long WorkerOut { get; set; }
        /// <summary>
        /// 调用错
        /// </summary>
        [DataMember, JsonProperty("worker_err")]
        public long WorkerErr { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [DataMember, JsonProperty("station_state")]
        public StationState State { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [DataMember, JsonProperty("state")]
        public string __ => State.ToString();


        /// <summary>
        /// 状态
        /// </summary>
        [DataMember, JsonProperty("workers")]
        public List<string> Workers { get; set; }

        /// <summary>
        /// 站点类型
        /// </summary>
        [DataMember, JsonProperty]
        public string TypeName
        {
            get
            {
                switch (StationType)
                {
                    default:
                        return "Error";
                    case ZeroStation.StationTypeApi:
                        return "API";
                    case ZeroStation.StationTypeDispatcher:
                        return "Dispatcher";
                    case ZeroStation.StationTypeMonitor:
                        return "Monitor";
                    case ZeroStation.StationTypePublish:
                        return "Publish";
                    case ZeroStation.StationTypeVote:
                        return "Vote";
                }
            }
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="src"></param>
        public void Copy(StationConfig src)
        {
            StationName = src.StationName;
            StationAlias = src.StationAlias;
            StationType = src.StationType;
            RequestPort = src.RequestPort;
            WorkerPort = src.WorkerPort;
            State = src.State;
            RequestIn = src.RequestIn;
            RequestOut = src.RequestOut;
            RequestErr = src.RequestErr;
            WorkerIn = src.WorkerIn;
            WorkerOut = src.WorkerOut;
            WorkerErr = src.WorkerErr;
            Workers = src.Workers;
            _sockets = src._sockets;
            lock (_pools)
            {
                _pools = src._pools;
            }
        }

        /// <summary>
        /// Socket名称标识
        /// </summary>
        private int _socketId;

        /// <summary>
        /// 所有连接
        /// </summary>
        [IgnoreDataMember, JsonIgnore] private List<RequestSocket> _sockets = new List<RequestSocket>();

        /// <summary>
        /// 连接池
        /// </summary>
        [IgnoreDataMember, JsonIgnore] private Queue<RequestSocket> _pools = new Queue<RequestSocket>();

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        internal RequestSocket GetSocket()
        {
            if (_isDisposed)
                return null;
            lock (_pools)
            {
                if (_pools.Count != 0)
                    return _pools.Dequeue();
            }
            var socket = new RequestSocket();
            socket.Options.Identity = ZeroIdentityHelper.ToZeroIdentity(ShortName ?? StationName, (++_socketId).ToString());
            socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 10);
            socket.Options.ReconnectIntervalMax = new TimeSpan(0, 0, 0, 0, 500);
            socket.Options.TcpKeepalive = true;
            socket.Options.TcpKeepaliveIdle = new TimeSpan(0, 1, 0);
            socket.Connect(RequestAddress);
            _sockets.Add(socket);
            return socket;
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        internal void Close(RequestSocket socket)
        {
            if (socket == null)
                return;
            _sockets.Remove(socket);
            socket.CloseSocket(RequestAddress);
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        internal void Free(RequestSocket socket)
        {
            if (socket == null)
                return;
            lock (_pools)
            {
                if (_isDisposed || _pools.Count > 99)
                {
                    socket.Disconnect(RequestAddress);
                    socket.Close();
                    socket.Dispose();
                    _sockets.Remove(socket);
                }
                else
                {
                    _pools.Enqueue(socket);
                }
            }
        }
        /// <summary>
        /// 是否已析构
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        private bool _isDisposed;

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public void Resume()
        {
            Dispose();
            _isDisposed = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            while (_pools.Count != _sockets.Count)
            {
                Thread.Sleep(10);
            }
            lock (_pools)
            {
                _pools.Clear();
            }
            foreach (var socket in _sockets)
            {
                socket.CloseSocket(RequestAddress);
            }
            _sockets.Clear();
        }
    }
}
