using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Agebull.Common.Ioc;
using Agebull.Common.Tson;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.PubSub
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
            //Hearter = SystemManager.Instance;
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        public string Subscribe { get; set; } = "";

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
        public abstract void Handle(TPublishItem args);

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        void DoHandle(TPublishItem args)
        {
            using (IocScope.CreateScope())
            {
                try
                {
                    Handle(args);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Name, e, args.Content);
                }
            }
        }


        //private string inporcName;

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected override bool RunInner(CancellationToken token)
        {
            Hearter?.HeartReady(StationName, RealName);
            //using (var socket = ZSocket.CreateClientSocket(inporcName, ZSocketType.PAIR))
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(ZPollEvent.In,
                    ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.SUB, Identity, Subscribe));
                State = StationState.Run;
                while (CanLoop)
                {
                    if (!pool.Poll())
                    {
                        Idle();
                    }
                    else if (pool.CheckIn(0, out var message))
                    {
                        if (Unpack(message, out var item))
                        {
                            DoHandle(item);
                        }

                        //socket.SendTo(message);
                    }
                }
            }

            Hearter?.HeartLeft(StationName, RealName);
            return true;
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual bool Unpack(ZMessage messages, out TPublishItem item)
        {
            if (messages == null)
            {
                item = null;
                return false;
            }
            try
            {
                if (messages.Count < 3)
                {
                    item = null;
                    return false;
                }
                var description = messages[1].Read();
                if (description.Length < 2)
                {
                    item = null;
                    return false;
                }

                int end = description[0] + 2;
                if (end != messages.Count)
                {
                    item = null;
                    return false;
                }

                item = new TPublishItem
                {
                    Title = messages[0].ReadString(),
                    State = (ZeroOperatorStateType)description[1],
                    ZeroEvent = (ZeroNetEventType)description[1]
                };

                for (int idx = 2; idx < end; idx++)
                {
                    var bytes = messages[idx].Read();
                    if (bytes.Length == 0)
                        continue;
                    switch (description[idx])
                    {
                        case ZeroFrameType.SubTitle:
                            item.SubTitle = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.Station:
                            item.Station = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.Publisher:
                            item.Publisher = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.TextContent:
                            if (item.Content == null)
                                item.Content = Encoding.UTF8.GetString(bytes);
                            else
                                item.Values.Add(Encoding.UTF8.GetString(bytes));
                            break;
                        case ZeroFrameType.BinaryValue:
                            item.Buffer = bytes;
                            break;
                        case ZeroFrameType.TsonValue:
                            item.Tson = bytes;
                            break;
                        default:
                            item.Values.Add(Encoding.UTF8.GetString(bytes));
                            break;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Unpack", e);
                item = null;
                return false;
            }
            finally
            {
                messages.Dispose();
            }
        }

        /// <summary>
        /// 命令处理任务
        /// </summary>
        protected virtual void HandleTask()
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
        }


        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Initialize()
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
                    int size = serializer.ReadLen();
                    for (int idx = 0; !serializer.IsBad && idx < size; idx++)
                    {
                        using (var scope = TsonObjectScope.CreateScope(serializer))
                        {
                            if (scope.DataType != TsonDataType.Empty)
                            {
                                var item = new TData();
                                TsonOperator.FromTson(serializer, item);
                                list.Add(item);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }

                return list;
            }

            if (args.Content != null)
                return JsonConvert.DeserializeObject<List<TData>>(args.Content);
            if (args.Buffer != null)
            {
                using (MemoryStream ms = new MemoryStream(args.Buffer))
                {
                    DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(List<TData>));
                    return (List<TData>)js.ReadObject(ms);
                }
            }

            return new List<TData>();
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
            if (args.Buffer != null)
            {
                using (MemoryStream ms = new MemoryStream(args.Buffer))
                {
                    DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(TData));
                    return (TData)js.ReadObject(ms);
                }
            }

            return default(TData);
        }

    }
}