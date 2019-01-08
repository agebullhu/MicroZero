using System;
using System.Threading;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;

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
            try
            {
                ZeroPublisher.Publish("WechatCallBack", "test", "test", "{}");
                Interlocked.Increment(ref ExCount);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
    }
}