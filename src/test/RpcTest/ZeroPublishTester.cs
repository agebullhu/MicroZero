using System;
using System.Threading;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using ApiTest;
using Newtonsoft.Json;

namespace RpcTest
{

    public class DeviceEventArgument
    {
        public string DeviceName { get; set; }
        public string EventName { get; set; }

        public string ArgumentJson { get; set; }

        public string Requester { get; set; }

        public DateTime Time { get; set; }

    }
    internal class ZeroPublishTester : Tester
    {
        public override bool Init()
        {
            return SystemManager.Instance.TryInstall(Tester.Station, "api");
        }

        private Random random = new Random((int) (DateTime.Now.Ticks % int.MaxValue));
        protected override void DoAsync()
        {
            ZeroPublisher.Publish("DeviceInstruct","","",new DeviceEventArgument
            {
                EventName= "Message",
                ArgumentJson = "Test From ZeroNet"
            });

        }

    }
}