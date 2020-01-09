using System.Threading.Tasks;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using ZeroMQ;

namespace MicroZero.Http.Route
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
            IsRealModel = true;
        }

        ///// <summary>
        ///// 空转
        ///// </summary>
        ///// <returns></returns>
        //protected override void OnLoopIdle()
        //{
        //    //DoPublish();
        //}

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PlanItem args)
        {
            PlanManage.OnPlanEvent(args.ZeroEvent, args.Plan).Wait();
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="planItem"></param>
        /// <returns></returns>
        protected override bool Unpack(ZMessage frames, out PlanItem planItem)
        {
            return PlanItem.Unpack(frames, out planItem);
        }
    }
}