using System;
using System.Buffers.Text;
using System.Collections.Generic;
using Agebull.Common.Logging;
using ZeroMQ;
using System.Linq;
using System.Text;
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
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override Task<bool> CheckConfig()
        {
            Config = new StationConfig
            {
                Name = StationName,
                Caption = "API客户端代理",
                IsBaseStation = true
            };
            return Task.FromResult(true);
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
                LogRecorderX.MonitorTrace($"{StationName}({error.Name}) : {error.Text}");
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e, "ApiStation.SendResult");
                LogRecorderX.MonitorTrace(e.Message);
            }
#if UNMANAGE_MONEY_CHECK
            finally
            {
                LogRecorderX.MonitorTrace($"MemoryCheck:{MemoryCheck.AliveCount}");
            }
#endif
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
        protected sealed override async Task<bool> Loop(/*CancellationToken token*/)
        {
            await Task.Yield();
            _isChanged = true;
            while (CanLoopEx)
            {
                try
                {
                    if (_isChanged)
                    {
                        _zmqPool = CreatePool();
                    }
                    if (!await _zmqPool.PollAsync())
                    {
                        continue;
                    }

                    var message = await _zmqPool.CheckInAsync(0);
                    if (message != null)
                    {
                        OnLocalCall(message);
                    }

                    for (int idx = 1; idx < _zmqPool.Size; idx++)
                    {
                        message = await _zmqPool.CheckInAsync(idx);
                        if (message != null)
                        {
                            OnRemoteResult(message);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogRecorderX.Exception(e);
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
            StationProxyItem item;
            ZMessage message2;
            byte[] caller;
            string str;
            using (message)
            {
                caller = message[0].ReadAll();
                str = Encoding.UTF8.GetString(caller);
                var station = message[1].ToString();
                if (!StationProxy.TryGetValue(station, out item))
                {
                    SendLayoutErrorResult(message[0].ReadAll());
                    return;
                }
                message2 = message.Duplicate(2);
            }

            bool success;
            ZError error;
            using (message2)
            {
                success = item.Socket.SendMessage(message2, ZSocketFlags.DontWait, out error);
            }

            if (success)
                return;
            if (long.TryParse(str, out var id) && _tasks.TryGetValue(id, out var src))
            {
                src.SetResult(new ZeroResult
                {
                    State = ZeroOperatorStateType.LocalSendError,
                    ZmqError = error
                });
            }
            else
            {
                ZeroTrace.WriteError("ApiProxy . OnLocalCall", error.Text);
                SendNetErrorResult(caller);
            }
        }


        private void OnRemoteResult(ZMessage message)
        {
            var message2 = message.Duplicate();
            var result = ZeroResultData.Unpack<ZeroResult>(message, true, (res, type, bytes) =>
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
            if (long.TryParse(result.Requester, out var id) && _tasks.TryGetValue(id, out var src))
            {
                if (result.State != ZeroOperatorStateType.Runing)
                    src.SetResult(result);
            }
            else
            {
                message2.Prepend(new ZFrame(result.Requester ?? result.RequestId));
                SendResult(message2);
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
            ZeroTrace.SystemLog("ApiProxy", "Run", $"{RealName} : {Config.RequestAddress}");
            RealState = StationState.Run;
            ZeroApplication.OnObjectActive(this);
        }


        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        protected override Task OnLoopComplete()
        {
            ZeroApplication.ZeroNetEvents.Remove(OnZeroNetEvent);
            _proxyServiceSocket.Dispose();
            RealState = StationState.Closed;
            _zmqPool?.Dispose();

            foreach (var proxyItem in StationProxy.Values)
            {
                proxyItem.Socket?.Dispose();
            }
            StationProxy.Clear();
            return Task.CompletedTask;
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
            //var added = StationProxy.Values.Where(p => p.Config.State > ZeroCenterState.Pause).Select(p => p.Socket).ToArray();
            var alive = StationProxy.Values.ToArray();
            var list = new ZSocket[alive.Length + 1];
            list[0] = _proxyServiceSocket;
            for (int idx = 0; idx < alive.Length; idx++)
            {
                var item = alive[idx];
                Console.WriteLine($"{item.Config.Name}:{item.Config.RequestAddress}");
                if (item.Socket == null)
                    item.Socket = ZSocketEx.CreatePoolSocket(item.Config.RequestAddress, item.Config.ServiceKey, ZSocketType.DEALER, ZSocket.CreateIdentity(false, item.Config.Name));
                list[idx + 1] = item.Socket;
            }
            _zmqPool = ZmqPool.CreateZmqPool();

            _zmqPool.Sockets = list;
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
            return ++_id;
        }

        private long CreateSocket(ProxyCaller2 caller, out ZSocketEx socket)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                caller.Result = ApiResultIoc.NoReadyJson;
                caller.State = ZeroOperatorStateType.LocalNoReady;
                socket = null;
                return 0;
            }

            if (!ZeroApplication.Config.TryGetConfig(caller.Station, out caller.Config))
            {
                caller.Result = ApiResultIoc.NoFindJson;
                caller.State = ZeroOperatorStateType.NotFind;
                socket = null;
                return 0;
            }

            //if (Config.State == ZeroCenterState.None || Config.State == ZeroCenterState.Pause)
            //{
            //    caller.Result = ApiResultIoc.PauseJson;
            //    caller.State = ZeroOperatorStateType.Pause;
            //    socket = null;
            //    return 0;
            //}

            //if (Config.State != ZeroCenterState.Run)
            //{
            //    caller.Result = ApiResultIoc.NotSupportJson;
            //    caller.State = ZeroOperatorStateType.Pause;
            //    socket = null;
            //    return 0;
            //}

            caller.State = ZeroOperatorStateType.None;
            socket = ApiProxy.GetProxySocket(caller.Name.ToString());
            socket.ServiceKey = caller.Config.ServiceKey;
            return caller.Name;
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Task<ZeroResult> Send(ProxyCaller2 caller, byte[] description, params byte[][] args)
        {
            return SendToZero(caller, description, args);
        }


        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Task<ZeroResult> SendToZero(ProxyCaller2 caller, byte[] description, IEnumerable<byte[]> args)
        {
            var id = CreateSocket(caller, out var socket);
            if(id == 0)
                return Task.FromResult(new ZeroResult
                {
                    State = ZeroOperatorStateType.NoWorker,
                });
            using (MonitorScope.CreateScope("SendToZero"))
            using (var message = new ZMessage())
            {
                message.Add(new ZFrame(caller.Station));
                message.Add(new ZFrame(description));
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        message.Add(new ZFrame(arg));
                    }
                    message.Add(new ZFrame(caller.Config.ServiceKey));
                }

                var res = socket.SendTo(message);
                if (!res)
                {
                    return Task.FromResult(new ZeroResult
                    {
                        State = ZeroOperatorStateType.LocalSendError,
                        ZmqError = socket.LastError
                    });
                }
            }

            var src = new TaskCompletionSource<ZeroResult>();
            _tasks.Add(id, src);
            return src.Task;
        }


        private readonly Dictionary<long, TaskCompletionSource<ZeroResult>> _tasks = new Dictionary<long, TaskCompletionSource<ZeroResult>>();


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
        public ZSocket Socket { get; set; }

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

