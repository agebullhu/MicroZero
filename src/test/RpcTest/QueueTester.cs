using System;
using System.Threading;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.PubSub;
using Gboxt.Common.DataModel;

namespace RpcTest
{
    internal class QueueTester : Tester
    {
        public override bool Init()
        {
            return true;
        }

        protected override void DoAsync()
        {
            using (IocScope.CreateScope())
            {
                GlobalContext.Current.Request.RequestId = RandomOperate.Generate(8);
                try
                {
                    bool re = ZeroPublisher.Publish("WeixinMessage", "test", "test", "{}");
                    Interlocked.Increment(ref ExCount);
                    if (!re)
                        Interlocked.Increment(ref NetError);
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                }
            }
        }
    }
}