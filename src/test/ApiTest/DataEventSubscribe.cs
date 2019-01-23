using System;
using System.Threading;
using System.Timers;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.PubSub;
using Gboxt.Common.DataModel.ExtendEvents;
using Newtonsoft.Json;
using Timer = System.Threading.Timer;

namespace ApiTest
{
    /// <summary>
    /// 指令消息订阅
    /// </summary>
    public class DataEventSubscribe : SubStation<PublishItem>
    {
        /// <summary>
        /// 构造
        /// </summary>
        public DataEventSubscribe()
        {
            Name = "EntityEvent";
            StationName = "EntityEvent";
            IsRealModel = true;
            Subscribe = "";
        }


        /// <summary>
        /// 在上次处理后是否有数据更新，用于定时器决定是否处理数据
        /// </summary>
        private int _eventCount;

        /// <inheritdoc />
        /// <summary>执行命令</summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override void Handle(PublishItem item)
        {
            if (item == null)
                return;
            EntityEventArgument argument;
            try
            {
                argument = JsonConvert.DeserializeObject<EntityEventArgument>(item.Content);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                return;
            }

            if (argument.ValueType != EntityEventValueType.EntityJson)
                return;
            Interlocked.Increment(ref _eventCount);
        }

    }
}