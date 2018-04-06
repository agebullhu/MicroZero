using System;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.LogService
{
    /// <summary>
    /// 日志服务
    /// </summary>
    public class LogService
    {
        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="arg"> 日志消息 </param>
        internal static void RecordLog(string arg)
        {
            try
            {
                var info = JsonConvert.DeserializeObject<RecordInfo>(arg);
                LogRecorder.Push(info);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
    }
}
