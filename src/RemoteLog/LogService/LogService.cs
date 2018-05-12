using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Frame;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.LogService
{
    /// <summary>
    /// 日志服务
    /// </summary>
    public class RemoteLogStation : SubStation
    {
        public static RemoteLogStation Station { get; set; }
        private string LogPath;
        /// <summary>
        /// 构造
        /// </summary>
        public RemoteLogStation()
        {
            StationName = "RemoteLog";
            Subscribe = "Record";
            Station = this;
            if (string.IsNullOrWhiteSpace(LogPath))
            {
                LogPath = ConfigurationManager.AppSettings["LogPath"];
            }
            if (LogPath != null && !Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            Console.WriteLine($"LogPath:{LogPath}");

        }
        /// <summary>
        /// 记录的总数
        /// </summary>
        public static ulong RecorderCount { get; set; }

        private long point;
        private Dictionary<string, StreamWriter > writers = new Dictionary<string, StreamWriter>( StringComparer.OrdinalIgnoreCase);
        protected override void OnTaskStop()
        {
            foreach (var writer in writers.Values)
            {
                writer.Flush();
                writer.Dispose();
            }

            writers.Clear();
            base.OnTaskStop();
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PublishItem args)
        {
            var now = DateTime.Today.Year * 1000000 + DateTime.Today.Month * 10000 + DateTime.Today.Day * 100 + (DateTime.Now.Hour / 2);
            if (now != point)
            {
                foreach (var w in writers.Values)
                {
                    w.Flush();
                    w.Dispose();
                }

                writers.Clear();
                point = now;
                
            }

            try
            {
                //var record = JsonConvert.DeserializeObject<RecordInfo>(args.Content);
                if (!writers.TryGetValue(args.SubTitle, out var writer))
                {
                    string ph = Path.Combine(LogPath, $"{point}.{args.SubTitle}.log");
                    var file = new FileStream(ph, FileMode.Append, FileAccess.Write, FileShare.Read);
                    
                    writer = new StreamWriter(file) {AutoFlush = true};
                    
                    writers.Add(args.SubTitle, writer);
                }
                writer.WriteLine(args.Content);
                RecorderCount++;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
    }
}
