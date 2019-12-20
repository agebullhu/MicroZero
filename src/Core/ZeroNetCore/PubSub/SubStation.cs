using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Tson;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.MicroZero.PubSub
{
    /// <summary>
    /// 消息订阅站点
    /// </summary>
    public abstract class SubStation<TPublishItem> : ZeroStation
        where TPublishItem : PublishItem, new()
    {
        /// <summary>
        /// 构造
        /// </summary>
        protected SubStation() : base(ZeroStationType.Notify, true)
        {
            //Hearter = ZeroCenterProxy.Master;
        }
        /// <summary>
        /// 订阅主题
        /// </summary>
        public string Subscribe
        {
            get => Subscribes.FirstOrDefault();
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                if (Subscribes.Count == 0)
                    Subscribes.Add(value);
                else
                    Subscribes[0] = value;
            }
        }


        /// <summary>
        /// 订阅主题
        /// </summary>
        public readonly List<string> Subscribes = new List<string>();


        /// <summary>
        /// 是否实时数据(如为真,则不保存未处理数据)
        /// </summary>
        public bool IsRealModel { get; set; }

        /*// <summary>
        /// 命令处理方法 
        /// </summary>
        public Action<string> ExecFunc { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual void ExecCommand(string args)
        {
            ExecFunc?.Invoke(args);
        }*/

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract Task Handle(TPublishItem args);

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        async Task DoHandle(TPublishItem args)
        {
            using (IocScope.CreateScope())
            {
                args.RestoryContext(StationName);
                try
                {
                    await Handle(args);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(StationName, e, args.Content);
                }
                //finally
                //{
                //    GlobalContext.Current.Dispose();
                //    GlobalContext.SetUser(null);
                //    GlobalContext.SetRequestContext(null);
                //}
            }
        }


        //private string inporcName;

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected override async Task<bool> Loop(/*CancellationToken token*/)
        {
            //await Task.Yield();
            await Hearter.HeartReady(StationName, RealName);
            //using (var socket = ZSocket.CreateClientSocket(inporcName, ZSocketType.PAIR))
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(ZPollEvent.In, ZSocket.CreateSubSocket(Config.WorkerCallAddress, Identity, Subscribes));
                RealState = StationState.Run;
                while (CanLoop)
                {
                    if (!await pool.PollAsync())
                    {
                       await OnLoopIdle();
                    }
                    else if (!CanLoop)
                        continue;

                    var message = await pool.CheckInAsync(0);
                    if (message == null)
                        continue;
                    if (Unpack(message, out var item))
                    {
                        await DoHandle(item);
                    }

                    //socket.SendTo(message);
                }
            }
            return true;
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual bool Unpack(ZMessage msgs, out TPublishItem item)
        {
            return PublishItem.Unpack2(msgs, out item);
        }

        /*// <summary>
        /// 命令处理任务
        /// </summary>
        protected virtual Task HandleTask()
        {
            ZeroApplication.OnGlobalStart(this);
            //using (var pool = ZmqPool.CreateZmqPool())
            //{
            //    pool.Prepare(new[] { ZSocket.CreateServiceSocket(inporcName, ZSocketType.PAIR) }, ZPollEvent.In);
            //    while (ZeroApplication.IsAlive)
            //    {
            //        if (!pool.Poll())
            //        {
            //            Idle();
            //            continue;
            //        }
            //        if (pool.CheckIn(0, out var message) && message.Unpack(out var item))
            //        {
            //            Handle(item);
            //        }
            //    }
            //}
            ZeroApplication.OnGlobalEnd(this);

        }*/


        /// <summary>
        /// 初始化
        /// </summary>
        protected override void OnInitialize()
        {
            //inporcName = $"inproc://{StationName}_{RandomOperate.Generate(8)}.pub";
            //Task.Factory.StartNew(HandleTask);
        }

    }

    /// <summary>
    /// 消息订阅站点
    /// </summary>
    public abstract class SubStation : SubStation<PublishItem>
    {

    }

    /// <summary>
    /// 消息订阅站点
    /// </summary>
    public abstract class SubStation<TData, TPublishItem> : SubStation<TPublishItem>
        where TData : new()
        where TPublishItem : PublishItem, new()
    {
        /// <summary>
        /// TSON序列化操作器
        /// </summary>
        protected ITsonOperator<TData> TsonOperator { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected List<TData> DeserializeList(TPublishItem args)
        {
            if (args.Tson != null)
            {
                var list = new List<TData>();
                using (ITsonDeserializer serializer = new TsonDeserializer(args.Tson))
                {
                    serializer.ReadType();
                    var size = serializer.ReadLen();
                    for (var idx = 0; !serializer.IsBad && idx < size; idx++)
                    {
                        using (var scope = TsonObjectScope.CreateScope(serializer))
                        {
                            if (scope.DataType == TsonDataType.Empty)
                                continue;
                            var item = new TData();
                            TsonOperator.FromTson(serializer, item);
                            list.Add(item);
                        }
                    }
                }

                return list;
            }

            if (args.Content != null)
                return JsonConvert.DeserializeObject<List<TData>>(args.Content);
            if (args.Buffer == null)
                return new List<TData>();
            return JsonHelper.DeserializeObject<List<TData>>(args.Buffer.FromUtf8Bytes());
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected TData DeserializeObject(TPublishItem args)
        {
            if (args.Tson != null)
            {
                var item = new TData();
                using (var des = new TsonDeserializer(args.Buffer))
                    TsonOperator.FromTson(des, item);
                return item;
            }

            if (args.Content != null)
                return JsonConvert.DeserializeObject<TData>(args.Content);
            if (args.Buffer == null) return default;
            using (var ms = new MemoryStream(args.Buffer))
            {
                var js = new DataContractJsonSerializer(typeof(TData));
                return (TData)js.ReadObject(ms);
            }

        }

    }
}