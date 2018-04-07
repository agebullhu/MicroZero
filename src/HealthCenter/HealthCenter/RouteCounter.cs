using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由计数器
    /// </summary>
    internal class PerformanceCounter : SubStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        public PerformanceCounter()
        {
            StationName = "HealthCenter";
            Subscribe = "PerformanceCounter";
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
                if (args.SubTitle == "Save")
                {
                    Save();
                    return;
                }
                try
                {
                    var data = JsonConvert.DeserializeObject<RouteData>(args.Content);
                    End(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
        /// <summary>
        /// 记录
        /// </summary>
        /// <param name="arg"></param>
        public static void Record(string arg)
        {
        }
        /// <summary>
        /// 计数单元
        /// </summary>
        public static long Unit = DateTime.Today.Year * 1000000 + DateTime.Today.Month * 10000 + DateTime.Today.Day * 100 + DateTime.Now.Hour;

        /// <summary>
        /// 计数根
        /// </summary>
        public static CountItem Station { get; set; } = new CountItem();
        
        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        public static void End(RouteData data)
        {
            try
            {
                var tm = (data.End - data.Start).TotalMilliseconds;
                if (tm > 200)
                    LogRecorder.Warning($"{data.HostName}/{data.ApiName}:执行时间异常({tm:F2}ms):");

                if (tm > AppConfig.Config.SystemConfig.WaringTime)
                    RuntimeWaring.Waring(data.HostName, data.ApiName, $"执行时间异常({tm:F0}ms)");

                long unit = DateTime.Today.Year * 1000000 + DateTime.Today.Month * 10000 + DateTime.Today.Day * 100 + DateTime.Now.Hour ;
                if (unit != Unit)
                {
                    Unit = unit;
                    Save();
                    Station = new CountItem();
                }

                Station.SetValue(tm, data);
                if (string.IsNullOrWhiteSpace(data.HostName))
                    return;
                CountItem host;
                lock (Station)
                {
                    if (!Station.Items.TryGetValue(data.HostName, out host))
                        Station.Items.Add(data.HostName, host = new CountItem());
                }
                host.SetValue(tm,data);

                if (string.IsNullOrWhiteSpace(data.ApiName))
                    return;
                CountItem api;
                lock (host)
                {
                    if (!host.Items.TryGetValue(data.ApiName, out api))
                        host.Items.Add(data.ApiName, api = new CountItem());
                }
                api.SetValue(tm, data);
            }
            catch (Exception e)
            {
                LogRecorder.Exception( e);
            }
        }


        /// <summary>
        /// 保存为性能日志
        /// </summary>
        public static void Save()
        {
            try
            {
                File.AppendAllText(Path.Combine(TxtRecorder.LogPath, $"{Unit}.count.log"), JsonConvert.SerializeObject(Station, Formatting.Indented));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }

    /// <summary>
    /// 路由计数器节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    internal class CountItem
    {
        /// <summary>
        /// 设置计数值
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="data"></param>
        public void SetValue(double tm, RouteData data)
        {
            LastTime = tm;
            Count += 1;

            if (data.Status == RouteStatus.DenyAccess)
            {
                Deny += 1;
            }
            else if (data.Status >= RouteStatus.LocalError)
            {
                Error += 1;
            }
            else
            {
                if (tm > AppConfig.Config.SystemConfig.WaringTime)
                {
                    TimeOut += 1;
                }
                TotalTime += tm;
                AvgTime = TotalTime / Count;
                if (MaxTime < tm)
                    MaxTime = tm;
                if (MinTime > tm)
                    MinTime = tm;
            }
        }

        /// <summary>
        /// 最长时间
        /// </summary>
        [DataMember, JsonProperty]
        public double MaxTime { get; set; } = double.MinValue;

        /// <summary>
        /// 最短时间
        /// </summary>
        [DataMember, JsonProperty]
        public double MinTime { get; set; } = Double.MaxValue;
        /// <summary>
        /// 总时间
        /// </summary>
        [DataMember, JsonProperty]
        public double TotalTime { get; set; }
        /// <summary>
        /// 平均时间
        /// </summary>
        [DataMember, JsonProperty]
        public double AvgTime { get; set; }

        /// <summary>
        /// 最后时间
        /// </summary>
        [DataMember, JsonProperty]
        public double LastTime { get; set; }
        
        /// <summary>
        /// 总次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Count { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        [DataMember, JsonProperty]
        public int TimeOut { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Error { get; set; }

        /// <summary>
        /// 拒绝次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Deny { get; set; }
        
        /// <summary>
        /// 子级
        /// </summary>
        [DataMember, JsonProperty]
        public Dictionary<string, CountItem> Items { get; set; } = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase);
    }
}