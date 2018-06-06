using System.Linq;
using System.Text;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 内部命令
    /// </summary>
    public class RouteCommand
    {
        /// <summary>
        /// 内部命令处理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool InnerCommand(string url, HttpResponse response)
        {

            //命令
            switch (url)
            {
                case "/":
                    response.WriteAsync("Wecome ZeroNet Http Router!", Encoding.UTF8);
                    return true;
                case "/_1_clear_1_":
                    Flush();
                    response.WriteAsync(JsonConvert.SerializeObject(AppConfig.Config, Formatting.Indented), Encoding.UTF8);
                    return true;
                //case "/_1_counter_1_/info":
                //    response.WriteAsync(JsonConvert.SerializeObject(RouteCounter.Station, Formatting.Indented), Encoding.UTF8);
                //    return true;
                //case "/_1_counter_1_/save":
                //    RouteCounter.Save();
                //    response.WriteAsync(JsonConvert.SerializeObject(RouteCounter.Station, Formatting.Indented), Encoding.UTF8);
                //    return true;
                case "/_1_config_1_":
                    response.WriteAsync(JsonConvert.SerializeObject(AppConfig.Config, Formatting.Indented), Encoding.UTF8);
                    return true;
                    //case "/_1_warings_1_":
                    //    response.WriteAsync(JsonConvert.SerializeObject(RuntimeWaring.WaringsTime, Formatting.Indented), Encoding.UTF8);
                    //    return true;
                    //case "/_1_cache_1_":
                    //    response.WriteAsync(JsonConvert.SerializeObject(RouteChahe.Cache, Formatting.Indented), Encoding.UTF8);
                    //    return true;
            }

            return false;
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public static void Flush()
        {
            AppConfig.Initialize();
            RouteChahe.Flush();
            RefreshStationConfig();
        }

        #region OnZeroNetEvent

        private static void OnZeroNetEvent(object sender, SystemMonitor.StationEventArgument e)
        {
            switch (e.EventName)
            {
                case "program_run":
                    BindZeroStation();
                    break;
                case "system_start":
                    OnSystemRun();
                    break;
                case "system_stop":
                    OnSystemStop();
                    break;
                case "station_resume":
                case "station_join":
                    StationJoin(e.EventConfig);
                    break;
                case "station_uninstall":
                    StationLeft(e.EventConfig);
                    break;
                case "station_left":
                case "station_pause":
                case "station_closing":
                    StationLeft(e.EventConfig);
                    break;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void RefreshStationConfig()
        {
            if (!AppConfig.Config.SystemConfig.FireZero)
                return;
            SystemMonitor.StationEvent -= OnZeroNetEvent;
            BindZeroStation();
            SystemMonitor.StationEvent += OnZeroNetEvent;
        }

        static void BindZeroStation()
        {
            ZeroApplication.Config.Foreach(config =>
            {
                if(config.StationType == ZeroStation.StationTypeApi)
                    StationJoin(config);
            });
        }

        private static void StationJoin(StationConfig station)
        {
            ZeroTrace.WriteInfo("Api Station", $"Join:{station.StationName}");

            if (AppConfig.Config.RouteMap.TryGetValue(station.StationName, out var host))
            {
                host.Failed = false;
                host.ByZero = true;
            }
            else
            {
                lock (AppConfig.Config.RouteMap)
                    AppConfig.Config.RouteMap.Add(station.StationName, host = new HostConfig
                    {
                        ByZero = true
                    });
            }
            if (station.StationAlias == null)
                return;
            foreach (var name in station.StationAlias)
            {
                if (AppConfig.Config.RouteMap.TryGetValue(station.StationName, out var host2))
                {
                    if (host2 != host)
                    {
                        AppConfig.Config.RouteMap[name] = host;
                    }
                }
                else
                {
                    lock (AppConfig.Config.RouteMap)
                        AppConfig.Config.RouteMap.Add(station.StationName, host);
                }
            }
        }

        private static void StationLeft(StationConfig station)
        {
            ZeroTrace.WriteInfo("Api Station", $"Left:{station.StationName}");

            if (AppConfig.Config.RouteMap.TryGetValue(station.StationName, out var host))
            {
                host.Failed = true;
                host.ByZero = true;
            }
        }


        private static void OnSystemStop()
        {
            foreach (var host in AppConfig.Config.RouteMap.Where(p => p.Value.ByZero).ToArray())
            {
                ZeroTrace.WriteInfo("Api Station", $"Left:{host.Key}");
                host.Value.Failed = true;
                host.Value.ByZero = true;
            }
        }


        private static void OnSystemRun()
        {
            //foreach (var host in AppConfig.Config.RouteMap.Where(p => p.Value.ByZero).ToArray())
            //{
            //    StationConsole.WriteInfo("Api Station", $"Left:{host.Key}");
            //    host.Value.Failed = true;
            //    host.Value.ByZero = true;
            //}
        }

        #endregion
    }
}