using System;
using Agebull.Common.Logging;
using Gboxt.Common.DataModel;
using NetMQ.Sockets;

namespace ZmqNet.Rpc.Core.ZeroNet
{
    /// <summary>
    /// 消息发布
    /// </summary>
    public class Publisher
    {
        /// <summary>
        /// 执行
        /// </summary>
        public static bool Publish(string type, string title, string value)
        {
            var config = StationProgram.GetConfig(type);
            var socket = new RequestSocket();
            try
            {
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
            try
            {
                var word = socket.Request($"{title}\r\n{StationProgram.Config.StationName}\r\n{value}");
                return word != "ok";
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationProgram.WriteLine($"【{type}】request error =>{e.Message}");
                return false;
            }
        }
    }
}