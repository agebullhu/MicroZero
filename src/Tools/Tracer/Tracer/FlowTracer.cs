using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;
using Agebull.EntityModel.Common;
using WebMonitor;
using MicroZero.Devops.ZeroTracer;
using MicroZero.Devops.ZeroTracer.DataAccess;
using WebMonitor.Models;
using Timer = System.Timers.Timer;
namespace MicroZero.Http.Route
{
    /// <summary>
    /// API调用跟踪器
    /// </summary>
    public class FlowTracer : SubStation<CountData, PublishItem>
    {
        private readonly bool _saveTrace;
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public FlowTracer()
        {
            StationName = "TraceDispatcher";
            Name = "FlowTracer";
            IsRealModel = true;
            _timer = new Timer(1000)
            {
                AutoReset = true
            };
            _saveTrace = Agebull.Common.Configuration.ConfigurationManager.AppSettings.GetBool("saveTrace");
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }


        #region 数据处理

        /// <summary>
        /// 流程
        /// </summary>
        private readonly Dictionary<string, FlowRoot> _flows = new Dictionary<string, FlowRoot>();

        /// <summary>
        /// 单位时间内的请求ID，用于过期删除
        /// </summary>
        private readonly SortedDictionary<long, List<string>> _timeRequestIds = new SortedDictionary<long, List<string>>();

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
                    return ;
                if (string.IsNullOrWhiteSpace(args.CallId))
                    args.CallId = "0";
                string title = args.CommandOrSubTitle;
                GlobalValue.Documents.TryGetValue(args.Requester, out var doc);
                if (doc != null && doc.Aips.TryGetValue(title, out var api))
                    title = api.Caption;
                //Console.WriteLine($"{args.RequestId}({args.CallId}>{args.GlobalId}) : {args.Tag}/{args.State} : {args.Station}/{args.CommandOrSubTitle}");
                if (_flows.TryGetValue(args.RequestId, out var root))
                {
                    Handle(args, root);
                    SendToWebSocket(root);
                }
                else
                {
                    _flows.Add(args.RequestId, root = new FlowRoot
                    {
                        RequestId = args.RequestId,
                        First = new FlowStep
                        {
                            Item = args,
                            GlobalId = args.GlobalId,
                            StationName = doc?.Caption ?? args.Requester,
                            Title = title,
                            CallId = args.CallId,
                            Station = args.Requester,
                            Command = args.CommandOrSubTitle,
                            Tag = args.Tag,
                            State = args.ZeroState
                        }
                    });
                    Handle(args, root);
                    var tk = DateTime.Now.Ticks;
                    if (_timeRequestIds.TryGetValue(tk, out var ids))
                        ids.Add(args.RequestId);
                    else _timeRequestIds.Add(tk, new List<string> { args.RequestId });
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
                    par = root.First;
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
                if (step == root.First && step.Station != args.Requester)
                {
                    par.Child.Add(step = new FlowStep
                    {
                        Item = args,
                        GlobalId = args.GlobalId,
                        CallId = args.CallId,
                        Station = args.Requester,
                        Command = args.CommandOrSubTitle,
                        Tag = args.Tag,
                        State = args.ZeroState,
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
                    step.State = args.ZeroState;
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
                    State = args.ZeroState
                });
                FlowStep par;
                if (args.CallId == "0")
                {
                    par = root.First;
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
                State = args.ZeroState
            });
        }

        #endregion

        #region 限流发送

        void SendToWebSocket(FlowRoot root)
        {
            lock (_waiting)
            {
                if (!_waiting.ContainsKey(root.RequestId))
                    _waiting.Add(root.RequestId, root);
            }
        }

        private readonly Timer _timer;

        private readonly Dictionary<string, FlowRoot> _waiting = new Dictionary<string, FlowRoot>();

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var min = DateTime.Now.AddMinutes(-5).Ticks;
            foreach (var k in _timeRequestIds.Keys.Where(p=>p <= min).ToArray())
            {
                foreach (var id in _timeRequestIds[k])
                {
                    _flows.Remove(id);
                }
                _timeRequestIds.Remove(k);
            }

            List<FlowRoot> datas;
            lock (_waiting)
            {
                if (_waiting.Count == 0)
                    return;
                datas = _waiting.Values.ToList();
                _waiting.Clear();
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
                var json = JsonHelper.SerializeObject(root);
                WebSocketNotify.Publish("trace_flow", root.RequestId, json);
                if (!_saveTrace)
                    continue;
                if (access.Any(p => p.RequestId == root.RequestId))
                    access.SetValue(p => p.FlowJson, json, p => p.RequestId == root.RequestId);
                else
                    access.Insert(new FlowLogData
                    {
                        RequestId = root.RequestId,
                        RootStation = root.First.Station,
                        RootCommand = root.First.Command,
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
        #endregion

        #region 查询

        public static ApiResult Query(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return ApiResult.Succees("查询条件为空");
            Task.Factory.StartNew(() =>
            {
                var access = new FlowLogDataAccess();
                var datas = access.PageData(0, 300, p => p.Id, true, p => p.RequestId.Contains(key));
                foreach (var root in datas)
                {
                    WebSocketNotify.Publish("trace_flow", "~" + key, root.FlowJson);
                }
            }, TaskCreationOptions.LongRunning);
            return new ApiResult
            {
                Success = true,
                Status = new OperatorStatus
                {
                    ClientMessage = "正在查询中...您将陆续收到查询结果"
                }
            };
        }

        #endregion
    }
}