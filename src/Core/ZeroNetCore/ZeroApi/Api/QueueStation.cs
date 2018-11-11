using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ZeroMQ;

namespace Agebull.ZeroNet.PubSub
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
        protected override IZmqPool Prepare(byte[] identity, out ZSocket socket)
        {
            socket = null;
            var pool = ZmqPool.CreateZmqPool();
            var socket1 = ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.SUB, identity, Encoding.ASCII.GetBytes(Subscribe ?? ""));
            var socket2 = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.SUB, identity, identity);
            pool.Prepare(ZPollEvent.In, socket1, socket2);
            Reload();

            return pool;
        }

        #region 内部重载


        /// <summary>
        ///     配置校验
        /// </summary>
        protected override ZeroStationOption GetApiOption()
        {
            return new ZeroStationOption
            {
                SpeedLimitModel = SpeedLimitType.Single
            };
        }

        /// <inheritdoc />
        protected override void OnRunStop()
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
        internal override bool OnExecuestEnd(ref ZSocket socket, ApiCallItem item, ZeroOperatorStateType state)
        {
            if (string.IsNullOrEmpty(item.StationCallId))
                return true;
            return CacheProcess(long.Parse(item.StationCallId));
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal override void SendLayoutErrorResult(ref ZSocket socket, ApiCallItem item)
        {
            if (string.IsNullOrEmpty(item.StationCallId))
                return;
            CacheProcess(long.Parse(item.StationCallId));
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        public override void Idle()
        {
            SaveIds();
        }
        #endregion

        #region 已处理集合

        long isProcess = 0;

        void SaveIds()
        {
            if (Interlocked.Read(ref isProcess) == 0)
                return;
            try
            {
                var fileName = Path.Combine(ZeroApplication.Config.DataFolder, StationName + ".json");
                lock (processIds)
                    File.WriteAllText(fileName, JsonConvert.SerializeObject(processIds));
                Interlocked.Exchange(ref isProcess, 0);
            }
            catch
            {
            }
        }

        private List<long> processIds = new List<long>();


        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <returns></returns>
        private void Reload()
        {
            Interlocked.Exchange(ref isProcess, 0);
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
            if (processIds.Count == 0)
            {
                CallCommand(0, 0);
                return;
            }
            var pre = processIds[0];
            for (int i = 1; i < processIds.Count; i++)
            {
                if (processIds[i] - pre > 1)
                {
                    CallCommand(pre, processIds[i]);
                }
                pre = processIds[i];
            }
            CallCommand(pre, 0);
        }
        static byte[] description = new byte[]
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
        private ZeroResultData CallCommand(long start, long end)
        {
            try
            {
                var socket = ZSocket.CreateDealerSocket(Config.RequestAddress);
                var result = ZeroManageCommand.SendTo(socket, description, start.ToString(), end.ToString());
                if (!result.InteractiveSuccess)
                {
                    return result;
                }
                return socket.ReceiveString();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return new ZeroResultData
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
            if (processIds.Count == 0)
                return true;
            var nowId = long.Parse(item.StationCallId);
            lock (processIds)
                return processIds[0] < nowId && !processIds.Contains(nowId);
        }


        bool CacheProcess(long nowId)
        {
            lock (processIds)
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
            Interlocked.Decrement(ref isProcess);
            return true;
        }
        #endregion
    }
}