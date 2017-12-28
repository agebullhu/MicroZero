// 所在工程：GBoxtCommonService
// 整理用户：bull2
// 建立时间：2012-08-13 5:34
// 整理时间：2012-08-30 2:58

#region

using System;
using System.Data.SqlClient;
using System.IO;

using Agebull.Common.Logging;

using log4net;
using log4net.Config;

#endregion

namespace Agebull.Common.Server.Logging
{
    /// <summary>
    ///   日志处理类
    /// </summary>
    public class Log4Recorder : ILogRecorder
    {
        /// <summary>
        ///   日志名
        /// </summary>
        private string _Logname;

        /// <summary>
        ///   默认构造
        /// </summary>
        public Log4Recorder()
        {
        }

        /// <summary>
        ///   配置文件构造
        /// </summary>
        public Log4Recorder(string cfgPath)
        {
            this.Logname = cfgPath;
        }

        /// <summary>
        ///   日志名
        /// </summary>
        public string Logname
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._Logname))
                {
                    this._Logname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "log4net.config");
                }
                return this._Logname;
            }
            private set
            {
                this._Logname = value;
            }
        }

        /// <summary>
        ///   日志处理对象
        /// </summary>
        public ILog Log { get; private set; }

        #region IRecorder Members

        /// <summary>
        ///   初始化
        /// </summary>
        public void Initialize()
        {

            XmlConfigurator.ConfigureAndWatch(new FileInfo(this.Logname)); //应用配置文件
            this.Log = LogManager.GetLogger("Agebull.Log"); //初始化日志对象
        }

        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        public void RecordLog(RecordInfo info)
        {
            if (this.Log == null)
            {
                return;
            }
            LogContent lc = new LogContent
            {
                Sort = info.TypeName,
                Infomation = info.Message,
                //User = info.User ,
                QueryKey = info.gID.ToString().ToUpper()
            };
            switch (info.Type)
            {
                case LogType.Trace:
                    this.Log.Debug(lc);
                    break;
                case LogType.Message:
                    this.Log.Info(lc);
                    break;
                case LogType.Warning:
                    this.Log.Warn(lc);
                    break;
                case LogType.Error:
                    this.Log.Error(lc);
                    break;
                case LogType.Exception:
                    this.Log.Fatal(lc);
                    break;
                default:
                    this.Log.Debug(lc);
                    break;
            }
        }

        /// <summary>
        ///   停止
        /// </summary>
        public void Shutdown()
        {
        }

        #endregion

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="lc"> 日志详细信息 </param>
        public void Record(LogContent lc)
        {
            this.Log.Info(lc);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="queryKey"> </param>
        /// <param name="message"> </param>
        /// <param name="user"> 用户 </param>
        public void Record(string queryKey, string message, string user)
        {
            this.Record(new LogContent
            {
                QueryKey = queryKey,
                Infomation = message
            });
        }

        ///<summary>
        ///  写入一般日志
        ///</summary>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public void Record(string message, params object[] formatArgs)
        {
            this.Log.InfoFormat(message, formatArgs);
        }

        ///<summary>
        ///  写入调试日志
        ///</summary>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public void Debug(string message, params object[] formatArgs)
        {
            this.Log.DebugFormat(message, formatArgs);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="lc"> 上下文 </param>
        public void Debug(LogContent lc)
        {
            this.Log.Debug(lc);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="message"> 日志详细信息 </param>
        public void Debug(string message)
        {
            LogContent lc = LogContent.Content;
            lc.Infomation = message;
            this.Log.Debug(lc);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="lc"> 上下文 </param>
        public void Exception(LogContent lc)
        {
            this.Log.Error(lc);
        }

        /// <summary>
        ///   记录异常日志
        /// </summary>
        /// <param name="message"> 日志详细信息 </param>
        /// <param name="e"> 异常 </param>
        public string Exception(Exception e, string message)
        {
            LogContent lc = LogContent.Content;
            lc.Infomation = message;
            this.Log.Error(lc, e);
            return lc.QueryKey;
        }

        /// <summary>
        ///   记录异常日志
        /// </summary>
        /// <param name="lc"> 上下文 </param>
        /// <param name="e"> 异常 </param>
        public string Exception(LogContent lc, Exception e)
        {
            string re = "发生未处理异常";
            if (lc == null)
            {
                lc = LogContent.Content;
            }
            if (e != null)
            {
                //if (e is SqlException)
                //{
                //    if (AgebullSystemException.SqlExceptionLevel(e as SqlException) > 16)
                //    {
                //        lc.Infomation = "内部错误";
                //        re = string.Format("发生内部错误,系统标识:{0}", lc.QueryKey);
                //    }
                //    else
                //    {
                //        re = string.Format("{1},系统标识:{0}", lc.QueryKey, e.Message);
                //    }
                //}
                //else 
                if (e is SystemException)
                {
                    lc.Infomation = "系统错误";
                    re = string.Format("发生内部错误,系统标识:{0}", lc.QueryKey);
                }
                else if (e is AgebullSystemException)
                {
                    lc.Infomation = "系统错误";
                    re = string.Format("发生内部错误,系统标识:{0}", lc.QueryKey);
                }
                else if (e is BugException)
                {
                    lc.Infomation = "设计错误";
                    re = string.Format("发生设计错误,系统标识:{0}", lc.QueryKey);
                }
                else if (e is AgebullBusinessException)
                {
                    lc.Infomation = e.Message;
                    re = string.Format("{1},系统标识:{0}", lc.QueryKey, e.Message);
                }
                else
                {
                    re = string.Format("发生未知错误,系统标识:{0}", lc.QueryKey);
                }
                lc.LocaleObject.Add(new LocaleItem
                {
                    Name = "Exception",
                    Value = e,
                    Description = lc.Infomation
                });
            }
            this.Log.Error(lc, e);
            return re;
        }

        /// <summary>
        ///   记录异常日志
        /// </summary>
        /// <param name="e"> 异常 </param>
        /// <returns> </returns>
        public string Exception(Exception e)
        {
            return Exception(null, e);
        }

        /// <summary>
        ///   记录消息
        /// </summary>
        /// <param name="msg"> </param>
        public void Record(string msg)
        {
            this.Log.Info(msg);
        }
    }
}
