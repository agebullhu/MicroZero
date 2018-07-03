using System;
using System.Threading;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    internal class ZeroTester : Tester
    {
        public override bool Init()
        {
            return SystemManager.Instance.TryInstall(Tester.Station, "api");
        }

        protected override void DoAsync()
        {
            ApiClient client = new ApiClient
            {
                Station = Station,
                Commmand = Api,
                Argument = Arg
            };
            client.CallCommand();
            if (client.State < ZeroOperatorStateType.Failed)
            {
            }
            else if (client.State < ZeroOperatorStateType.Error)
            {
                Interlocked.Increment(ref BlError);
            }
            else if (client.State < ZeroOperatorStateType.TimeOut)
            {
                Interlocked.Increment(ref WkError);
            }
            else if (client.State > ZeroOperatorStateType.LocalNoReady)
            {
                Interlocked.Increment(ref ExError);
            }
            else
            {
                Interlocked.Increment(ref NetError);
            }
        }

    }
}