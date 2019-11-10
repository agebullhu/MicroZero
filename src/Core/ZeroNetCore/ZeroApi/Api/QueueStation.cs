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
        protected override IZmqPool PrepareLoop(byte[] identity, out ZSocket socket)
        {
            socket = ZSocket.CreatePoolSocket(Config.WorkerResultAddress, Config.ServiceKey, ZSocketType.DEALER, identity);

            var pool = ZmqPool.CreateZmqPool();
            pool.Prepare(ZPollEvent.In, ZSocket.CreateSubSocket(Config.WorkerCallAddress, Config.ServiceKey, identity, Subscribe), socket);

            Task.Factory.StartNew(Reload);

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
                CacheProcess(long.Parse(item.LocalId), state == ZeroOperatorStateType.Ok);

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
                item.LocalId.ToZeroBytes()
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
            CacheProcess(long.Parse(item.LocalId), false);
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

        private readonly object _localObj = new object();

        private QueueData _data = new QueueData();



        void SaveIds()
        {
            try
            {
                File.WriteAllText(FileName, JsonHelper.SerializeObject(_data));
            }
            catch (Exception ex)
            {
                LogRecorderX.Exception(ex);
            }
        }

        string _fileName;
        string FileName => _fileName ?? (Path.Combine(ZeroApplication.Config.DataFolder, StationName + ".json"));

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <returns></returns>
        private void Reload()
        {
            Thread.Sleep(1000);
            QueueData queueData = null;
            try
            {
                if (File.Exists(FileName))
                {
                    var json = File.ReadAllText(FileName);
                    if (string.IsNullOrEmpty(json))
                    {
                        return;
                    }
                    queueData = JsonConvert.DeserializeObject<QueueData>(json);
                }
            }
            catch //(Exception e)
            {
                return;
            }
            if (queueData == null)
            {
                return;
            }
            long[] ids;
            lock (_localObj)
            {
                _data = queueData;
                if(_data.FailedIds.Count == 0)
                    _data.FailedIds = _data.FailedIds.Distinct().ToList();
                ids = _data.FailedIds.ToArray();
            }
            using (var cmd = new QueueCommand())
            {
                if (!cmd.Prepare(Config.RequestAddress, Config.ServiceKey, StationName))
                    return;
                foreach (var id in ids)
                {
                    cmd.CallCommand(id, id);
                }
                cmd.CallCommand(_data.Max, 0);
            }
        }

        /// <summary>
        /// 准备执行
        /// </summary>
        /// <returns></returns>
        protected override bool PrepareExecute(ApiCallItem item)
        {
            lock (_localObj)
            {
                if (_data.Max == 0)
                    return true;
                var nowId = long.Parse(item.LocalId);
                return _data.Max < nowId || _data.FailedIds.Contains(nowId);
            }
        }


        bool CacheProcess(long nowId, bool success)
        {
            lock (_localObj)
            {
                if (!success)
                    _data.FailedIds.Add(nowId);
                else
                {
                    if (_data.Max < nowId)
                        _data.Max = nowId;
                    _data.FailedIds.Remove(nowId);
                }
            }
            SaveIds();
            return true;
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
        ZSocket socket;

        static readonly byte[] description =
        {
            3,
            (byte)ZeroByteCommand.Restart,
            ZeroFrameType.Argument,
            ZeroFrameType.Argument,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        public bool Prepare(string address,byte[] serviceKey, string stationName)
        {
            if (socket != null)
                return true;
            try
            {
                socket = ZSocket.CreateOnceSocket(address, serviceKey, ZSocket.CreateIdentity(false, stationName));
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
        public ZeroResult CallCommand(long start, long end)
        {
            try
            {
                var result = new ZSimpleCommand().SendTo(socket, description, start.ToString(), end.ToString());
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

        protected override void OnDispose() => socket?.Dispose();
    }
}