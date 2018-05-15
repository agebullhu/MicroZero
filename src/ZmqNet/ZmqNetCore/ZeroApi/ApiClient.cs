using System;
using System.Collections.Generic;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using NetMQ;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public static class ApiClient
    {
        /// <summary>
        /// 请求格式说明
        /// </summary>
        static readonly byte[] Description = new byte[]
        {
            5,
            ZeroFrameType.Command,
            ZeroFrameType.Requester,
            ZeroFrameType.RequestId,
            ZeroFrameType.Context,
            ZeroFrameType.Argument,
            ZeroFrameType.End
        };
        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string Call(string station, string commmand, string argument)
        {
            string result = null;
            using (MonitorStepScope.CreateScope("内部Zero调用"))
            {
                try
                {
                    LogRecorder.MonitorTrace($"Station:{station},Command:{commmand}");
                    result = CallInner(station, commmand, argument);
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                    LogRecorder.MonitorTrace($"发生异常：{ex.Message}");
                    result = ZeroNetStatus.NetworkErrorJson;
                }
                finally
                {
                    LogRecorder.MonitorTrace($"Result:{result}");
                }
            }
            return result;
        }
        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string CallInner(string station, string commmand, string argument)
        {
            if (ZeroApplication.State != StationState.Run)
                return ZeroNetStatus.NoReadyJson;
            if (!ZeroApplication.Configs.TryGetValue(station, out var config))
            {
                return ZeroNetStatus.NoFindJson;
            }
            var socket = config.GetSocket();
            if (socket == null)
            {
                return ZeroNetStatus.NoReadyJson;
            }
            List<byte[]> result;
            try
            {
                if (!Send(socket, out var message,
                     commmand,
                     ZeroApplication.Config.StationName,
                     ApiContext.RequestContext.RequestId,
                     JsonConvert.SerializeObject(ApiContext.Current),
                    argument))
                {
                    config.Close(socket);
                    socket = null;
                    return message;
                }

                if (!Receive(socket, out message, out result))
                {
                    config.Close(socket);
                    socket = null;
                    return message;
                }
            }
            finally
            {
                config.Free(socket);
            }
            if (result.Count == 0)
                return ZeroNetStatus.TimeOutJson;
            int size = result[0][0];
            string value = null;
            for (int i = 1; i <= size && i < result[0].Length; i++)
            {
                switch (result[0][i])
                {
                    case ZeroFrameType.JsonValue:
                        value = result[i].FromAsciBytes();
                        continue;
                    case ZeroFrameType.Status:
                        if (result[i][0] == ZeroNetStatus.ZeroStatusBad)
                        {
                            switch (result[i].FromAsciBytes())
                            {
                                case ZeroNetStatus.ZeroCommandInvalid:
                                    return ZeroNetStatus.ArgumentErrorJson;
                                case ZeroNetStatus.ZeroCommandNotWorker:
                                    return ZeroNetStatus.NoFindJson;
                                default:
                                    return ZeroNetStatus.InnerErrorJson;
                            }
                        }
                        continue;
                    case ZeroFrameType.End:
                        return value ?? ZeroNetStatus.SucceesJson;
                }
            }
            return value;
        }

        private static bool Send(NetMQSocket socket, out string result, params string[] values)
        {
            TimeSpan timeout = new TimeSpan(0, 0, 0, 1);
            try
            {
                bool success = socket.TrySendFrame(timeout, Description, true);
                if (!success)
                {
                    result = ZeroNetStatus.TimeOutJson;
                    return false;
                }

                int last = values.Length - 1;
                for (var index = 0; index <= last; index++)
                {
                    var value = values[index];
                    success = socket.TrySendFrame(timeout, value, index != last);
                    if (!success)
                    {
                        result = ZeroNetStatus.TimeOutJson;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

                LogRecorder.Exception(ex);
                result = ZeroNetStatus.NetworkErrorJson;
                return false;
            }
            result = null;
            return true;
        }
        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="datas"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        static bool Receive(NetMQSocket request, out string message, out List<byte[]> datas)
        {
            datas = new List<byte[]>();

            var timout = new TimeSpan(0, 0, 3);
            try
            {
                var more = true;
                var cnt = 0;
                //收完消息
                while (more)
                {
                    Msg msg = new Msg();
                    msg.InitDelimiter();
                    if (!request.TryReceiveFrameBytes(timout, out var bytes, out more))
                    {
                        if (++cnt >= 3)
                        {
                            message = ZeroNetStatus.TimeOutJson;
                            return false;
                        }
                        more = true;
                    }
                    else
                    {
                        datas.Add(bytes);
                    }
                }
                message = null;
                return true;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                message = ZeroNetStatus.UnknowErrorJson;
                return false;
            }
        }
    }
}