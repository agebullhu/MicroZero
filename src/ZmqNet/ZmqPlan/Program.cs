using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;
using ZeroMQ;

namespace ZmqPlan
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.RegistZeroObject<TestPlanSub>();
            ZeroApplication.Initialize();
            SystemMonitor.ZeroNetEvent += SystemMonitor_ZeroNetEvent;
            ZeroApplication.Run();
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                CallCommand();
            }
            ZeroApplication.Shutdown();
        }

        private static void SystemMonitor_ZeroNetEvent(object sender, SystemMonitor.ZeroNetEventArgument e)
        {
            if (e.Event == ZeroNetEventType.AppRun)
            {
                Task.Factory.StartNew(CallCommand);
            }
        }

        public class TestPlanSub : SubStation
        {
            public TestPlanSub()
            {
                Name = "PlanDispatcher";
                StationName = "PlanDispatcher";
                IsRealModel = true;
            }
            public override void Handle(PublishItem args)
            {
                Console.Write("*");
                //ZeroTrace.WriteInfo("PlanDispatcher", args.Title,args.State.Text(), args.SubTitle, args.Content, args.GlobalId,
                //    args.Values.LinkToString(" > "));
            }
        }

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <returns></returns>
        public static void CallCommand()
        {
            if (!SystemManager.TryInstall("Test"))
                return;

            byte[] description = {
                3,
                ZeroByteCommand.Plan,
                ZeroFrameType.Plan,
                ZeroFrameType.Command,
                ZeroFrameType.Argument,
                ZeroFrameType.End
            };
            var plan = new ZeroPlan
            {
                plan_type = plan_date_type.minute,
                plan_value = 1,
                plan_repet = 1,
                description = "test:plan"
            };
            ZeroTrace.WriteInfo("PlanTest","Start plan");

            var socket = ZeroConnectionPool.GetSocket("Test", null);
            if (socket.Socket == null)
                return;
            var result = socket.Socket.SendTo(description,JsonConvert.SerializeObject(plan), "host", "*");
            if (!result.InteractiveSuccess)
            {
                ZeroTrace.WriteInfo("PlanTest","Send", result.State);
                return;
            }
            if(!socket.Socket.Recv(out var message))
            {
                ZeroTrace.WriteInfo("PlanTest", "Recv", socket.Socket.LastError);
                return;
            }

            var value = message.Unpack();
            ZeroTrace.WriteInfo("PlanTest", value);
        }
    }
}
