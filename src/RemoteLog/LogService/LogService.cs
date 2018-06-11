using System;
using System.Collections.Generic;
using System.IO;
using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.LogService
{
    /// <inheritdoc />
    /// <summary>
    /// 日志服务
    /// </summary>
    public class RemoteLogStation : SubStation
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static RemoteLogStation Station { get; set; }

        /// <summary>
        /// 日志地址
        /// </summary>
        private readonly string _logPath;

        /// <summary>
        /// 构造
        /// </summary>
        public RemoteLogStation()
        {
            StationName = "RemoteLog";
            Subscribe = "";
            Station = this;
            var sec = ConfigurationManager.Get("LogRecorder");
            _logPath = sec["txtPath"];

            try
            {
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
            }
            catch
            {
                sec["txtPath"] = _logPath = IOHelper.CheckPath(ConfigurationManager.Root.GetValue("contentRoot", Environment.CurrentDirectory), "Logs");
            }

            ZeroTrace.WriteInfo("RemoteLog", "LogPath", _logPath);
        }

        /// <summary>
        /// 记录的总数
        /// </summary>
        public static ulong RecorderCount { get; set; }

        /// <summary>
        /// 当前记录的时间点
        /// </summary>
        private long _recordTimePoint;

        /// <summary>
        /// 所有写入的文件句柄
        /// </summary>
        private readonly Dictionary<string, StreamWriter> _writers =
            new Dictionary<string, StreamWriter>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        /// <summary>
        /// 任务结束,环境销毁
        /// </summary>
        protected sealed override void OnRunStop()
        {
            foreach (var writer in _writers.Values)
            {
                writer.Flush();
                writer.Dispose();
            }
            _writers.Clear();
            base.OnRunStop();
        }

        /// <inheritdoc />
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PublishItem args)
        {
            try
            {
                CheckTimePoint();
                List<RecordInfo> items = JsonConvert.DeserializeObject<List<RecordInfo>>(args.Content);
                foreach (var info in items)
                {
                    string sub = info.Type.ToString();
                    StreamWriter writer = GetWriter(sub);
                    writer.WriteLine($@"
Date:{DateTime.Now}({info.Time})
Machine:{info.Machine}
ThreadId:{info.ThreadID}
User:{info.User}
RequestId:{info.RequestID}
Index:{info.Index}
Type:{info.TypeName},
{info.Message}
");
                    Console.WriteLine(info.Message);
                    RecorderCount++;
                    if (RecorderCount == ulong.MaxValue)
                        RecorderCount = 0;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }

        private void CheckTimePoint()
        {
            var now = DateTime.Today.Year * 1000000 + DateTime.Today.Month * 10000 + DateTime.Today.Day * 100 +
                                  (DateTime.Now.Hour / 2);
            if (now == _recordTimePoint) return;
            foreach (var w in _writers.Values)
            {
                w.Flush();
                w.Dispose();
            }
            _writers.Clear();
            _recordTimePoint = now;
        }

        private StreamWriter GetWriter(string sub)
        {
            if (_writers.TryGetValue(sub, out var writer))
                return writer;
            string ph = Path.Combine(_logPath, $"{_recordTimePoint}.{sub}.log");
            var file = new FileStream(ph, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

            writer = new StreamWriter(file) { AutoFlush = true };

            _writers.Add(sub, writer);

            return writer;
        }
    }
}