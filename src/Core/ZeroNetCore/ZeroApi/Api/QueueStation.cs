using Agebull.Common.Base;
using Agebull.Common.Logging;
using Agebull.MicroZero.ZeroApis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        protected override IZmqPool PrepareLoop(byte[] identity, out ZSocketEx socket)
        {
            socket = ZSocketEx.CreatePoolSocket(Config.WorkerResultAddress, Config.ServiceKey, ZSocketType.DEALER, identity);

            var pool = ZmqPool.CreateZmqPool();
            pool.Prepare(ZPollEvent.In, ZSocketEx.CreateSubSocket(Config.WorkerCallAddress,Config.ServiceKey, identity, Subscribe), socket);

            var queueData = LoadData() ?? new QueueData();
            long[] ids;
            lock (data)
            {
                data.Max = queueData.Max;
                data.FailedIds = queueData.FailedIds;
                if (data.FailedIds.Count == 0)
                    data.FailedIds = data.FailedIds.Distinct().ToList();
                ids = data.FailedIds.ToArray();
            }
            _= ReNotify(ids);

            return pool;
        }

        #region 内部重载


        /// <summary>
        ///     配置校验
        /// </summary>
        protected override ZeroStationOption GetApiOption()
        {
            var option = ZeroApplication.GetApiOption(StationName);
            option.SpeedLimitModel = SpeedLimitType.Single;
            return option;
        }

        /// <inheritdoc />
        protected override async Task OnLoopComplete()
        {
            string json;
            lock (data)
            {
                json = JsonHelper.SerializeObject(data);
            }
            await SaveIds(json);
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal override async Task<bool> OnExecuestEnd(ZSocket socket, ApiCallItem item, ZeroOperatorStateType state)
        {
            if (!string.IsNullOrEmpty(item.LocalId) && long.TryParse(item.LocalId, out var id))
                await Ack(id, state == ZeroOperatorStateType.Ok);

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
                Config.ServiceKey
            };
            return await SendResult(socket, new ZMessage(msg));
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal override async Task SendLayoutErrorResult(ZSocket socket, ApiCallItem item)
        {
            if (!string.IsNullOrEmpty(item.LocalId) && long.TryParse(item.LocalId, out var id))
                await Ack(id, false);
        }

        ///// <summary>
        ///// 空转
        ///// </summary>
        ///// <returns></returns>
        //protected override void OnLoopIdle()
        //{
        //    SaveIds();
        //}

        #endregion

        #region 已处理集合


        private readonly QueueData data = new QueueData();



        async Task SaveIds(string json)
        {
            try
            {
                await File.WriteAllTextAsync(FileName, json);
            }
            catch (Exception ex)
            {
                LogRecorderX.Exception(ex);
            }
        }

        private string _fileName;
        private string FileName => _fileName ?? (_fileName = Path.Combine(ZeroApplication.Config.DataFolder, StationName + ".json"));

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <returns></returns>
        private async Task ReNotify(long[] ids)
        {
            Thread.Sleep(1000);

            using (var cmd = new QueueCommand())
            {
                if (!cmd.Prepare(Config.RequestAddress, StationName))
                    return;
                foreach (var id in ids)
                {
                    await cmd.CallCommand(id, id);
                }
                long max;
                lock (data)
                {
                    max = data.Max;
                }
                await cmd.CallCommand(max, 0);
            }
        }

        QueueData LoadData()
        {
            try
            {
                if (File.Exists(FileName))
                {
                    var json = File.ReadAllText(FileName);
                    if (!string.IsNullOrEmpty(json))
                        return JsonConvert.DeserializeObject<QueueData>(json);
                }
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
            }
            return null;
        }
        /// <summary>
        /// 准备执行
        /// </summary>
        /// <returns></returns>
        protected override bool PrepareExecute(ApiCallItem item)
        {
            lock (data)
            {
                if (data.Max == 0)
                    return true;
                var nowId = long.Parse(item.LocalId);
                return data.Max < nowId || data.FailedIds.Contains(nowId);
            }
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        protected override async Task OnLoopIdle()
        {
            using (var cmd = new QueueCommand())
            {
                if (!cmd.Prepare(Config.RequestAddress, StationName))
                    return;
                long max;
                lock (data)
                {
                    max = data.Max;
                }
                await cmd.CallCommand(max, 0);
            }
        }

        async Task Ack(long nowId, bool success)
        {
            string json;
            lock (data)
            {
                if (!success)
                    data.FailedIds.Add(nowId);
                else
                    data.FailedIds.Remove(nowId);

                if (data.Max < nowId)
                    data.Max = nowId;
                json = JsonHelper.SerializeObject(data);
            }
            await SaveIds(json);
        }
        #endregion
    }
    internal class QueueData
    {
        /// <summary>
        /// 最大消费的ID
        /// </summary>
        public long Max { get; set; }

        /// <summary>
        /// 处理出错的ID
        /// </summary>
        public List<long> FailedIds = new List<long>();
    }

    /// <summary>
    /// 重发队列数据的命令
    /// </summary>
    internal class QueueCommand : ScopeBase
    {
        ZSocketEx socket;

        static readonly byte[] description =
        {
            3,
            (byte)ZeroByteCommand.Restart,
            ZeroFrameType.Argument,
            ZeroFrameType.Argument,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        public bool Prepare(string address, string stationName)
        {
            if (socket != null || !ZeroApplication.Config.TryGetConfig(stationName,out var cfg))
                return true;
            
            try
            {
                socket = ZSocketEx.CreateOnceSocket(address, cfg.ServiceKey, ZSocket.CreateIdentity(false, stationName));
                return true;
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
                return false;
            }
        }
        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <returns></returns>
        public async Task<ZeroResult> CallCommand(long start, long end)
        {
            try
            {
                ZeroResult result;
                if (await socket.SendByServiceKey(description, start.ToString(), end.ToString()))
                    result = new ZeroResult
                    {
                        State = ZeroOperatorStateType.Ok,
                        InteractiveSuccess = true
                    };
                else
                    result = new ZeroResult
                    {
                        State = ZeroOperatorStateType.LocalRecvError,
                        ZmqError = socket.LastError
                    };
                return !result.InteractiveSuccess ? result : await socket.Receive<ZeroResult>();
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

        protected override void OnDispose() => socket?.Dispose();
    }
}