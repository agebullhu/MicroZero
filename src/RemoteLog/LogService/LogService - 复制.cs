// 所在工程：GBoxtCommonService
// 整理用户：bull2
// 建立时间：2012-08-13 5:35
// 整理时间：2012-08-30 3:12

#region

using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using Agebull.Common.Frame;

#if SILVERLIGHT
using System.IO.IsolatedStorage;
using System.Xml.Linq;
#endif

#endregion

namespace Agebull.Common.Logging
{
    /// <summary>
    ///   文本记录器
    /// </summary>
#if SERVICE
    public sealed class TxtRecorder : TraceListener, ILogRecorder
#else
    public sealed class TxtRecorder : ILogRecorder
#endif
    {
        /// <summary>
        ///   初始化
        /// </summary>
        public static TxtRecorder Recorder = new TxtRecorder();

        /// <summary>
        ///   初始化
        /// </summary>
        public void Initialize()
        {
#if !SILVERLIGHT
            try
            {
                if (string.IsNullOrWhiteSpace(LogPath))
                {
                    var cfgpath = ConfigurationManager.AppSettings["LogPath"];
                    if (string.IsNullOrWhiteSpace(cfgpath))
                    {
#if CLIENT
                        string exeth = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
#else
                        string exeth = Path.GetDirectoryName(GetType().Assembly.Location);
#endif
                        if (exeth != null)
                        {
                            cfgpath = Path.Combine(exeth, "logs");
                        }
                    }
                    LogPath = cfgpath;
                }
                if (LogPath != null && !Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                Console.WriteLine($"LogPath:{LogPath}");
            }
            catch (Exception ex)
            {
                LogRecorder.SystemTrace("日志记录:TextRecorder.Initialize", ex);
            }
#endif
        }

        /// <summary>
        ///   停止
        /// </summary>
        public void Shutdown()
        {
        }

        /// <summary>
        ///   文本日志的路径,如果不配置,就为:[应用程序的路径]\log\
        /// </summary>
        public static string LogPath { get; set; }

        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        public void RecordLog(RecordInfo info)
        {
#if CLIENT
            RecordLog(info.gID, info.Message, info.TypeName);
#else
            switch (info.Type)
            {
                case LogType.System:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "system");
                    break;
                case LogType.Login:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "user");
                    break;
                case LogType.Request:
                case LogType.WcfMessage:
                case LogType.Trace:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "trace");
                    break;
                case LogType.DataBase:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "sql");
                    break;
                case LogType.Warning:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "warning");
                    break;
                case LogType.Error:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "error");
                    break;
                case LogType.Exception:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "exception");
                    break;
                case LogType.Plan:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "plan");
                    break;
                case LogType.Monitor:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "monitor");
                    break;
                default:
                    RecordLog(info.RequestID, info.Message, info.TypeName, info.User, "info");
                    break;
            }
#endif
        }

        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="id"> 标识 </param>
        /// <param name="msg"> 消息 </param>
        /// <param name="type"> 日志类型 </param>
        /// <param name="user"> 当前操作者 </param>
        /// <param name="name"> 标识的文件后缀(如.error,则文件名可能为 20160602.error.log) </param>
        private void RecordLog(string id, string msg, string type, string user = null, string name = null)
        {
#if SILVERLIGHT
            XElement xmlTree1 = new XElement("LogItem", new XElement("Date", DateTime.Now), new XElement("Type", type), new XElement("User", user), new XElement("Infomation", msg));
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            xmlTree1.Save(sw);
            string info = sb.ToString();
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.DirectoryExists("Log"))
                    {
                        store.CreateDirectory("Log");
                    }
                    string ph = Path.Combine("Log", DateTime.Today.ToString("yyyy-MM-dd") + ".log");
                    using (IsolatedStorageFileStream rootFile = store.OpenFile(ph, FileMode.Append))
                    {
                        StreamWriter writer = new StreamWriter(rootFile);
                        writer.Write(info.ToString());
                        writer.Flush();
                        rootFile.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                SytemDebug.WriteLine("日志记录时发生异常：\r\n{0}\r\n{1}" , info , ex) ;
            }
#else
            string log = type == "DataBase"
                ? $@"
/*Date:{ DateTime.Now.ToString(CultureInfo.InvariantCulture)}
RQID:{id}*/
{msg}"
                : $@"
RQID:{id}
Date:{DateTime.Now.ToString(CultureInfo.InvariantCulture)}
Type:{type}
User:{user}
{msg}";
            try
            {
                //if (!Directory.Exists(LogPath))
                //{
                //    Directory.CreateDirectory(LogPath);
                //}
                string ph = Path.Combine(LogPath, $"{DateTime.Today:yyyyMMdd}.{name ?? "info"}.log");

                //using (ThreadLockScope.Scope(this))
                {
                    File.AppendAllText(ph, log, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                LogRecorder.SystemTrace("日志记录:TextRecorder.RecordLog4", ex);
            }
#endif
        }

        /// <summary>
        ///   记录跟踪信息
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="type"></param>
        public static void RecordTrace(string message, string type = "trace")
        {
            try
            {
                Recorder.RecordTraceInner(message, type);
            }
            catch (Exception ex)
            {
                LogRecorder.SystemTrace("日志记录:TextRecorder.RecordLog1", ex);
            }
        }

        /// <summary>
        ///   记录日志
        /// </summary>
        private void RecordTraceInner(string msg, string type = "trace")
        {
#if SILVERLIGHT
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.DirectoryExists("Log"))
                {
                    store.CreateDirectory("Log");
                }
                string ph = Path.Combine(LogPath, DateTime.Today.ToString("yyyy-MM-dd") + ".trace.log");
                using (IsolatedStorageFileStream rootFile = store.OpenFile(ph, FileMode.Append))
                {
                    StreamWriter writer = new StreamWriter(rootFile);
                    writer.Write(msg);
                    writer.Write("\n");
                    writer.Flush();
                    rootFile.Close();
                }
            }
#else
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            string ph = Path.Combine(LogPath, $"{DateTime.Today:yyyy-MM-dd}.{DateTime.Now.Hour / 2}.{type.Trim('.')}.log");

            using (ThreadLockScope.Scope(this))
            {
                File.AppendAllText(ph, msg + "\n", Encoding.UTF8);
            }
#endif
        }
        /// <summary>
        ///   写消息--Trace
        /// </summary>
        /// <param name="message"> </param>
#if SERVICE
        public override void Write(string message)
#else
        public void Write(string message)
#endif
        {
            RecordLog(LogRecorder.GetRequestId(), message, LogRecorder.TypeToString(LogType.Trace));
        }

        /// <summary>
        ///   写一行消息--Trace
        /// </summary>
        /// <param name="message"> </param>
#if SERVICE
        public override void WriteLine(string message)
#else
        public void WriteLine(string message)
#endif
        {
            RecordLog(LogRecorder.GetRequestId(), message, LogRecorder.TypeToString(LogType.Trace));
        }
    }
}
