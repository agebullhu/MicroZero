using System;
using System.Collections.Generic;
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
    public class ApiCounterSubscribe : SubStation
    {
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public ApiCounterSubscribe()
        {
            StationName = "HealthCenter";
            Subscribe = "ApiCounter";
            IsRealModel = true;
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
                Handle(JsonConvert.DeserializeObject<CountData>(args.Content));
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError("ApiCounter", args.Content);
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
            Label = "Zero Net",
            Children = new List<CountItem>(),
            Items = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase)
        });
        /// <summary>
        /// 未发送计数
        /// </summary>
        private int _noCount = 0;
        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        private void Handle(CountData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.HostName))
                return;
            try
            {
                var tm = (data.End - data.Start).TotalMilliseconds;
                var now = DateTime.Now;
                long unit = now.Year * 1000000 + now.Month * 10000 + now.Day * 100 + now.Hour;
                if (unit != Unit)
                {
                    Unit = unit;
                    Save();
                    Root.Children.Clear();
                    Root.Items.Clear();
                }
                Root.SetValue(tm, data);

                if (!Root.Items.TryGetValue(data.Machine, out var station))
                {
                    Root.Items.Add(data.Machine, station = new CountItem
                    {
                        Id = "/" + data.Machine,
                        Label = data.Machine,
                        Children = new List<CountItem>(),
                        Items = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase)
                    });
                    Root.Children.Add(station);
                }
                else
                {
                    station.SetValue(tm, data);
                }

                if (!station.Items.TryGetValue(data.HostName, out var host))
                {
                    var config = ZeroApplication.GetConfig(data.HostName);
                    station.Items.Add(data.HostName, host = new CountItem
                    {
                        Id = station.Id + "/" + data.HostName,
                        Label = config == null ? data.HostName + "(*)" : config.StationName,
                        Children = new List<CountItem>(),
                        Items = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase)
                    });
                    station.Children.Add(host);
                }
                else
                {
                    host.SetValue(tm, data);
                }


                if (!string.IsNullOrWhiteSpace(data.ApiName))
                {
                    if (!host.Items.TryGetValue(data.ApiName, out var api))
                    {
                        host.Items.Add(data.ApiName, api = new CountItem
                        {
                            Id = host.Id + "/" + data.ApiName,
                            Label = data.ApiName,
                            Children = new List<CountItem>(),
                            Items = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase)
                        });
                        host.Children.Add(api);
                    }
                    else
                    {
                        api.SetValue(tm, data);
                    }
                }

                if (!Items.IsEmpty && ++_noCount <= 299)
                    return;
                _noCount = 0;
                WebSocketPooler.Instance?.Publish("Health", JsonConvert.SerializeObject(Root));
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }


        /// <summary>
        /// 保存为性能日志
        /// </summary>
        public static void Save()
        {
            try
            {
                File.AppendAllText(Path.Combine(TxtRecorder.LogPath, $"{Unit}.count.log"), JsonConvert.SerializeObject(Root, Formatting.Indented));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}