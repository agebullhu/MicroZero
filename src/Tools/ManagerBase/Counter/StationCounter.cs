using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;
using WebMonitor;
using WebMonitor.Models;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     站点计数器
    /// </summary>
    public class StationCounter
    {
        private static readonly Dictionary<string, KLine> minuteLine = new Dictionary<string, KLine>();
        public static Dictionary<string, List<KLine>> KLines = new Dictionary<string, List<KLine>>();
        public static readonly Dictionary<string, StationCountItem> StationCountItems = new Dictionary<string, StationCountItem>();

        private static DateTime BaseLine = DateTime.Now;

        public static void Start()
        {
            var file = Path.Combine(ZeroApplication.Config.DataFolder, "kline_station.json");
            if (File.Exists(file))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var dir = JsonConvert.DeserializeObject<Dictionary<string, List<KLine>>>(json);
                    if (dir != null)
                        KLines = dir;
                    var now = DateTime.Now.AddMinutes(-1);//上一分钟
                    BaseLine = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                    foreach (var items in KLines.Values.ToArray())
                    {
                        var array = items.ToArray();
                        var line = GetTime(BaseLine);
                        int cnt = 0;
                        for (int idx = array.Length - 1; idx >= 0; idx--)
                        {
                            if (array[idx] == null)
                                continue;
                            ++cnt;
                            var sp = line - array[idx].Time;
                            if (sp < 0)
                                continue;
                            if (sp == 0)
                            {
                                line -= 60000;//一分钟
                                continue;
                            }
                            while (line > array[idx].Time && cnt <= 240)
                            {
                                if (idx == items.Count - 1)
                                {
                                    items.Add(new KLine
                                    {
                                        Time = line
                                    });
                                }
                                else
                                {
                                    items.Insert(idx + 1, new KLine
                                    {
                                        Time = line
                                    });
                                }
                                line -= 60000;
                                ++cnt;
                            }
                            if(cnt > 240)
                            {
                                if (items.Count > 240)
                                    items.RemoveRange(0, items.Count - 240);
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteError("Restore Station KLine Data", e, file);
                }
            }
            ZeroApplication.ZeroNetEvent += new StationCounter().SystemMonitor_StationEvent;
        }

        private void SystemMonitor_StationEvent(object sender, ZeroNetEventArgument e)
        {
            Task.Factory.StartNew(() => StationEvent(e));
        }
        private void PublishConfig(StationConfig config)
        {
            if (config == null)
                return;
            var info = new StationInfo(config);
            if (StationCountItems.TryGetValue(config.Name, out var status))
                info.Status = status;
            var json = JsonConvert.SerializeObject(info);

            WebSocketNotify.Publish("config", config.StationName, json);
        }
        private void StationEvent(ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.CenterStationJoin:
                case ZeroNetEventType.CenterStationLeft:
                case ZeroNetEventType.CenterStationPause:
                case ZeroNetEventType.CenterStationResume:
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationInstall:
                case ZeroNetEventType.CenterStationUpdate:
                case ZeroNetEventType.CenterStationRemove:
                    PublishConfig(e.EventConfig);
                    return;
                case ZeroNetEventType.AppStop:
                case ZeroNetEventType.AppRun:
                    NewBaseTime();
                    ZeroApplication.Config.Foreach(cfg =>
                    {
                        PublishConfig(cfg);
                        if (!KLines.ContainsKey(cfg.StationName))
                            KLines.Add(cfg.StationName, new List<KLine>());
                        var nLine = new KLine
                        {
                            Time = GetTime(BaseLine)
                        };
                        if (minuteLine.ContainsKey(cfg.StationName))
                            minuteLine[cfg.StationName] = nLine;
                        else
                            minuteLine.Add(cfg.StationName, nLine);
                    });
                    return;
                case ZeroNetEventType.AppEnd:
                    File.WriteAllText(Path.Combine(ZeroApplication.Config.DataFolder, "kline_station.json"),
                        JsonConvert.SerializeObject(KLines));
                    return;
                case ZeroNetEventType.CenterStationState:
                    if (e.EventConfig == null || e.Context == null)
                        return;
                    Collect(e.EventConfig, e.Context);
                    return;
            }
        }
        /// <summary>
        /// 取1970年起的毫秒数
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        static long GetTime(DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
        void NewBaseTime()
        {
            var now = DateTime.Now;
            BaseLine = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <returns></returns>
        private void Collect(StationConfig station, string json)
        {
            var scope = OnceScope.TryCreateScope(minuteLine, 100);
            if (scope == null)
                return;//系统太忙，跳过处理
            using (scope)
            {
                var now = JsonConvert.DeserializeObject<StationCountItem>(json);
                if (!StationCountItems.TryGetValue(station.StationName, out var item))
                {
                    now.Count = 1;
                    StationCountItems.Add(station.StationName, now);
                    return;
                }

                item.CheckValue(station, now);

                WebSocketNotify.Publish("status", station.StationName, JsonConvert.SerializeObject(item));
                long value = station.StationType == ZeroStationType.Api ||
                             station.StationType == ZeroStationType.Vote
                    ? item.LastTps
                    : item.LastQps;
                if (minuteLine.TryGetValue(station.StationName, out var kLine))
                {
                    if (kLine.Count == 0)
                    {
                        kLine.Total = value;
                        kLine.Open = value;
                        kLine.Close = value;
                        kLine.Max = value;
                        kLine.Min = value;
                    }
                    else
                    {
                        kLine.Total += value;
                        kLine.Close = value;
                        if (kLine.Max < value)
                            kLine.Max = value;
                        if (kLine.Min > value)
                            kLine.Min = value;
                    }

                    kLine.Count++;
                }
                else
                {
                    minuteLine.Add(station.StationName, kLine = new KLine
                    {
                        Time = GetTime(BaseLine),
                        Count = 1,
                        Total = value,
                        Open = value,
                        Close = value,
                        Max = value,
                        Min = value,
                        Avg = value
                    });
                    KLines.Add(station.StationName, new List<KLine> { kLine });
                }
            }

            if ((DateTime.Now - BaseLine).TotalMinutes < 1)
                return;
            NewBaseTime();
            foreach (var key in KLines.Keys.ToArray())
            {
                if (!minuteLine.TryGetValue(key, out var line))
                {
                    minuteLine.Add(key, line = new KLine
                    {
                        Time = GetTime(BaseLine),
                        Count = 1
                    });
                }
                line.Avg = line.Count == 0 ? 0 : decimal.Round(line.Total / line.Count, 4);
                while (KLines[key].Count > 240)
                    KLines[key].RemoveAt(0);
                KLines[key].Add(line);
                WebSocketNotify.Publish("kline", key, JsonConvert.SerializeObject(line));
                var nLine = new KLine
                {
                    Time = GetTime(BaseLine)
                };
                minuteLine[key] = nLine;
            }

        }


    }

    #region 性能统计

    #endregion
}