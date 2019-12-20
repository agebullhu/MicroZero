using System.Threading;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
using Agebull.MicroZero.ZeroApis;

namespace RpcTest
{
    internal class ZeroTester : Tester
    {
        public override bool Init()
        {
            var task = SystemManager.Instance.TryInstall(Station, "api");
            task.Wait();
            return task.Result;
        }

        protected override void DoTest()
        {
            var client = new ApiClient
            {
                Station = Station,
                Commmand = Api,
                Argument = Arg
            };
            client.CallCommand();

            if (client.State < ZeroOperatorStateType.Failed)
            {
                Interlocked.Increment(ref SuCount);
            }
            else if (client.State < ZeroOperatorStateType.Error)
            {
                Interlocked.Increment(ref BugError);
            }
            else if (client.State < ZeroOperatorStateType.NetTimeOut)
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