using System;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     Zmq帮助类
    /// </summary>
    public static class ZeroHelper
    {
        #region 关闭支持

        /// <summary>
        ///     关闭套接字
        /// </summary>
        public static void CloseSocket(this NetMQSocket socket, string address)
        {
            if (socket == null || address == null)
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

        #endregion

        #region 调用支持

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData<string> Send(this NetMQSocket socket, byte[] desicription, params string[] args)
        {
            var ts = new TimeSpan(0, 0, 3);
            try
            {
                if (args != null && args.Length > 0)
                {
                    if (!socket.TrySendFrame(ts, desicription, true))
                    {
                        return new ZeroResultData<string>
                        {
                            State = ZeroStateType.LocalRecvError
                        };
                    }
                    var i = 0;
                    for (; i < args.Length - 1; i++)
                    {
                        if (!socket.TrySendFrame(ts, args[i], true))
                        {
                            return new ZeroResultData<string>
                            {
                                State = ZeroStateType.LocalRecvError
                            };
                        }
                    }
                    if (!socket.TrySendFrame(ts, args[i]))
                    {
                        return new ZeroResultData<string>
                        {
                            State = ZeroStateType.LocalRecvError
                        };
                    }
                }
                else
                {
                    if (!socket.TrySendFrame(ts, desicription))
                    {
                        return new ZeroResultData<string>
                        {
                            State = ZeroStateType.LocalRecvError
                        };
                    }
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return new ZeroResultData<string>
                {
                    State = ZeroStateType.Exception,
                    Exception = e
                };
            }

            return new ZeroResultData<string>
            {
                State = ZeroStateType.Ok,
                InteractiveSuccess = true
            };
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData<string> Call(this NetMQSocket socket, byte[] desicription, params string[] args)
        {
            var ts = new TimeSpan(0, 0, 3);
            try
            {
                if (args != null && args.Length > 0)
                {
                    if (!socket.TrySendFrame(ts, desicription, true))
                    {
                        return new ZeroResultData<string>
                        {
                            State = ZeroStateType.LocalRecvError
                        };
                    }
                    var i = 0;
                    for (; i < args.Length - 1; i++)
                    {
                        if (!socket.TrySendFrame(ts, args[i], true))
                        {
                            return new ZeroResultData<string>
                            {
                                State = ZeroStateType.LocalRecvError
                            };
                        }
                    }
                    if (!socket.TrySendFrame(ts, args[i]))
                    {
                        return new ZeroResultData<string>
                        {
                            State = ZeroStateType.LocalRecvError
                        };
                    }
                }
                else
                {
                    if (!socket.TrySendFrame(ts, desicription))
                    {
                        return new ZeroResultData<string>
                        {
                            State = ZeroStateType.LocalRecvError
                        };
                    }
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return new ZeroResultData<string>
                {
                    State = ZeroStateType.Exception,
                    Exception = e
                };
            }

            return socket.ReceiveString();
        }

        #endregion

        #region 广播支持

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static readonly byte[] PubDescription =
        {
            5,
            ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.SubTitle,
            ZeroFrameType.Argument,
            ZeroFrameType.End
        };
        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish<T>(this NetMQSocket socket, T content)
            where T : class, IPublishData
        {
            return Publish(socket, content.Title, null, JsonConvert.SerializeObject(content));
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish<T>(this NetMQSocket socket, string title, string subTitle, T content)
            where T : class
        {
            return Publish(socket, title, subTitle, JsonConvert.SerializeObject(content));
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish<T>(this NetMQSocket socket, string title, T content) where T : class
        {
            return Publish(socket, title, null, JsonConvert.SerializeObject(content));
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="item"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this NetMQSocket socket, PublishItem item)
        {
            return Publish(socket, item.Title, item.SubTitle, item.Content);
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this NetMQSocket socket, string title, string subTitle, string content)
        {
            var ts = new TimeSpan(0, 0, 3);
            lock (socket)
            {
                if (!socket.TrySendFrame(ts, PubDescription, true))
                    return false;
                if (!socket.TrySendFrame(ts, title ?? "", true))
                    return false;
                if (!socket.TrySendFrame(ts, ApiContext.RequestContext.RequestId, true))
                    return false;
                if (!socket.TrySendFrame(ts, ZeroApplication.Config.RealName, true))
                    return false;
                if (!socket.TrySendFrame(ts, subTitle ?? "", true))
                    return false;
                if (!socket.TrySendFrame(ts, content ?? ""))
                    return false;
                var result = socket.ReceiveString();
                return result.InteractiveSuccess && result.State == ZeroStateType.Ok;
            }
        }

        #endregion

        #region 接收支持

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tryCnt"></param>
        /// <returns></returns>
        public static ZeroResultData<string> ReceiveString(this NetMQSocket request, int tryCnt = 3)
        {
            var result = new ZeroResultData<string>();

            var ts = new TimeSpan(0, 0, 3);
            try
            {
                byte[] bytes = { 0 };
                var more = false;
                for (var idx = 0; idx <= tryCnt; idx++)
                {
                    if (idx == tryCnt)
                    {
                        result.State = ZeroStateType.LocalRecvError;
                        return result;
                    }

                    if (request.TryReceiveFrameBytes(ts, out bytes, out more) && bytes.Length >0) break;
                }

                result.State = (ZeroStateType)bytes[1];
                var sub = 2;
                //收完消息
                while (more)
                {
                    if (!request.TryReceiveFrameString(ts, out var data, out more)) return result;

                    if (sub >= bytes.Length)
                    {
                        if (more)
                            request.TrySkipMultipartMessage();
                        result.State = ZeroStateType.LocalRecvError;
                        return result;
                    }

                    result.Add(sub < bytes.Length ? bytes[sub++] : ZeroFrameType.BinaryValue, data);
                }

                result.InteractiveSuccess = true;
                return result;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                result.Exception = e;
                result.State = ZeroStateType.Exception;
                return result;
            }
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tryCnt"></param>
        /// <returns></returns>
        public static ZeroResultData<byte[]> Receive(this NetMQSocket request, int tryCnt = 3)
        {
            var result = new ZeroResultData<byte[]>();
            var ts = new TimeSpan(0, 0, 3);
            try
            {
                byte[] bytes = { 0 };
                var more = false;
                for (var idx = 0; idx <= tryCnt; idx++)
                {
                    if (idx <= tryCnt)
                    {
                        result.State = ZeroStateType.LocalRecvError;
                        return result;
                    }

                    if (request.TryReceiveFrameBytes(ts, out bytes, out more)) break;
                }

                result.State = (ZeroStateType)bytes[1];
                var sub = 2;
                //收完消息
                while (more)
                {
                    if (!request.TryReceiveFrameBytes(ts, out var data, out more)) return result;

                    if (sub >= bytes.Length)
                    {
                        if (more)
                            request.TrySkipMultipartMessage();
                        result.State = ZeroStateType.LocalRecvError;
                        return result;
                    }

                    result.Add(sub < bytes.Length ? bytes[sub++] : ZeroFrameType.BinaryValue, data);
                }

                result.InteractiveSuccess = true;
                return result;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                result.Exception = e;
                result.State = ZeroStateType.Exception;
                return result;
            }
        }

        #endregion
    }
}