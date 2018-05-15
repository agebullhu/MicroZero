using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.Common.Logging;
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
        ///     关闭套接字
        /// </summary>
        public static void CloseSocket(this NetMQSocket socket, string address)
        {
            if (socket == null)
                return;
            try
            {
                socket.Disconnect(address);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e); //一般是无法连接服务器而出错
            }
            socket.Close();
            socket.Dispose();
        }
        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="datas"></param>
        /// <param name="tryCnt"></param>
        /// <returns></returns>
        public static bool ReceiveString(this NetMQSocket request, out List<string> datas, int tryCnt = 3)
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
        /// <param name="datas"></param>
        /// <param name="tryCnt"></param>
        /// <returns></returns>
        public static bool Receive(this NetMQSocket request, out List<byte[]> datas, int tryCnt = 3)
        {
            datas = new List<byte[]>();

            var more = true;
            var cnt = 0;
            var ts = new TimeSpan(0, 0, 30);
            //收完消息
            while (more)
            {
                Msg msg = new Msg();
                msg.InitDelimiter();
                if (!request.TryReceiveFrameBytes(ts, out var bytes, out more))
                {
                    if (++cnt >= tryCnt)
                        return false;
                    more = true;
                }
                else
                {
                    datas.Add(bytes);
                }
            }
            return true;
        }
        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tryCnt"></param>
        /// <returns></returns>
        public static string ReceiveString(NetMQSocket request, int tryCnt = 3)
        {
            return ReceiveString(request, out var datas, tryCnt) ? datas.FirstOrDefault() : null;
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool SendString(this NetMQSocket request, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("args 不能为空");
            return SendStringInner(request, args);
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool SendStringInner(NetMQSocket request, params string[] args)
        {
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
        public static string Request(this NetMQSocket request, params string[] args)
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
                request.Options.Identity = ZeroApplication.Config.Identity;
                request.Options.ReconnectInterval = new TimeSpan(0, 0, 1);
                request.Options.DisableTimeWait = true;
                request.Connect(address);
                
                if (!SendStringInner(request, args))
                    throw new Exception($"{address}:发送失败");
                var res = ReceiveString(request, out var datas, 1) ? datas.FirstOrDefault() : null;
                request.Disconnect(address);
                return res;
            }
        }

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="address">请求地址</param>
        /// <param name="args">请求参数</param>
        /// <returns>返回的所有数据</returns>
        public static List<string> MulitRequestNet(this string address, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("args 不能为空");
            using (var request = new RequestSocket())
            {
                request.Options.Identity = ZeroApplication.Config.Identity;
                request.Options.ReconnectInterval = new TimeSpan(0, 0, 1);
                request.Options.DisableTimeWait = true;
                request.Connect(address);

                if (!SendStringInner(request, args))
                    throw new Exception($"{address}:发送失败");
                ReceiveString(request, out var datas, 1);
                request.Disconnect(address);
                return datas;
            }
        }
    }
}