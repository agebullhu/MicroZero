using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using WebMonitor;
using ZeroNet.Devops.ZeroTracer;
using ZeroNet.Devops.ZeroTracer.DataAccess;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// API调用跟踪器
    /// </summary>
    public class FlowTracer : SubStation<CountData, PublishItem>
    {
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public FlowTracer()
        {
            Name = "FlowTracer";
            StationName = "TraceDispatcher";
            Subscribe = "";
            IsRealModel = true;
            _timer = new System.Timers.Timer(1000) { AutoReset = true };
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }


        #region 数据处理


        private readonly Dictionary<string, FlowRoot> _flows = new Dictionary<string, FlowRoot>();

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PublishItem args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(args.RequestId))
                    return;
                if (string.IsNullOrWhiteSpace(args.CallId))
                    args.CallId = "0";
                Console.WriteLine($"{args.RequestId}({args.CallId}>{args.GlobalId}) : {args.Tag}/{args.State} : {args.Station}/{args.CommandOrSubTitle}");
                if (!_flows.TryGetValue(args.RequestId, out var root))
                {
                    _flows.Add(args.RequestId, root = new FlowRoot
                    {
                        RequestId = args.RequestId,
                        Start = new FlowStep
                        {
                            Item = args,
                            GlobalId = args.GlobalId,
                            CallId = args.CallId,
                            Station = args.Requester,
                            Command = args.CommandOrSubTitle,
                            Tag = args.Tag,
                            State = (byte)args.State
                        }
                    });
                    Handle(args, root);
                }
                else
                {
                    Handle(args, root);
                    SendToWebSocket(root);
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("FlowTracer", e, args.Content);
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        void Handle(PublishItem args, FlowRoot root)
        {
            if (root.Items.TryGetValue(args.GlobalId, out var step))
            {
                // 1 ? 之前找不到父级时已被加入
                FlowStep par;
                if (args.CallId == "0")
                {
                    par = root.Start;
                }
                else if (!root.Items.TryGetValue(args.CallId, out par))
                {
                    root.Items.Add(args.CallId, par = new FlowStep
                    {
                        GlobalId = args.CallId
                    }); //2 ? 上级置疑：后续引发1?的判断
                    par.Child.Add(step);
                    step.Parent = par;
                }
                if (step == root.Start && step.Station != args.Requester)
                {
                    par.Child.Add(step = new FlowStep
                    {
                        Item = args,
                        GlobalId = args.GlobalId,
                        CallId = args.CallId,
                        Station = args.Requester,
                        Command = args.CommandOrSubTitle,
                        Tag = args.Tag,
                        State = (byte)args.State,
                        Parent = par
                    });
                    par.Child.Add(step);
                    root.Items[args.GlobalId] = step;
                }
                else
                {
                    step.Item = args;
                    step.CallId = args.CallId;
                    step.Station = args.Station;
                    if (string.IsNullOrWhiteSpace(step.Command))
                        step.Command = args.CommandOrSubTitle;
                    if (step.Tag < args.Tag)
                        step.Tag = args.Tag;
                    step.State = (byte)args.State;
                }
            }
            else
            {
                root.Items.Add(args.GlobalId, step = new FlowStep
                {
                    Item = args,
                    GlobalId = args.GlobalId,
                    CallId = args.CallId,
                    Station = args.Station,
                    Command = args.CommandOrSubTitle,
                    Tag = args.Tag,
                    State = (byte)args.State
                });
                FlowStep par;
                if (args.CallId == "0")
                {
                    par = root.Start;
                }
                else if (!root.Items.TryGetValue(args.CallId, out par))
                {
                    root.Items.Add(args.CallId, par = new FlowStep
                    {
                        GlobalId = args.CallId
                    }); //2 ? 上级置疑：后续引发1?的判断
                }
                step.Parent = par;
                par.Child.Add(step);
            }
            step.States.Add(new FlowPoint
            {
                Item = args,
                Tag = args.Tag,
                State = (byte)args.State
            });
        }

        #endregion

        #region 限流发送

        private readonly Dictionary<string, FlowRoot> waiting = new Dictionary<string, FlowRoot>();

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<FlowRoot> datas;
            lock (waiting)
            {
                if (waiting.Count == 0)
                    return;
                datas = waiting.Values.ToList();
            }

            int cnt = 0;
            var access = new FlowLogDataAccess();
            foreach (var root in datas)
            {
                if (++cnt == 100)
                {
                    Thread.Sleep(10);
                    cnt = 0;
                }
                var json = JsonConvert.SerializeObject(root);
                WebSocketNotify.Publish("trace_flow", root.RequestId, json);
                if (access.Any(p => p.RequestId == root.RequestId))
                    access.SetValue(p => p.FlowJson, json, p => p.RequestId == root.RequestId);
                else
                    access.Insert(new FlowLogData
                    {
                        RequestId = root.RequestId,
                        RootStation = root.Start.Station,
                        RootCommand = root.Start.Command,
                        RecordDate = DateTime.Now,
                        FlowJson = json
                    });
            }
        }

        protected override void DoDispose()
        {
            _timer.Close();
            _timer.Dispose();
            base.DoDispose();
        }
        private readonly System.Timers.Timer _timer;

        void SendToWebSocket(FlowRoot root)
        {
            lock (waiting)
            {
                if (!waiting.ContainsKey(root.RequestId))
                    waiting.Add(root.RequestId, root);
            }
        }

        #endregion

        #region 查询

        public static ApiResult Query(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return ApiValueResult.Succees("查询条件为空");
            var access = new FlowLogDataAccess();
            var datas = access.PageData(0, 300, p => p.Id, true, p => p.RequestId.Contains(key));
            if (datas.Count == 0)
                return ApiValueResult.Succees("未查找到历史数据");
            Task.Factory.StartNew(() =>
            {
                foreach (var root in datas)
                {
                    WebSocketNotify.Publish("trace_flow", "~" + root.RequestId, root.FlowJson);
                }
            });
            return ApiValueResult.Succees($"查找到{datas.Count}条数据");
        }

        #endregion
    }
}