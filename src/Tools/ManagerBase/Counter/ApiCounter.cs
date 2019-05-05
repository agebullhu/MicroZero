using System;
using System.IO;
using Agebull.Common.Logging;
using Agebull.Common.Tson;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;
using Newtonsoft.Json;
using WebMonitor;

namespace MicroZero.Http.Route
{
    /// <summary>
    /// 路由计数器
    /// </summary>
    public class ApiCounter : SubStation<CountData, PublishItem>
    {
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public ApiCounter()
        {
            Name = "ApiCounter";
            StationName = "HealthCenter";
            Subscribes.Add("ApiCounter");
            IsRealModel = true;
            TsonOperator = new CountDataTsonOperator();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void OnInitialize()
        {
            Load();
        }
        protected override void DoDestory()
        {
            Save();
            base.DoDestory();
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        protected override void OnLoopIdle()
        {
            //DoPublish();
        }
        void DoPublish()
        {
            if ((DateTime.Now - preSend).TotalSeconds < 1 || Root.UnitCount == 0)
                return;

            Root.End();
            var json = JsonHelper.SerializeObject(Root);
            Root.Start();
            preSend = DateTime.Now;
            WebSocketNotify.Publish("api", "root", json);
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
                var datas = DeserializeList(args);
                if (datas == null)
                    return;
                foreach (var data in datas)
                    Handle(data);
                DoPublish();
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
        /// 上次发送时间
        /// </summary>
        private DateTime preSend;
        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        private void Handle(CountData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.HostName) || data.End == 0)
                return;
            try
            {
                var tm = (data.End - data.Start) / 10;
                Root.SetValue(tm, data);

                if (!Root.Items.TryGetValue(data.Machine, out var machine))
                {
                    Root.Items.Add(data.Machine, machine = new CountItem
                    {
                        Id = data.Machine,
                        Label = data.Machine
                    });
                    Root.Children.Add(machine);
                }
                else
                {
                    machine.SetValue(tm, data);
                }

                var stationName = data.Station ?? data.HostName;
                if (!machine.Items.TryGetValue(stationName, out var station))
                {
                    machine.Items.Add(stationName, station = new CountItem
                    {
                        Id = $"/{data.Machine}/{stationName}",
                        Label = stationName
                    });
                    machine.Children.Add(station);
                }
                else
                {
                    station.SetValue(tm, data);
                }

                var tn = data.IsInner ? "api" : "call";
                if (!station.Items.TryGetValue(tn, out var tag))
                {
                    station.Items.Add(tn, tag = new CountItem
                    {
                        Id = $"{station.Id}/{tn}",
                        Label = tn
                    });
                    station.Children.Add(tag);
                }
                else
                {
                    tag.SetValue(tm, data);
                }

                if (!tag.Items.TryGetValue(data.HostName, out var host))
                {
                    var config = ZeroApplication.Config[data.HostName];

                    tag.Items.Add(data.HostName, host = new CountItem
                    {
                        Id = $"{tag.Id}/{data.HostName}",
                        Label = $"{data.HostName}{(config == null ? "?" : "")}"
                    });
                    tag.Children.Add(host);
                }
                else
                {
                    host.SetValue(tm, data);
                }
                if (!host.Items.TryGetValue(data.ApiName, out var api))
                {
                    host.Items.Add(data.ApiName, api = new CountItem
                    {
                        Id = $"{host.Id}/{data.ApiName}",
                        Label = data.ApiName
                    });
                    host.Children.Add(api);
                }
                else
                {
                    api.SetValue(tm, data);
                }
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
            }
        }


        /// <summary>
        /// 保存为性能日志
        /// </summary>
        void Save()
        {
            try
            {
                File.WriteAllText(Path.Combine(ZeroApplication.Config.DataFolder, "ApiCount.json"), JsonConvert.SerializeObject(Root, Formatting.Indented));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// 保存为性能日志
        /// </summary>
        void Load()
        {
            var file = Path.Combine(ZeroApplication.Config.DataFolder, "ApiCount.json");
            if (!File.Exists(file))
                return;
            try
            {
                var json = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(json))
                    return;
                _root = JsonConvert.DeserializeObject<CountItem>(json) ?? new CountItem();
                RebuildItems(_root);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        void RebuildItems(CountItem item)
        {
            if (item.Items == null)
                item.Items = new System.Collections.Generic.Dictionary<string, CountItem>();
            if (item.Children == null)
                item.Children = new System.Collections.Generic.List<CountItem>();
            else
                foreach (var child in item.Children)
                {
                    RebuildItems(child);
                    item.Items.Add(item.Id, item);
                }
        }
    }
}