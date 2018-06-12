using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.LogService
{
    /// <inheritdoc />
    /// <summary>
    /// 日志服务
    /// </summary>
    public class RemoteLogStation : SubStation
    { /// <summary>
        ///     刷新
        /// </summary>
        public RemoteLogStation()
        {
            Name = "RemoteLog";
            StationName = "RemoteLog";
            Subscribe = "";
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
                if (args.Title == "Logs")
                {
                    List<RecordInfo> infos = JsonConvert.DeserializeObject<List<RecordInfo>>(args.Content);
                    LogRecorder.BaseRecorder.RecordLog(infos);
                }
                else
                {
                    var info = JsonConvert.DeserializeObject<RecordInfo>(args.Content);
                    LogRecorder.BaseRecorder.RecordLog(info);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }

    }
}