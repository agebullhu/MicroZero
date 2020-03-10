using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Tson;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using WebMonitor;
using ZeroMQ;

namespace ZeroNet.Http.Route
{

    /// <summary>
    /// 路由计数器
    /// </summary>
    public class PlanSubscribe : SubStation<ZeroPlan, PlanItem>
    {
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public PlanSubscribe()
        {
            Name = "PlanSubscribe";
            StationName = "PlanDispatcher";
            Subscribe = "";
            IsRealModel = true;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void DoDestory()
        {
            base.DoDestory();
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        public override void Idle()
        {
            //DoPublish();
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PlanItem args)
        {
            PlanManage.OnPlanEvent(args.ZeroEvent,args.Plan);
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool Unpack(ZMessage messages, out PlanItem item)
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

                item = new PlanItem
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
                        case ZeroFrameType.Context:
                            var json = Encoding.UTF8.GetString(bytes);
                            item.Plan = JsonConvert.DeserializeObject<ZeroPlan>(json);
                            break;
                        case ZeroFrameType.JsonValue:
                            item.CallResultJson = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.Status:
                            item.Status = bytes;
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

    }
}