using System;
using System.Collections.Generic;
using System.IO;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;

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
            Subscribe = "Record";
            Station = this;
            _logPath = ApiContext.Configuration["logPath"] ??
                       IOHelper.CheckPath(ApiContext.Configuration["contentRoot"], "Logs");

            try
            {
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
            }
            catch
            {
                _logPath = IOHelper.CheckPath(ApiContext.Configuration["contentRoot"], "Logs");
            }

            Console.WriteLine($"LogPath:{_logPath}");
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

        /// <summary>
        /// 任务结束,环境销毁
        /// </summary>
        protected override void OnTaskStop()
        {
            foreach (var writer in _writers.Values)
            {
                writer.Flush();
                writer.Dispose();
            }

            _writers.Clear();
            base.OnTaskStop();
        }

        /// <inheritdoc />
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PublishItem args)
        {
            var now = DateTime.Today.Year * 1000000 + DateTime.Today.Month * 10000 + DateTime.Today.Day * 100 +
                      (DateTime.Now.Hour / 2);
            if (now != _recordTimePoint)
            {
                foreach (var w in _writers.Values)
                {
                    w.Flush();
                    w.Dispose();
                }

                _writers.Clear();
                _recordTimePoint = now;
            }

            try
            {
                if (!_writers.TryGetValue(args.SubTitle, out var writer))
                {
                    string ph = Path.Combine(_logPath, $"{_recordTimePoint}.{args.SubTitle}.log");
                    var file = new FileStream(ph, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

                    writer = new StreamWriter(file) {AutoFlush = true};

                    _writers.Add(args.SubTitle, writer);
                }

                writer.WriteLine(args.Content);
                RecorderCount++;
                if (RecorderCount == ulong.MaxValue)
                    RecorderCount = 0;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
    }
}