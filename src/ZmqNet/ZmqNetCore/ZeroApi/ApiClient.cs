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
            if (StationProgram.State != StationState.Run)
                return ZeroNetStatus.NoReadyJson;
            if (!StationProgram.Configs.TryGetValue(station, out var config))
            {
                return ZeroNetStatus.NoFindJson;
            }

            List<string> result;
            RequestSocket socket = null;
            try
            {
                socket = config.GetSocket();
                try
                {
                    socket.SendMoreFrame(Description);
                    socket.SendMoreFrame(commmand);
                    socket.SendMoreFrame(StationProgram.Config.StationName);
                    socket.SendMoreFrame(ApiContext.RequestContext.RequestId);
                    socket.SendMoreFrame(JsonConvert.SerializeObject(ApiContext.Current));
                    socket.SendFrame(argument);
                }
                catch (Exception ex)
                {
                    config.Close(socket);
                    LogRecorder.Exception(ex);
                    return ZeroNetStatus.NetworkErrorJson;
                }
                try
                {
                    socket.ReceiveString(out result, 1);
                }
                catch (Exception ex)
                {
                    config.Close(socket);
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
            switch (result[1])
            {
                case ZeroNetStatus.ZeroCommandInvalid:
                    return ZeroNetStatus.ArgumentErrorJson;
                case ZeroNetStatus.ZeroCommandNotWorker:
                    return ZeroNetStatus.NoFindJson;
                default:
                    return result[2];
            }
        }

    }
}