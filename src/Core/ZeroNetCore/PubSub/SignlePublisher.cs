using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Agebull.Common.Tson;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.Log;
using Agebull.ZeroNet.ZeroApi;
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
        protected SignlePublisher() : base(ZeroStationType.Publish, false)
        {
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

                        socket.SendTo(ZeroPublishExtend.PubDescriptionTson,
                            data.Title.ToZeroBytes(),
                            ApiContext.RequestContext.RequestId.ToZeroBytes(),
                            ZeroApplication.Config.RealName.ToZeroBytes(),
                            buf);
                    }
                    else
                    {
                        socket.SendTo(ZeroPublishExtend.PubDescriptionJson,
                            data.Title.ToZeroBytes(),
                            ApiContext.RequestContext.RequestId.ToZeroBytes(),
                            ZeroApplication.Config.RealName.ToZeroBytes(),
                            data.ToZeroBytes());
                    }
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("Pub", e, socket.Connects.LinkToString(','),
                        $"Socket Ptr:{socket.SocketPtr}");
                    datas.Add(data);
                }
            }
        }


        /// <inheritdoc />
        protected override bool OnStart()
        {
            inporcName = $"inproc://{StationName}_{RandomOperate.Generate(8)}.pub";
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
        /// 数据进入的处理
        /// </summary>
        protected virtual void OnSend(TData data)
        {

        }


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

        private string inporcName;


        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            using (var socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity))
            {
                foreach (var data in datas)
                {
                    socket.Publish(data);
                }
                datas.Clear();
                File.Delete(CacheFileName);
                using (var poll = ZmqPool.CreateZmqPool())
                {
                    poll.Prepare(ZPollEvent.In,
                        ZSocket.CreateServiceSocket(inporcName, ZSocketType.PULL));
                    for (int i = 0; i < 64; i++)
                    {
                        sockets.Add(ZSocket.CreateClientSocket(inporcName, ZSocketType.PUSH));
                    }
                    SystemManager.Instance.HeartReady(StationName, RealName);
                    State = StationState.Run;
                    Thread.Sleep(5);
                    foreach (var data in datas)
                    {
                        socket.Publish(data);
                    }
                    datas.Clear();
                    while (true)
                    {
                        if (poll.CheckIn(0, out var message))
                            socket.SendTo(message);
                        else if (!CanLoop)//保证发送完成
                            break;
                    }
                    SystemManager.Instance.HeartLeft(StationName, RealName);
                }
            }
            return true;
        }

        #endregion
    }
}