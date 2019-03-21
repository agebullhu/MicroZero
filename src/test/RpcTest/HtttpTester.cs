using System.Net;
using System.Threading;
using MicroZero.Http.Route;

namespace RpcTest
{
    internal class HttpTester : Tester
    {
        public override bool Init()
        {
            return true;
        }

        protected override void DoAsync()
        {
            HttpApiCaller caller = new HttpApiCaller(Host);
            caller.CreateRequest($"{Station}/{Api}","POST",Arg);
            var result = caller.GetResult().Result;
            if (caller.Status != WebExceptionStatus.Success)
                Interlocked.Increment(ref NetError);
        }
    }
}