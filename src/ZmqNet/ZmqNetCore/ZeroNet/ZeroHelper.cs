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

        //正常状态
        public const byte zero_status_success = (byte)'+';
        //错误状态
        public const byte zero_status_bad = (byte)'-';
        //终止符号
        public const byte zero_end = (byte)'?';
        //执行计划
        public const byte zero_plan = (byte)'@';
        //参数
        public const byte zero_arg = (byte)'$';
        //请求ID
        public const byte zero_request_id = (byte)':';
        //请求者/生产者
        public const byte zero_requester = (byte)'>';
        //发布者/生产者
        public const byte zero_pub_publisher = zero_requester;
        //回复者/浪费者
        public const byte zero_responser = (byte)'<';
        //订阅者/浪费者
        public const byte zero_pub_subscriber = zero_responser;
        //广播主题
        public const byte zero_pub_title = (byte)'*';
        //广播副题
        public const byte zero_pub_sub = (byte)'&';
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
            //收完消息
            while (more)
            {
                if (!request.TryReceiveFrameString(new TimeSpan(0, 0, 3), out var data, out more))
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
                request.Options.ReconnectInterval = new TimeSpan(0, 0, 3);
                request.Connect(address);
                return Request(request, args);
            }
        }
    }
}