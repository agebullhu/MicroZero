using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Gboxt.Common.DataModel;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class StationConfig : IDisposable
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember, JsonProperty("station_name", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string StationName { get; set; }
        /// <summary>
        /// 站点别名
        /// </summary>
        [DataMember, JsonProperty("station_alias", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> StationAlias { get; set; }
        /// <summary>
        /// 站点类型
        /// </summary>
        [DataMember, JsonProperty("station_type", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int StationType { get; set; }
        /// <summary>
        /// 入站端口
        /// </summary>
        [DataMember, JsonProperty("out_port", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int OutPort { get; set; }

        /// <summary>
        /// 入站地址
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string OutAddress => $"tcp://{StationProgram.Config.ZeroAddress}:{OutPort}";

        /// <summary>
        /// 入站端口
        /// </summary>
        [DataMember, JsonProperty("inner_port", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int InnerPort { get; set; }

        /// <summary>
        /// 出站地址
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string InnerAddress => $"tcp://{StationProgram.Config.ZeroAddress}:{InnerPort}";

        /// <summary>
        /// 心跳端口
        /// </summary>
        [DataMember, JsonProperty("heart_port", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int HeartPort { get; set; }

        /// <summary>
        /// 心跳地址
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string HeartAddress => $"tcp://{StationProgram.Config.ZeroAddress}:{HeartPort}";

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="src"></param>
        public void Copy(StationConfig src)
        {
            StationName = src.StationName;
            StationAlias = src.StationAlias;
            StationType = src.StationType;
            OutPort = src.OutPort;
            InnerPort = src.InnerPort;
            HeartPort = src.HeartPort;

        }

        /// <summary>
        /// 状态
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public StationState State { get; set; }

        /// <summary>
        /// 所有连接
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public readonly List<RequestSocket> Sockets = new List<RequestSocket>();

        /// <summary>
        /// 连接池
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public readonly Queue<RequestSocket> Pools = new Queue<RequestSocket>();

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        internal RequestSocket GetSocket()
        {
            if (_isDisposed)
                return null;
            lock (Pools)
            {
                if (Pools.Count != 0)
                    return Pools.Dequeue();
            }
            var socket = new RequestSocket();
            socket.Options.Identity = $"{StationProgram.Config.RealName}-{Sockets.Count +1}".ToAsciiBytes();
            socket.Options.ReconnectInterval = new TimeSpan(0, 0, 1);
            socket.Options.DisableTimeWait = true;
            socket.Connect(OutAddress);
            Sockets.Add(socket);
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
            socket.Disconnect(OutAddress);
            socket.Close();
            socket.Dispose();
            Sockets.Remove(socket);
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        internal void Free(RequestSocket socket)
        {
            if (socket == null)
                return;
            lock (Pools)
            {
                if (_isDisposed || Pools.Count > 99)
                {
                    socket.Disconnect(OutAddress);
                    socket.Close();
                    socket.Dispose();
                    Sockets.Remove(socket);
                }
                else
                {
                    Pools.Enqueue(socket);
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
        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            while (Pools.Count != Sockets.Count)
            {
                Thread.Sleep(10);
            }
            lock (Pools)
            {
                Pools.Clear();
                foreach (var socket in Pools)
                {
                    socket.Disconnect(OutAddress);
                    socket.Close();
                    socket.Dispose();
                }
                Sockets.Clear();
            }
        }
    }
}
