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
            //Datas = new List<RouteData>();

        }

        #region ZeroNet


        /// <summary>
        /// 初始化
        /// </summary>
        public static void ZeroFlush()
        {
            if (!AppConfig.Config.SystemConfig.FireZero)
                return;

            StationProgram.StationEvent -= StationProgram_StationEvent;

            foreach (var host in AppConfig.Config.RouteMap.Where(p => p.Value.ByZero).ToArray())
                AppConfig.Config.RouteMap.Remove(host.Key);

            foreach (var station in StationProgram.Configs.Values.Where(p => p.StationType == 2))
            {
                var host = new HostConfig
                {
                    ByZero = true
                };
                if (!AppConfig.Config.RouteMap.ContainsKey(station.StationName))
                    AppConfig.Config.RouteMap.Add(station.StationName, host);
                foreach (var name in station.StationAlias)
                    if (!AppConfig.Config.RouteMap.ContainsKey(name))
                        AppConfig.Config.RouteMap.Add(name, host);
            }
            StationProgram.StationEvent += StationProgram_StationEvent;
        }
        private static void StationProgram_StationEvent(object sender, StationProgram.StationEventArgument e)
        {
            switch (e.EventName)
            {
                case "system_start":
                    break;
                case "system_stop":
                    foreach (var host in AppConfig.Config.RouteMap.Where(p => p.Value.ByZero).ToArray())
                        AppConfig.Config.RouteMap.Remove(host.Key);
                    break;
                case "worker_heat":
                case "station_resume":
                case "station_install":
                case "station_join":
                    var route = new HostConfig
                    {
                        ByZero = true
                    };
                    if (!AppConfig.Config.RouteMap.ContainsKey(e.EventConfig.StationName))
                        AppConfig.Config.RouteMap.Add(e.EventConfig.StationName, route);
                    foreach (var name in e.EventConfig.StationAlias)
                        if (!AppConfig.Config.RouteMap.ContainsKey(name))
                            AppConfig.Config.RouteMap.Add(name, route);
                    break;
                case "station_left":
                    break;
                case "station_pause":
                case "station_closing":
                    AppConfig.Config.RouteMap.Remove(e.EventConfig.StationName);
                    foreach (var name in e.EventConfig.StationAlias)
                        AppConfig.Config.RouteMap.Remove(name);
                    break;
            }
        }

        #endregion
    }
}