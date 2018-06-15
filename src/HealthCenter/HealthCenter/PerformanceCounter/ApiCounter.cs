using System;
using System.IO;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由计数器
    /// </summary>
    internal class ApiCounter : SubStation
    {
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public ApiCounter()
        {
            Name = "ApiCounter";
            StationName = "HealthCenter";
            Subscribe = "";//ApiCounter
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
                    var data = JsonConvert.DeserializeObject<CountData>(args.Content);
                    End(data);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("HealthCenter",e, args.Content);
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
        void End(CountData data)
        {
            try
            {
                var tm = (data.End - data.Start).TotalMilliseconds;
                if (tm > 200)
                    RuntimeWaring.Instance.Waring(data.HostName, data.ApiName, $"执行时间异常({tm:F0}ms)");

                long unit = DateTime.Today.Year * 1000000 + DateTime.Today.Month * 10000 + DateTime.Today.Day * 100 + DateTime.Now.Hour;
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
                host.SetValue(tm, data);

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
                File.AppendAllText(Path.Combine(TxtRecorder.LogPath, $"{Unit}.count.log"), JsonConvert.SerializeObject(Station, Formatting.Indented));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}