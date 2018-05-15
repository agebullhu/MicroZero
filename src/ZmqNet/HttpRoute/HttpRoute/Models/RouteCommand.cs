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
            RuntimeWaring.Flush();
            ZeroFlush();
        }

        #region ZeroNet


        /// <summary>
        /// 初始化
        /// </summary>
        public static void ZeroFlush()
        {
            if (!AppConfig.Config.SystemConfig.FireZero)
                return;

            SystemMonitor.StationEvent -= StationProgram_StationEvent;
            BindZero();
            SystemMonitor.StationEvent += StationProgram_StationEvent;
        }

        static void BindZero()
        {
            if (ZeroApplication.State != StationState.Run)
                return;
            foreach (var station in ZeroApplication.Configs.Values.Where(p => p.StationType == ZeroStation.StationTypeApi).ToArray())
            {
                ApiStationJoin(station);
            }
        }

        private static void ApiStationJoin(StationConfig station)
        {
            StationConsole.WriteInfo($"Zero Station:{station.StationName}");

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

        private static void StationProgram_StationEvent(object sender, SystemMonitor.StationEventArgument e)
        {
            switch (e.EventName)
            {
                case "program_run":
                    BindZero();
                    break;
                case "system_start":
                    {
                        foreach (var host in AppConfig.Config.RouteMap.Values.Where(p => p.ByZero).ToArray())
                            host.Failed = false;
                    }
                    break;
                case "system_stop":
                    {
                        foreach (var host in AppConfig.Config.RouteMap.Values.Where(p => p.ByZero).ToArray())
                            host.Failed = true;
                    }
                    break;
                case "worker_heat":
                case "station_resume":
                case "station_install":
                case "station_join":
                    ApiStationJoin(e.EventConfig);
                    break;
                case "station_uninstall":
                    AppConfig.Config.RouteMap.Remove(e.EventConfig.StationName);
                    break;
                case "station_left":
                case "station_pause":
                case "station_closing":
                    {
                        if (AppConfig.Config.RouteMap.TryGetValue(e.EventConfig.StationName, out var host))
                            host.Failed = true;
                    }
                    break;
            }
        }

        #endregion
    }
}