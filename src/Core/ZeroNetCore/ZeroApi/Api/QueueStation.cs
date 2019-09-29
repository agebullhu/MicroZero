using Agebull.Common.Logging;
using Agebull.MicroZero.ZeroApis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZeroMQ;

namespace Agebull.MicroZero.PubSub
{
    /// <summary>
    /// 消息订阅站点
    /// </summary>
    public class QueueStation : ApiStationBase
    {
        /// <summary>
        /// 订阅主题
        /// </summary>
        public string Subscribe { get; set; } = "";

        /// <summary>
        /// 构造
        /// </summary>
        protected QueueStation() : base(ZeroStationType.Queue, true)
        {

        }

        /// <summary>
        /// 构造Pool
        /// </summary>
        /// <returns></returns>
        protected override IZmqPool PrepareLoop(byte[] identity, out ZSocket socket)
        {
            socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, identity);
            var pool = ZmqPool.CreateZmqPool();
            pool.Prepare(ZPollEvent.In, ZSocket.CreateSubSocket(Config.WorkerCallAddress, identity, Subscribe), socket);
            Reload();

            return pool;
        }

        #region 内部重载


        /// <summary>
        ///     配置校验
        /// </summary>
        protected override ZeroStationOption GetApiOption()
        {
            var option = ZeroApplication.GetApiOption(StationName);
            if (option.SpeedLimitModel == SpeedLimitType.None)
                option.SpeedLimitModel = SpeedLimitType.Single;
            return option;
        }

        /// <inheritdoc />
        protected override void OnLoopComplete()
        {
            SaveIds();
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal override bool OnExecuestEnd(ZSocket socket, ApiCallItem item, ZeroOperatorStateType state)
        {
            if (!string.IsNullOrEmpty(item.LocalId))
                CacheProcess(long.Parse(item.LocalId));

            var des = new byte[]
            {
                3,
                (byte) state,
                ZeroFrameType.Requester,
                ZeroFrameType.LocalId,
                ZeroFrameType.SerivceKey,
                ZeroFrameType.ResultEnd
            };
            var msg = new List<byte[]>
            {
                item.Caller,
                des,
                item.Requester.ToZeroBytes(),
                item.LocalId.ToZeroBytes(),
                ZeroCommandExtend.ServiceKeyBytes
            };
            return SendResult(socket, new ZMessage(msg));
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal override void SendLayoutErrorResult(ZSocket socket, ApiCallItem item)
        {
            if (string.IsNullOrEmpty(item.LocalId))
                return;
            CacheProcess(long.Parse(item.LocalId));
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        protected override void OnLoopIdle()
        {
            SaveIds();
        }

        #endregion

        #region 已处理集合

        long _isProcess;

        void SaveIds()
        {
            if (Interlocked.Read(ref _isProcess) == 0)
                return;
            lock (localObj)
            {
                try
                {
                    var fileName = Path.Combine(ZeroApplication.Config.DataFolder, StationName + ".json");

                    File.WriteAllText(fileName, JsonHelper.SerializeObject(processIds));
                    Interlocked.Exchange(ref _isProcess, 0);
                }
                catch (Exception ex)
                {
                    LogRecorderX.Exception(ex);
                }
            }
        }
        private readonly object localObj = new object();

        private List<long> processIds = new List<long>();


        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <returns></returns>
        private void Reload()
        {
            long pre = 0;
            lock (localObj)
            {
                Interlocked.Exchange(ref _isProcess, 0);
                try
                {
                    var fileName = Path.Combine(ZeroApplication.Config.DataFolder, StationName + ".json");
                    if (File.Exists(fileName))
                    {
                        var json = File.ReadAllText(fileName);
                        if (!string.IsNullOrEmpty(json))
                        {
                            processIds = JsonConvert.DeserializeObject<List<long>>(json) ?? new List<long>();
                        }
                        else
                        {
                            processIds = new List<long>();
                        }
                    }
                }
                catch //(Exception e)
                {
                    processIds = new List<long>();
                }

                if (processIds.Count > 0)
                {
                    pre = processIds[0];
                    for (int i = 1; i < processIds.Count; i++)
                    {
                        if (processIds[i] - pre > 1)
                        {
                            CallCommand(pre, processIds[i]);
                        }

                        pre = processIds[i];
                    }
                }
            }

            CallCommand(pre, 0);
        }
        static readonly byte[] description =
        {
            3,
            (byte)ZeroByteCommand.Restart,
            ZeroFrameType.Argument,
            ZeroFrameType.Argument,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <returns></returns>
        public ZeroResult CallCommand(long start, long end)
        {
            try
            {
                var socket = ZSocket.CreateDealerSocket(Config.RequestAddress, ZSocket.CreateIdentity(false, StationName));
                var result = ZSimpleCommand.SendTo(socket, description, start.ToString(), end.ToString());
                return !result.InteractiveSuccess ? result : socket.ReceiveString();
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
                return new ZeroResult
                {
                    InteractiveSuccess = false,
                    Exception = e
                };
            }
        }
        /// <summary>
        /// 准备执行
        /// </summary>
        /// <returns></returns>
        protected override bool PrepareExecute(ApiCallItem item)
        {
            lock (localObj)
            {
                if (processIds.Count == 0)
                    return true;
                var nowId = long.Parse(item.LocalId);
                return processIds[0] < nowId && !processIds.Contains(nowId);
            }
        }


        bool CacheProcess(long nowId)
        {
            lock (localObj)
            {
                if (processIds.Count == 0)
                    processIds.Add(nowId);
                else
                {
                    if (processIds[0] >= nowId)
                        return true;
                    if (processIds[0] + 1 == nowId)
                    {
                        processIds[0] = nowId;
                    }
                    else
                    {
                        for (int i = 0; i < processIds.Count; i++)
                        {
                            if (processIds[i] > nowId)
                            {
                                processIds.Insert(i, nowId);
                                nowId = 0;
                                break;
                            }
                        }

                        if (nowId > 0)
                            processIds.Add(nowId);
                    }

                    var pre = processIds[0];
                    for (int i = 1; i < processIds.Count; i++)
                    {
                        if (processIds[i] == pre + 1)
                        {
                            pre = processIds[i];
                            processIds.RemoveAt(--i);
                        }
                        else pre = processIds[i];
                    }
                }
            }
            Interlocked.Decrement(ref _isProcess);
            return true;
        }
        #endregion
    }
}