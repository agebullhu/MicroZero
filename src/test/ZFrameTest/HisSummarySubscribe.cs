using Agebull.Common.Logging;
using Agebull.MicroZero.PubSub;
using System;
using System.Threading.Tasks;

namespace ZFrameTest
{
    /// <summary>
    /// 指令消息订阅
    /// </summary>
    public class HisSummarySubscribe : SubStation<PublishItem>
    {
        /// <summary>
        /// 构造
        /// </summary>
        public HisSummarySubscribe()
        {
            Name = "HisSummary";
            StationName = "HisSummary";
            IsRealModel = true;
        }

        /// <inheritdoc />
        /// <summary>执行命令</summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Task Handle(PublishItem item)
        {
            return Task.CompletedTask;
            //try
            //{
            //    WebSocketNotify.Publish("mq", item.Title, item.SubTitle, item.Content);
            //}
            //catch (Exception ex)
            //{
            //    LogRecorderX.Exception(ex);
            //}
        }
    }
}
