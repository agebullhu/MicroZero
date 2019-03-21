using Agebull.Common.Logging;
using Agebull.MicroZero.PubSub;
using System;

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
        public override void Handle(PublishItem item)
        {
            try
            {
                //WebSocketNotify.Publish("mq", item.Title, item.SubTitle, item.Content);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
        }
    }
}
