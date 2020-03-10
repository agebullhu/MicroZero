using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Agebull.Common.Logging;
using ZeroMQ;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.EntityModel.Common;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     API客户端代理
    /// </summary>
    public class ApiProxy : ZeroStation
    {
        #region Socket

        /// <summary>
        /// 代理地址
        /// </summary>
        private const string InprocAddress = "inproc://ApiProxy.req";

        /// <summary>
        /// 本地代理
        /// </summary>
        private ZSocket _proxyServiceSocket;


        /// <summary>
        /// 取得连接器
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public static ZSocketEx GetSocket(string station)
        {
            return GetSocket(station, RandomOperate.Generate(8));
        }

        /// <summary>
        /// 取得连接器
        /// </summary>
        /// <param name="station"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ZSocketEx GetSocket(string station, string name)
        {
            if (!StationProxy.TryGetValue(station, out var item))// || item.Config.State != ZeroCenterState.Run
                return null;
            return ZSocketEx.CreateOnceSocket(InprocAddress, item.Config.ServiceKey, name.ToZeroBytes(), ZSocketType.PAIR);
        }

        /// <summary>
        /// 取得连接器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ZSocketEx GetProxySocket(string name = null)
        {
            return ZSocketEx.CreateOnceSocket(InprocAddress, null, name.ToZeroBytes(), ZSocketType.PAIR);
        }

        /// <summary>
        /// 站点是否已修改
        /// </summary>
        private static bool _isChanged;

        #endregion

        #region 流程

        /// <summary>
        /// 等待数量
        /// </summary>
        public int WaitCount;

        /// <summary>
        /// 实例
        /// </summary>
        public static ApiProxy Instance { get; private set; }

        /// <summary>
        /// 构造
        /// </summary>
        public ApiProxy() : base(ZeroStationType.Proxy, false)
        {
            StationName = "_api_proxy_";
            Instance = this;
            Config = new StationConfig
            {
                Name = StationName,
                Caption = "API客户端代理",
                IsBaseStation = true
            };
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override bool CheckConfig()
        {
            return true;
        }

        private static readonly byte[] LayoutErrorFrame = new byte[]
        {
            0,
            (byte) ZeroOperatorStateType.FrameInvalid,
            ZeroFrameType.ResultEnd
        };

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private void SendLayoutErrorResult(byte[] caller)
        {
            SendResult(new ZMessage
            {
                new ZFrame(caller),
                new ZFrame(LayoutErrorFrame)
            });
            Interlocked.Decrement(ref WaitCount);
        }

        private static readonly byte[] NetErrorFrame = new byte[]
        {
            0,
            (byte) ZeroOperatorStateType.NetError,
            ZeroFrameType.ResultEnd
        };

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private void SendNetErrorResult(byte[] caller)
        {
            SendResult(new ZMessage
            {
                new ZFrame(caller),
                new ZFrame(NetErrorFrame)
            });
            Interlocked.Decrement(ref WaitCount);
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private void SendResult(ZMessage message)
        {
            try
            {
                ZError error;
                lock (_proxyServiceSocket)
                {
                    using (message)
                    {
                        if (_proxyServiceSocket.Send(message, out error))
                            return;
                    }
                }

                ZeroTrace.WriteError(StationName, error.Text, error.Name);
                LogRecorder.MonitorTrace(() => $"{StationName}({error.Name}) : {error.Text}");
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "ApiStation.SendResult");
                LogRecorder.MonitorTrace(e.Message);
            }
        }

        #endregion

        #region 主循环

        /// <summary>
        /// 能不能循环处理
        /// </summary>
        protected bool CanLoopEx => WaitCount > 0 || CanLoop;
        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool Loop(/*CancellationToken token*/)
        {
            _isChanged = true;
            while (CanLoopEx)
            {
                try
                {
                    if (_isChanged)
                    {
                        _zmqPool = CreatePool();
                    }
                    if (!_zmqPool.Poll())
                    {
                        continue;
                    }

                    if (_zmqPool.CheckIn(0, out var message))
                    {
                        OnLocalCall(message);
                    }

                    for (int idx = 1; idx < _zmqPool.Size; idx++)
                    {
                        if (_zmqPool.CheckIn(idx, out message))
                            OnRemoteResult(message);
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                }
            }

            return true;
        }



        #endregion

        #region 方法

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="message"></param>
        private void OnLocalCall(ZMessage message)
        {
            Interlocked.Increment(ref WaitCount);
            using (message)
            {
                var station = message[1].ToString();
                if (!StationProxy.TryGetValue(station, out var item))
                {
                    SendLayoutErrorResult(message[0].ReadAll());
                    return;
                }
                var message2 = message.Duplicate(2);
                bool success;
                ZError error;
                using (message2)
                {
                    success = item.Socket.SendMessage(message2, ZSocketFlags.DontWait, out error);
                }

                if (success)
                    return;
                ZeroTrace.WriteError("ApiProxy . OnLocalCall", error.Text);
                var caller = message[0].ReadAll();
                SendNetErrorResult(caller);
            }

        }

        private void OnRemoteResult(ZMessage message)
        {
            long id;
            ZeroResult result;
            using (message)
            {
                result = ZeroResultData.Unpack<ZeroResult>(message, true, (res, type, bytes) =>
                {
                    switch (type)
                    {
                        case ZeroFrameType.ResultText:
                            res.Result = ZeroNetMessage.GetString(bytes);
                            return true;
                        case ZeroFrameType.BinaryContent:
                            res.Binary = bytes;
                            return true;
                    }
                    return false;
                });
                if (result.State != ZeroOperatorStateType.Runing)
                    Interlocked.Decrement(ref WaitCount);
                if (!long.TryParse(result.Requester, out id))
                {
                    using (var message2 = message.Duplicate())
                    {
                        message2.Prepend(new ZFrame(result.Requester ?? result.RequestId));
                        SendResult(message2);
                    }
                    return;
                }
            }

            if (!Tasks.TryGetValue(id, out var src))
                return;
            src.Caller.State = result.State;
            if (result.State == ZeroOperatorStateType.Runing)
            {
                LogRecorder.Trace($"task:({id})=>Runing");
                return;
            }
            Tasks.TryRemove(id, out _);
            LogRecorder.Trace("OnRemoteResult");
            if (!src.TaskSource.TrySetResult(result))
            {
                LogRecorder.Error($"task:({id})=>Failed result({JsonHelper.SerializeObject(result)})");
            }
        }

        #endregion


        #region 配置


        /// <summary>
        /// 所有代理
        /// </summary>
        internal static readonly Dictionary<string, StationProxyItem> StationProxy = new Dictionary<string, StationProxyItem>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 开始执行的处理
        /// </summary>
        /// <returns></returns>
        protected override void OnLoopBegin()
        {
            ZeroApplication.ZeroNetEvents.Add(OnZeroNetEvent);
            var identity = GlobalContext.ServiceRealName.ToZeroBytes();
            foreach (var config in ZeroApplication.Config.GetConfigs())
            {
                StationProxy.Add(config.StationName, new StationProxyItem
                {
                    Config = config,
                    Open = DateTime.Now,
                    Socket = ZSocketEx.CreateLongLink(config.RequestAddress, config.ServiceKey, ZSocketType.DEALER, identity)
                });
            }
            _proxyServiceSocket = ZSocketEx.CreateServiceSocket(InprocAddress, null, ZSocketType.ROUTER);
            ZeroTrace.SystemLog("ApiProxy", "Run", RealName);
            RealState = StationState.Run;
            ZeroApplication.OnObjectActive(this);
        }


        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        protected override void OnLoopComplete()
        {
            ZeroApplication.ZeroNetEvents.Remove(OnZeroNetEvent);
            _proxyServiceSocket.Dispose();
            _zmqPool?.Dispose();

            foreach (var proxyItem in StationProxy.Values)
            {
                proxyItem.Socket?.Dispose();
            }
            StationProxy.Clear();
            RealState = StationState.Closed;
        }

        private Task OnZeroNetEvent(ZeroAppConfigRuntime config, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.ConfigUpdate:
                case ZeroNetEventType.CenterStationDocument:
                case ZeroNetEventType.CenterStationJoin:
                case ZeroNetEventType.CenterStationInstall:
                case ZeroNetEventType.CenterStationResume:
                case ZeroNetEventType.CenterStationUpdate:
                    if (e.EventConfig?.StationName != null)
                        StationProxy.TryAdd(e.EventConfig.StationName, new StationProxyItem
                        {
                            Config = e.EventConfig
                        });
                    _isChanged = true;
                    break;
                case ZeroNetEventType.CenterStationLeft:
                case ZeroNetEventType.CenterStationPause:
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationRemove:
                case ZeroNetEventType.CenterStationStop:
                    _isChanged = true;
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Pool对象
        /// </summary>
        private IZmqPool _zmqPool;

        /// <summary>
        /// 构造Pool
        /// </summary>
        public IZmqPool CreatePool()
        {
            _isChanged = false;
            if (_zmqPool != null)
            {
                _zmqPool.Sockets = null;
                _zmqPool.Dispose();
            }
            //var added = StationProxy.Values.Where(p => p.Config.State > ZeroCenterState.Pause).Select(p => p.Socket).ToArray();
            var alive = StationProxy.Values.ToArray();
            var list = new List<ZSocket>
            {
                _proxyServiceSocket
            };
            foreach (var item in alive)
            {
                if (item.Config.State == ZeroCenterState.None || item.Config.State == ZeroCenterState.Run)
                {
                    if (item.Socket == null)
                    {
                        item.Socket = ZSocketEx.CreatePoolSocket(item.Config.RequestAddress, item.Config.ServiceKey, ZSocketType.DEALER, ZSocket.CreateIdentity(false, item.Config.Name));
                        item.Open = DateTime.Now;
                    }
                    list.Add(item.Socket);
                }
                else
                {
                    item.Socket?.Dispose();
                }
            }
            _zmqPool = ZmqPool.CreateZmqPool();
            _zmqPool.Sockets = list.ToArray();
            _zmqPool.RePrepare(ZPollEvent.In);
            return _zmqPool;
        }

        #endregion

        #region 异步


        private long _id;
        /// <summary>
        /// 取得Id
        /// </summary>
        /// <returns></returns>
        public long GetId()
        {
            return Interlocked.Increment(ref _id);
        }

        private long CreateSocket(ProxyCaller2 caller, out ZSocketEx socket)
        {
            //if (!ZeroApplication.ZerCenterIsRun)
            //{
            //    caller.Result = ApiResultIoc.NoReadyJson;
            //    caller.State = ZeroOperatorStateType.LocalNoReady;
            //    socket = null;
            //    return 0;
            //}

            //if (!StationProxy.TryGetValue(caller.Station, out var item))
            //{
            //    caller.Result = ApiResultIoc.NoFindJson;
            //    caller.State = ZeroOperatorStateType.NotFind;
            //    socket = null;
            //    return 0;
            //}
            //socket = item.Socket;

            if (!ZeroApplication.Config.TryGetConfig(caller.Station, out caller.Config))
            {
                caller.Result = ApiResultIoc.NoFindJson;
                caller.State = ZeroOperatorStateType.NotFind;
                socket = null;
                return 0;
            }

            if (Config.State != ZeroCenterState.None && caller.Config.State != ZeroCenterState.Run)
            {
                caller.Result = Config.State == ZeroCenterState.Pause
                    ? ApiResultIoc.PauseJson
                    : ApiResultIoc.NotSupportJson;
                caller.State = ZeroOperatorStateType.Pause;
                socket = null;
                return 0;
            }

            caller.State = ZeroOperatorStateType.None;
            socket = GetProxySocket(caller.Name.ToString());
            return caller.Name;
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Task<ZeroResult> CallZero(ProxyCaller2 caller, byte[] description, params byte[][] args)
        {
            return CallZero(caller, description, (IEnumerable<byte[]>)args);
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Task<ZeroResult> CallZero(ProxyCaller2 caller, byte[] description, IEnumerable<byte[]> args)
        {
            var info = new TaskInfo
            {
                Caller = caller,
                Start = DateTime.Now
            };
            if (!Tasks.TryAdd(caller.Name, info))
            {
                return Task.FromResult(new ZeroResult
                {
                    State = ZeroOperatorStateType.NoWorker,
                });
            }
            var id = CreateSocket(caller, out var socket);
            if (id == 0)
            {
                return Task.FromResult(new ZeroResult
                {
                    State = ZeroOperatorStateType.NoWorker,
                });
            }

            using (MonitorScope.CreateScope("SendToZero"))
            {
                using var message = new ZMessage
                {
                    new ZFrame(caller.Station),
                    new ZFrame(description)
                };
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        message.Add(new ZFrame(arg));
                    }
                    message.Add(new ZFrame(caller.Config.ServiceKey));
                }

                bool res;
                ZError error;
                using (socket)
                {
                    res = socket.Send(message, out error);
                }
                if (!res)
                {
                    return Task.FromResult(new ZeroResult
                    {
                        State = ZeroOperatorStateType.LocalSendError,
                        ZmqError = error
                    });
                }
            }

            info.TaskSource = new TaskCompletionSource<ZeroResult>();
            return info.TaskSource.Task;
        }


        internal readonly ConcurrentDictionary<long, TaskInfo> Tasks = new ConcurrentDictionary<long, TaskInfo>();


        internal class TaskInfo
        {
            /// <summary>
            /// TaskCompletionSource
            /// </summary>
            public TaskCompletionSource<ZeroResult> TaskSource;

            /// <summary>
            /// 任务开始时间
            /// </summary>
            public DateTime Start;

            /// <summary>
            /// 调用对象
            /// </summary>
            public ProxyCaller2 Caller;
        }

        #endregion
    }

    /// <summary>
    /// 站点代理节点
    /// </summary>
    public class StationProxyItem
    {
        /// <summary>
        /// 配置
        /// </summary>
        public StationConfig Config { get; set; }

        /// <summary>
        /// 远程连接
        /// </summary>
        public ZSocketEx Socket { get; set; }

        /// <summary>
        /// 打开时间
        /// </summary>
        public DateTime? Open { get; set; }

        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? Close { get; set; }
    }
}

