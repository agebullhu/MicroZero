using System;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Zmq帮助类
    /// </summary>
    public static class ZeroHelper
    {
        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="datas"></param>
        /// <param name="tryCnt"></param>
        /// <returns></returns>
        public static bool ReceiveString(this RequestSocket request, out List<string> datas, int tryCnt = 3)
        {
            datas = new List<string>();

            var more = true;
            var cnt = 0;
            var ts = new TimeSpan(0, 0, 3);
            //收完消息
            while (more)
            {
                if (!request.TryReceiveFrameString(ts, out var data, out more))
                {
                    if (++cnt >= tryCnt)
                        return false;
                    more = true;
                }
                datas.Add(data);
            }
            return true;
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tryCnt"></param>
        /// <returns></returns>
        public static string ReceiveString(RequestSocket request, int tryCnt = 3)
        {
            return ReceiveString(request, out var datas, tryCnt) ? datas.FirstOrDefault() : null;
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool SendString(this RequestSocket request, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("args 不能为空");
            var i = 0;
            for (; i < args.Length - 1; i++)
                request.SendFrame(args[i] ?? "", true);
            return request.TrySendFrame(args[i] ?? "");
        }

        /// <summary>
        ///     发送文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Request(this RequestSocket request, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("args 不能为空");
            if (!SendString(request, args))
                throw new Exception("发送失败");
            return ReceiveString(request);
        }


        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="address">请求地址</param>
        /// <param name="args">请求参数</param>
        /// <returns></returns>
        public static string RequestNet(this string address, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("args 不能为空");
            using (var request = new RequestSocket())
            {
                request.Options.Identity = StationProgram.Config.StationName.ToAsciiBytes();
                request.Options.ReconnectInterval = new TimeSpan(0, 0, 3);
                request.Connect(address);
                var res = Request(request, args);
                request.Disconnect(address);
                return res;
            }
        }
    }
}