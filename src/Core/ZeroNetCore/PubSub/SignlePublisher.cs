using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Agebull.Common.Rpc;
using Agebull.Common.Tson;
using Agebull.ZeroNet.Core;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     消息发布器
    /// </summary>
    public abstract class SignlePublisher<TData> : ZeroStation
        where TData : class, IPublishData
    {
        #region 广播
        /// <summary>
        /// 构造
        /// </summary>
        protected SignlePublisher() : base(ZeroStationType.Notify, false)
        {
            //Hearter = SystemManager.Instance;
        }

        private readonly List<ZSocket> sockets = new List<ZSocket>();

        private readonly Random random = new Random((int)DateTime.Now.Ticks % int.MaxValue);

        private List<TData> datas;
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="data"></param>
        public void Publish(TData data)
        {
            if (data == null)
                return;
            if (!CanLoop)
            {
                datas.Add(data);
                return;
            }
            PushToLocalQueue(data);
        }
        /// <summary>
        /// 数据写到本地发布队列中
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PushToLocalQueue(TData data)
        {
            int idx = random.Next(0, 64);
            var socket = sockets[idx];
            lock (socket)
            {
                try
                {
                    if (TsonOperator != null)
                    {
                        byte[] buf;
                        using (TsonSerializer serializer = new TsonSerializer())
                        {
                            TsonOperator.ToTson(serializer, data);
                            buf = serializer.Close();
                        }
                        socket.SendPublish(ZeroPublisher.PubDescriptionTson,data.Title.ToZeroBytes(), buf);
                    }
                    else
                    {
                        socket.SendPublish(ZeroPublisher.PubDescriptionJson, data.Title.ToZeroBytes(), data.ToZeroBytes());
                    }
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("Pub", e, socket.Endpoint,
                        $"Socket Ptr:{socket.SocketPtr}");
                    datas.Add(data);
                }
            }
        }

        /// <inheritdoc />
        protected override bool OnStart()
        {
            _inporcName = $"inproc://{StationName}_{RandomOperate.Generate(8)}.pub";
            return base.OnStart();

        }
        /// <summary>
        /// TSON序列化操作器
        /// </summary>
        protected ITsonOperator<TData> TsonOperator { get; set; }

        /// <inheritdoc />
        protected override void OnRunStop()
        {
            foreach (var socket in sockets)
                socket.TryClose();
            sockets.Clear();
            base.OnRunStop();
        }

        /// <inheritdoc />
        protected override void DoDestory()
        {
            if (datas.Count > 0)
            {
                var file = CacheFileName;
                try
                {
                    File.WriteAllText(file, JsonConvert.SerializeObject(datas));
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(StationName, e, file);
                }
            }
            base.DoDestory();
        }

        #endregion

        #region Task

        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private string CacheFileName => Path.Combine(ZeroApplication.Config.DataFolder, $"{StationName}.{Name}.sub.json");


        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override void Initialize()
        {
            var file = CacheFileName;
            if (File.Exists(file))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    datas = JsonConvert.DeserializeObject<List<TData>>(json);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(StationName, e, file);
                }
            }

            if (datas == null)
                datas = new List<TData>();
        }

        private string _inporcName;


        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            using (var socket = ZSocket.CreateDealerSocket(Config.RequestAddress, Identity))
            {
                var poll = ZmqPool.CreateZmqPool();
                poll.Prepare(ZPollEvent.In, ZSocket.CreateServiceSocket(_inporcName, ZSocketType.PULL));
                for (int i = 0; i < 64; i++)
                {
                    sockets.Add(ZSocket.CreateClientSocketByInproc(_inporcName, ZSocketType.PUSH));
                }
                Hearter?.HeartReady(StationName, RealName);
                State = StationState.Run;
                Thread.Sleep(5);
                //历史数据重新入列
                foreach (var data in datas)
                {
                    PushToLocalQueue(data);
                }
                datas.Clear();
                File.Delete(CacheFileName);
                using (poll)
                {
                    while (CanLoop)
                    {
                        if (poll.Poll() && poll.CheckIn(0, out var message))
                        {
                            using (message)
                                Send(socket, message);
                        }
                    }
                }
                Hearter?.HeartLeft(StationName, RealName);
            }
            return true;
        }
        /// <summary>
        /// 执行发送
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        protected virtual void Send(ZSocket socket, ZMessage message)
        {
            if (!socket.SendTo(message))
            {
                ZeroTrace.WriteError(StationName, "Pub", socket.LastError.Text, socket.Endpoint);
            }
        }
        #endregion
    }
}