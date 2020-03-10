using System;
using System.Collections.Generic;
using Agebull.Common.Logging;
using ZeroMQ;
using System.Linq;
using System.Threading;
using Agebull.Common.Context;
using Agebull.EntityModel.Common;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     API客户端代理
    /// </summary>
    public class ApiProxy : ZeroStation
    {
        #region Task

        /// <summary>
        /// 代理地址
        /// </summary>
        private const string InprocAddress = "inproc://ApiProxy.req";

        /// <summary>
        /// 本地代理
        /// </summary>
        private ZSocketEx _proxyServiceSocket;


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
            if (!StationProxy.TryGetValue(station, out var item) || item.Config.State != ZeroCenterState.Run)
                return null;
            return ZSocketEx.CreateOnceSocket(InprocAddress,item.Config.ServiceKey, name.ToZeroBytes(), ZSocketType.PAIR);
        }

        /// <summary>
        /// 站点是否已修改
        /// </summary>
        internal static bool IsChanged { get; set; }

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
        protected sealed override bool CheckConfig()
        {
            Config = new StationConfig
            {
                Name = StationName,
                Caption = "API客户端代理",
                IsBaseStation = true
            };
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
        protected sealed override bool Loop(/*CancellationToken token*/)
        {
            IsChanged = true;
            while (CanLoopEx)
            {
                try
                {
                    if (IsChanged)
                    {
                        _zmqPool = CreatePool();
                        IsChanged = false;
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
            using (message)
            {
                caller = message[0].ReadAll();
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
            ZeroTrace.WriteError("ApiProxy . OnLocalCall", error.Text);
            SendNetErrorResult(caller);
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
            if(result.State != ZeroOperatorStateType.Runing)
                Interlocked.Decrement(ref WaitCount);
            message2.Prepend(new ZFrame(result.Requester));
            SendResult(message2);
        }

        #endregion


        #region 配置


        /// <summary>
        /// 所有代理
        /// </summary>
        private static readonly Dictionary<string, StationProxyItem> StationProxy = new Dictionary<string, StationProxyItem>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 开始执行的处理
        /// </summary>
        /// <returns></returns>
        protected override void OnLoopBegin()
        {
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
            _proxyServiceSocket = ZSocketEx.CreateServiceSocket(InprocAddress,null, ZSocketType.ROUTER);
            ZeroTrace.SystemLog("ApiProxy", "Run", $"{RealName} : {Config.RequestAddress}");
            RealState = StationState.Run;
            ZeroApplication.OnObjectActive(this);
        }


        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        protected override void OnLoopComplete()
        {
            _proxyServiceSocket.Dispose();
            RealState = StationState.Closed;
            _zmqPool?.Dispose();

            foreach (var proxyItem in StationProxy.Values)
            {
                proxyItem.Socket?.Dispose();
            }
            StationProxy.Clear();
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
            //var added = StationProxy.Values.Where(p => p.Config.State > ZeroCenterState.Pause).Select(p => p.Socket).ToArray();
            var alive = StationProxy.Values.ToArray();
            var list = new ZSocketEx[alive.Length + 1];
            list[0] = _proxyServiceSocket;
            for (int idx = 0; idx < alive.Length; idx++)
            {
                var item = alive[idx];
                if (item.Socket == null)
                    item.Socket = ZSocketEx.CreatePoolSocket(item.Config.RequestAddress,item.Config.ServiceKey, ZSocketType.DEALER, ZSocket.CreateIdentity(false, item.Config.Name));
                list[idx + 1] = item.Socket;
            }
            if (_zmqPool == null)
                _zmqPool = ZmqPool.CreateZmqPool();

            _zmqPool.Sockets = list;
            var oldPtr = _zmqPool.RePrepare(ZPollEvent.In);
            oldPtr?.Dispose();
            return _zmqPool;
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

