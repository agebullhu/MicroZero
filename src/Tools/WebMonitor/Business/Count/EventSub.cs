using System;
using System.IO;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using WebMonitor;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由计数器
    /// </summary>
    public class EventSub : SubStation<OpenDoorArg, PublishItem>
    {
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public EventSub()
        {
            Name = "MachineEventMQ";
            StationName = "MachineEventMQ";
            Subscribe = "";
            IsRealModel = true;
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        public override void Idle()
        {
            //DoPublish();
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PublishItem args)
        {
            try
            {
                var data = DeserializeObject(args);
                if (data == null)
                    return;
                Handle(data);
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("ApiCounter", e, args.Content);
            }
        }
        /// <summary>
        /// 计数单元
        /// </summary>
        public static long Unit = DateTime.Today.Year * 1000000 + DateTime.Today.Month * 10000 + DateTime.Today.Day * 100 + DateTime.Now.Hour;

        private static CountItem _root;
        /// <summary>
        /// 计数根
        /// </summary>
        public static CountItem Root => _root ?? (_root = new CountItem
        {
            Id = "root",
            Label = "Zero Net"
        });

        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        private void Handle(OpenDoorArg data)
        {
            if (data == null)
                return;
            if (data.UserId % 2 == 1)
            {
                WebSocketNotify.Publish("event", "openDoor", JsonConvert.SerializeObject(new
                {
                    name = "胡天水",
                    msg = data.InOrOut == "进" ? $"欢迎胡天水先生光临" : "祝胡天水先生一路顺风",
                    data = JsonConvert.SerializeObject (data)
                }));
            }
            else
            {
                WebSocketNotify.Publish("event", "openDoor", JsonConvert.SerializeObject(new
                {
                    name = "刘强东",
                    msg = data.InOrOut == "进" ? $"刘强东来了,美女们注意安全" : "刘强东走了,警报解除",
                    data = JsonConvert.SerializeObject(data)
                }));
            }
        }
    }
    
}