using System;
using System.Collections.Generic;
using System.IO;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;
using WebMonitor;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由计数器
    /// </summary>
    public class HealthSubscribe : SubStation
    {
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public HealthSubscribe()
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
                try
                {
                    End(JsonConvert.DeserializeObject<RouteData>(args.Content));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    StationConsole.WriteError($"{e.Message}\r\n{args.Content}");
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
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
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        public static void End(RouteData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.HostName))
                return;
            try
            {
                var tm = (data.End - data.Start).TotalMilliseconds;
                long unit = DateTime.Today.Year * 1000000 + DateTime.Today.Month * 10000 + DateTime.Today.Day * 100 + DateTime.Now.Hour;
                if (unit != Unit)
                {
                    Unit = unit;
                    Save();
                    Root.Children.Clear();
                    Root.Items.Clear();
                }

                CountItem host;
                lock (Root)
                {
                    Root.SetValue(tm, data);
                    if (!Root.Items.TryGetValue(data.HostName, out host))
                    {
                        Root.Items.Add(data.HostName, host = new CountItem
                        {
                            Id = data.HostName,
                            Label = data.HostName,
                            Children = new List<CountItem>(),
                            Items = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase)
                        });
                        Root.Children.Add(host);
                    }
                }
                lock (host)
                {
                    host.SetValue(tm, data);
                    if (!string.IsNullOrWhiteSpace(data.ApiName))
                    {
                        if (!host.Items.TryGetValue(data.ApiName, out var api))
                        {
                            host.Items.Add(data.ApiName, api = new CountItem
                            {
                                Id = data.ApiName,
                                Label = data.ApiName,
                                Children = new List<CountItem>(),
                                Items = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase)
                            });
                            host.Children.Add(api);
                        }
                        api.SetValue(tm, data);
                    }
                }
                WebSocketPooler.Instance.Publish("Health", JsonConvert.SerializeObject(Root));
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