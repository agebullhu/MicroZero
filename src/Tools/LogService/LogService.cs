using System;
using System.Collections.Generic;
using Agebull.Common.Logging;
using Agebull.Common.Tson;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApi;
using Newtonsoft.Json;

namespace Agebull.MicroZero.LogService
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
            //Subscribe = "Logs";
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
                if (args.Content != null)
                {
                    LogRecorder.BaseRecorder.RecordLog(JsonConvert.DeserializeObject<List<RecordInfo>>(args.Content));
                    return;
                }


                if (args.Tson == null || args.Tson[0] == 0 || args.Tson[0] > 2)
                    return;
                ITsonDeserializer serializer = args.Tson[0] == 1
                    ? new TsonDeserializerV1(args.Tson) as ITsonDeserializer
                    : new TsonDeserializer(args.Tson);

                using (serializer)
                {
                    switch (serializer.DataType)
                    {
                        case TsonDataType.Object:
                            RecordInfo info = new RecordInfo();
                            RecordInfoTson.FromTson(serializer, info);
                            LogRecorder.BaseRecorder.RecordLog(info);
                            break;
                        case TsonDataType.Array:
                            serializer.ReadType();
                            int size = serializer.ReadLen();
                            List<RecordInfo> infos = new List<RecordInfo>();
                            for (int idx = 0; !serializer.IsBad && idx < size; idx++)
                            {
                                info = new RecordInfo();
                                serializer.Begin(out _);
                                RecordInfoTson.FromTson(serializer, info);
                                serializer.End();
                                infos.Add(info);
                            }
                            LogRecorder.BaseRecorder.RecordLog(infos);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }

    }
}