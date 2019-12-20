using System;
using System.Threading;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;

using Agebull.MicroZero.PubSub;
using Agebull.EntityModel.Common;

namespace RpcTest
{
    internal class QueueTester : Tester
    {
        public override bool Init()
        {
            return true;
        }

        protected override void DoTest()
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
                    LogRecorderX.Exception(e);
                }
            }
        }
    }
}