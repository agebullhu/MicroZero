using System;
using System.Collections.Generic;
using Agebull.Common.Logging;
using Gboxt.Common.DataModel;
using NetMQ.Sockets;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 消息发布
    /// </summary>
    public class Publisher
    {
        /// <summary>
        /// 保持长连接的连接池
        /// </summary>
        static readonly Dictionary<string, RequestSocket> publishers = new Dictionary<string, RequestSocket>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 执行
        /// </summary>
        public static bool Publish(string type, string title, string value)
        {
            RequestSocket socket;
            lock (publishers)
            {
                if (!publishers.TryGetValue(type, out socket))
                {
                    try
                    {
                        var config = StationProgram.GetConfig(type);
                        if (config == null)
                        {
                            StationProgram.WriteLine($"【{type}】connect error =>无法拉取配置");
                            return false;
                        }
                        socket = new RequestSocket();
                        socket.Options.Identity = RandomOperate.Generate(8).ToAsciiBytes();
                        socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                        socket.Connect(config.OutAddress);
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        StationProgram.WriteLine($"【{type}】connect error =>{e.Message}");
                        return false;
                    }
                    publishers.Add(type, socket);
                }
            }

            lock (socket)
            {
                try
                {
                    var word = socket.Request($"{title}\r\n{StationProgram.Config.StationName}\r\n{value}");
                    StationProgram.WriteLine($"【{type}】{word}");
                    return word == "ok";
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    StationProgram.WriteLine($"【{type}】request error =>{e.Message}");
                    lock (publishers)
                    {
                        publishers.Remove(type);
                    }
                    return false;
                }
            }
        }
    }
}