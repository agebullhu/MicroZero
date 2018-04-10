using System;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.LogService
{
    /// <summary>
    /// 日志服务
    /// </summary>
    public class RemoteLogRecorder : SubStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        public RemoteLogRecorder()
        {
            StationName = "RemoteLog";
            Subscribe = "Record";
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
                var info = JsonConvert.DeserializeObject<RecordInfo>(args.Content);
                LogRecorder.Push(info);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
    }
}
