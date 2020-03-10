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
            pool.Prepare(ZPollEvent.In, ZSocketEx.CreateSubSocket(Config.WorkerCallAddress, Config.ServiceKey, identity, Subscribe), socket);

            LoadData();
            Task.Factory.StartNew(ReNotify);

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
            SaveData();
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        protected override void OnLoopIdle()
        {
            //防丢处理
            using (var cmd = new QueueCommand())
            {
                if (!cmd.Prepare(Config.ServiceKey, Config.RequestAddress, StationName))
                    return;
                long max;
                lock (_data)
                {
                    max = _data.Max;
                }
                cmd.CallCommand(max, 0);
            }
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
            if (!string.IsNullOrEmpty(item.LocalId) && long.TryParse(item.LocalId,out var id))
                Ack(id, state == ZeroOperatorStateType.Ok);

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
            if (!string.IsNullOrEmpty(item.LocalId) && long.TryParse(item.LocalId, out var id))
                Ack(id, false);
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

        private readonly QueueData _data = new QueueData();


        /// <summary>
        /// 准备执行
        /// </summary>
        /// <returns></returns>
        protected override bool PrepareExecute(ApiCallItem item)
        {
            lock (_data)
            {
                if (_data.Max == 0)
                    return true;
                var nowId = long.Parse(item.LocalId);
                return _data.Max < nowId || _data.FailedIds.Contains(nowId);
            }
        }


        void Ack(long nowId, bool success)
        {
            lock (_data)
            {
                if (!success)
                    _data.FailedIds.Add(nowId);
                else
                    _data.FailedIds.Remove(nowId);

                if (_data.Max < nowId)
                    _data.Max = nowId;
            }
            SaveData();
        }


        private string _fileName;

        /// <summary>
        /// 存储订阅状态的文件名称
        /// </summary>
        private string FileName => _fileName ?? (_fileName = Path.Combine(ZeroApplication.Config.DataFolder, StationName + ".json"));

        void LoadData()
        {
            try
            {
                if (!File.Exists(FileName))
                {
                    return;
                }

                var json = File.ReadAllText(FileName);
                if (string.IsNullOrEmpty(json))
                {
                    return;
                }
                var queueData = JsonConvert.DeserializeObject<QueueData>(json);
                if (queueData?.FailedIds == null || queueData.Max <= 0)
                    return;
                if (queueData.FailedIds.Count > 0)
                    queueData.FailedIds = queueData.FailedIds.Distinct().ToList();
                lock (_data)
                {
                    _data.Max = queueData.Max;
                    _data.FailedIds = queueData.FailedIds;
                }
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
            }

        }

        void SaveData()
        {
            try
            {
                string json;
                lock (_data)
                {
                    json=JsonHelper.SerializeObject(_data);
                }
                File.WriteAllText(FileName, json);
            }
            catch (Exception ex)
            {
                LogRecorderX.Exception(ex);
            }
        }

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <returns></returns>
        private void ReNotify()
        {
            Thread.Sleep(1000);
            long[] ids;
            lock (_data)
            {
                ids = _data.FailedIds.ToArray();
            }
            using (var cmd = new QueueCommand())
            {
                if (!cmd.Prepare(Config.ServiceKey, Config.RequestAddress, StationName))
                    return;
                foreach (var id in ids)
                {
                    cmd.CallCommand(id, id);
                }
                long max;
                lock (_data)
                {
                    max = _data.Max;
                }
                cmd.CallCommand(max, 0);
            }
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
        private ZSocketEx _socket;

        /// <summary>
        /// 网络数据格式定义
        /// </summary>
        private static readonly byte[] Description =
        {
            3,
            (byte)ZeroByteCommand.Restart,
            ZeroFrameType.Argument,
            ZeroFrameType.Argument,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="address"></param>
        /// <param name="stationName"></param>
        /// <returns></returns>
        public bool Prepare(byte[] serviceKey, string address, string stationName)
        {
            if (_socket != null)
                return true;
            try
            {
                _socket = ZSocketEx.CreateOnceSocket(address, serviceKey, ZSocket.CreateIdentity(false, stationName));
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
                ZeroResult result;
                using (var message = new ZMessage(Description, start.ToString(), end.ToString()))
                {
                    if (_socket.SendByServiceKey(message))
                        result = new ZeroResult
                        {
                            State = ZeroOperatorStateType.Ok,
                            InteractiveSuccess = true
                        };
                    else
                        result = new ZeroResult
                        {
                            State = ZeroOperatorStateType.LocalRecvError,
                            ZmqError = _socket.LastError
                        };
                }
                return !result.InteractiveSuccess ? result : _socket.ReceiveString();
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

        /// <summary>清理资源</summary>
        protected override void OnDispose() => _socket?.Dispose();
    }
}