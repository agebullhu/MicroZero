using System;
using System.Collections.Generic;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using NetMQ;
using NetMQ.Sockets;
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
            if (ZeroApplication.State != StationState.Run)
                return ZeroNetStatus.NoReadyJson;
            if (!ZeroApplication.Configs.TryGetValue(station, out var config))
            {
                return ZeroNetStatus.NoFindJson;
            }

            List<byte[]> result;
            RequestSocket socket = null;
            try
            {
                socket = config.GetSocket();
                if (socket == null)
                {
                    return ZeroNetStatus.NoReadyJson;
                }
                try
                {
                    socket.SendMoreFrame(Description);
                    socket.SendMoreFrame(commmand);
                    socket.SendMoreFrame(ZeroApplication.Config.StationName);
                    socket.SendMoreFrame(ApiContext.RequestContext.RequestId);
                    socket.SendMoreFrame(JsonConvert.SerializeObject(ApiContext.Current));
                    socket.SendFrame(argument);
                }
                catch (Exception ex)
                {
                    config.Close(socket);
                    socket = null;
                    LogRecorder.Exception(ex);
                    return ZeroNetStatus.NetworkErrorJson;
                }
                try
                {
                    socket.Receive(out result, 1);
                }
                catch (Exception ex)
                {
                    config.Close(socket);
                    socket = null;
                    LogRecorder.Exception(ex);
                    return ZeroNetStatus.UnknowErrorJson;
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

    }
}