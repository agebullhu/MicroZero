using System;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;

namespace ZmqNet.Rpc.Core.ZeroNet
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
        /// <param name="try_cnt"></param>
        /// <returns></returns>
        public static bool ReceiveString(this RequestSocket request, out List<string> datas, int try_cnt = 3)
        {
            datas = new List<string>();

            var more = true;
            var cnt = 0;
            //收完消息
            while (more)
            {
                string data;
                if (!request.TryReceiveFrameString(new TimeSpan(0, 0, 3), out data, out more))
                {
                    if (++cnt >= try_cnt)
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
        /// <param name="try_cnt"></param>
        /// <returns></returns>
        public static string ReceiveString(RequestSocket request, int try_cnt = 3)
        {
            List<string> datas;
            return ReceiveString(request, out datas) ? datas.FirstOrDefault() : null;
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
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Request(this RequestSocket request, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("args 不能为空");
            if (!SendString(request, args))
                return "";
            return ReceiveString(request);
        }


        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="address"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string RequestNet(this string address, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("args 不能为空");
            using (var request = new RequestSocket())
            {
                request.Options.Identity = StationProgram.Config.StationName.ToAsciiBytes();
                request.Options.ReconnectInterval = new TimeSpan(0, 10, 59);
                request.Connect(address);
                return Request(request, args);
            }
        }
    }
}