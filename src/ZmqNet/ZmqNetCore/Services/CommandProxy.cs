using System;
using System.Threading;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using Yizuan.Service.Api;


namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 远程命令代理对象
    /// </summary>
    public class RpcProxy
    {
        #region 基础数据

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            RpcEnvironment.CurrentFlow = RpcFlowType.GlobalInitializing;
            GlobalsFuturesOption.ReigsterEntityType();
            RpcCore.Run();
            RpcEnvironment.CurrentFlow = RpcFlowType.GlobalInitialized;
        }

        /// <summary>
        ///     执行销毁时
        /// </summary>
        public static void Destroy()
        {
            RpcEnvironment.CurrentFlow = RpcFlowType.Exiting;
            var cnt = 0;
            while (RpcEnvironment.CurrentFlow == RpcFlowType.Exiting && cnt++ < 30)
                Thread.SpinWait(100);
            RpcCore.ShutDown();
            RpcEnvironment.ClearCacheCommand();
            RpcEnvironment.CurrentFlow = RpcFlowType.Destroied;
        }

        #endregion

        #region 通用消息处理

        /// <summary>
        ///     消息
        /// </summary>
        public static string Message
        {
            set
            {
                Console.WriteLine(value);
            }
        }

        #endregion

        #region 命令调用

        /// <summary>
        ///     广播
        /// </summary>
        /// <returns></returns>
        public static CommandArgument Publish<T>(string command, T argument)
        {
            var cmd = new CommandArgument
            {
                userToken = ApiContext.MyServiceKey,
                commandName = command,
                requestId = RandomOperate.Generate(12),
                cmdId = RpcEnvironment.NET_COMMAND_BUSINESS_NOTIFY,
                Data = new StringArgumentData
                {
                    Argument = JsonConvert.SerializeObject(argument)
                }
            };
            cmd.RequestStateChanged += OnRequestStateChanged;
            RpcCore.Singleton.Request.request_net_cmmmand(cmd, false);
            return cmd;
        }
        /// <summary>
        ///     请求命令
        /// </summary>
        /// <returns></returns>
        public static object Call(string command)
        {
            var ev = new ManualResetEvent(false);
            object result = null;
            var cmd = new CommandArgument
            {
                userToken = ApiContext.MyServiceKey,
                commandName = command,
                requestId = RandomOperate.Generate(12),
                cmdId = RpcEnvironment.NET_COMMAND_CALL,
                Data = new StringArgumentData(),
                OnEnd = res =>
                {
                    var data = (StringArgumentData)res.Data;
                    result = data?.Argument;
                    ev.Set();
                }
            };
            cmd.RequestStateChanged += OnRequestStateChanged;
            Message = "Start";
            RpcCore.Singleton.Request.request_net_cmmmand(cmd);
            ev.WaitOne();
            Message = "End";
            return result;
        }

        /// <summary>
        ///     请求命令
        /// </summary>
        /// <returns></returns>
        public static object Call<T>(string command, T argument)
        {
            var ev = new ManualResetEvent(false);
            object result = null;
            var cmd = new CommandArgument
            {
                userToken = ApiContext.MyServiceKey,
                commandName = command,
                requestId = RandomOperate.Generate(12),
                cmdId = RpcEnvironment.NET_COMMAND_CALL,
                Data = new StringArgumentData
                {
                    Argument = JsonConvert.SerializeObject(argument)
                },
                OnEnd = res =>
                {
                    var data = (StringArgumentData)res.Data;
                    if (data == null)
                    {
                        result = null;
                    }
                    else
                    {
                        result = data.Argument;
                    }
                    ev.Set();
                }
            };
            cmd.RequestStateChanged += OnRequestStateChanged;
            Message = "Start";
            RpcCore.Singleton.Request.request_net_cmmmand(cmd);
            ev.WaitOne();
            Message = "End";
            return result;
        }


        /// <summary>
        ///     发送文本消息
        /// </summary>
        /// <returns></returns>
        public static CommandArgument Result<T>(T argument)
        {
            var cmd = new CommandArgument
            {
                userToken = RpcContext.Current.Argument.Command.userToken,
                commandName = RpcContext.Current.Argument.Command.commandName,
                requestId = RpcContext.Current.Argument.Command.requestId,
                cmdId = RpcEnvironment.NET_COMMAND_RESULT,
                cmdState = RpcEnvironment.NET_COMMAND_STATE_SUCCEED,
                Data = new StringArgumentData
                {
                    Argument = JsonConvert.SerializeObject(argument)
                }
            };
            cmd.RequestStateChanged += OnRequestStateChanged;
            RpcCore.Singleton.Request.request_net_cmmmand(cmd);
            return cmd;
        }

        /// <summary>
        ///     消息处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="arg">最新接收的参数（如果这不是响应状态则与发送者相同）</param>
        public static void OnRequestStateChanged(CommandArgument sender, CommandArgument arg)
        {
            switch (arg.cmdState)
            {
                case RpcEnvironment.NET_COMMAND_STATE_SENDING:
                    Message = "正在发送请求";
                    return;
                case RpcEnvironment.NET_COMMAND_STATE_UNKNOW_DATA:
                    Message = "正在接收远程数据接";
                    return;
                case RpcEnvironment.NET_COMMAND_STATE_WAITING:
                    Message = "服务器正在处理请求";
                    return;
                case RpcEnvironment.NET_COMMAND_STATE_DATA:
                    Message = "远程数据推送";
                    return;
                case RpcEnvironment.NET_COMMAND_STATE_UNKNOW:
                    Message = "服务器未正确响应";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_CRC_ERROR:
                    Message = "CRC校验错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_NETERROR:
                    Message = "发生网络错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_RETRY_MAX:
                    Message = "超过最大错误重试次数";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_LOGICAL_ERROR:
                    Message = "系统内部错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_SERVER_UNKNOW:
                    Message = "服务器内部错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_TIME_OUT:
                    Message = "请求超时";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_DATA_REPEAT:
                    Message = "应废弃的重复请求";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_ARGUMENT_INVALID:
                    Message = "参数数错误";
                    break;
                case RpcEnvironment.NET_COMMAND_STATE_SUCCEED:
                    Message = "执行成功";
                    break;
            }
            sender.RequestStateChanged -= OnRequestStateChanged;
            RpcEnvironment.RemoveCacheCommand(arg);

            sender.OnEnd?.Invoke(arg);
        }

        #endregion
    }
}