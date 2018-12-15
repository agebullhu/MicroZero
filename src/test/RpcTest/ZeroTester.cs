using System;
using System.Globalization;
using System.Threading;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using ApiTest;
using Newtonsoft.Json;

namespace RpcTest
{
    internal class ZeroTester : Tester
    {
        public override bool Init()
        {
            return SystemManager.Instance.TryInstall(Station, "api");
        }

        private Random random = new Random((int) (DateTime.Now.Ticks % int.MaxValue));
        protected override void DoAsync()
        {
            var arg = new MachineEventArg
            {
                EventName = "OpenDoor",
                MachineId = $"Machine-{random.Next()}",
                JsonStr = JsonConvert.SerializeObject(new OpenDoorArg
                {
                    CompanyId ="公司id",
                    UserType ="用户类型",
                    UserId = random.Next().ToString(),
                    DeviceId ="设备id",
                    RecordDate=DateTime.Now.ToString("yyyy-MM-dd"),
                    RecordUserStatus="状态",
                    InOrOut= $"{((random.Next() % 2) == 1 ? "进" : "出")}",
                    EnterType="进出方式",
                    PhotoUrl="人脸照",
                    IdentityImageUrl="证件照",
                    PanoramaUrl="全景照",
                    Score="识别系数"
                })
            };
            ApiClient client = new ApiClient
            {
                Station = Station,
                Commmand = Api,
                Argument = JsonConvert.SerializeObject(arg)
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