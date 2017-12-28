using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Agebull.Zmq.Rpc
{
    //网络状态
    //网络命令
    using NET_COMMAND = UInt16;
    //网络命令状态 -0x10之后为自定义错误
    using COMMAND_STATE = Int32;

    /// <summary>
    ///     网络命令事件处理Handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="arg"></param>
    public delegate void CommandHandler(CommandArgument sender, CommandArgument arg);

    /// <summary>
    ///     命令的环境相关的定义
    /// </summary>
    public class RpcEnvironment
    {
        #region 命令标识

        /// <summary>
        ///     系统通知
        /// </summary>
        public const ushort NET_COMMAND_SYSTEM_NOTIFY = 0x1;

        /// <summary>
        ///     业务通知
        /// </summary>
        public const ushort NET_COMMAND_BUSINESS_NOTIFY = 0x2;

        /// <summary>
        ///     命令请求(有返回)
        /// </summary>
        public const ushort NET_COMMAND_CALL = 0x3;

        /// <summary>
        ///     命令请求(有返回)
        /// </summary>
        public const ushort NET_COMMAND_RESULT = 0x4;

        #endregion

        #region 常量定义

        /// <summary>
        ///     序列化头大小（长度4+类型4+版本1+类型1）
        /// </summary>
        public const int SERIALIZE_HEAD_LEN = 10;

        /// <summary>
        ///     序列化基本大小（长度4+类型4+版本1+类型1+结束4）
        /// </summary>
        public const int SERIALIZE_BASE_LEN = 14;

        /// <summary>
        ///     数据修改通知
        /// </summary>
        public const ushort NET_COMMAND_DATA_CHANGED = 0xFFFE;

        /// <summary>
        ///     数据推送
        /// </summary>
        public const ushort NET_COMMAND_DATA_PUSH = 0xFFFF;

        /*网络命令状态 -0x10之后为自定义错误*/
        /// <summary>
        ///     当前发送的为数据
        /// </summary>
        public const int NET_COMMAND_STATE_DATA = 0x0;

        /// <summary>
        ///     命令发送中
        /// </summary>
        public const int NET_COMMAND_STATE_SENDING = 0x7;

        /// <summary>
        ///     命令已发送
        /// </summary>
        public const int NET_COMMAND_STATE_SENDED = 0x8;

        /// <summary>
        ///     命令已在服务端排队
        /// </summary>
        public const int NET_COMMAND_STATE_WAITING = 0x9;

        /// <summary>
        ///     命令已执行完成
        /// </summary>
        public const int NET_COMMAND_STATE_SUCCEED = 0xA;

        /// <summary>
        ///     命令发送出错
        /// </summary>
        public const int NET_COMMAND_STATE_NETERROR = 0x6;

        /// <summary>
        ///     未知错误(未收到回执)
        /// </summary>
        public const int NET_COMMAND_STATE_UNKNOW = 0x5;

        /// <summary>
        ///     服务器未知错误(系统异常)
        /// </summary>
        public const int NET_COMMAND_STATE_SERVER_UNKNOW = 0x4;

        /// <summary>
        ///     本地未知错误(系统异常)
        /// </summary>
        public const int NET_COMMAND_STATE_CLIENT_UNKNOW = 0x3;

        /// <summary>
        ///     数据重复处理
        /// </summary>
        public const int NET_COMMAND_STATE_DATA_REPEAT = 0x2;

        /// <summary>
        ///     命令不允许执行
        /// </summary>
        public const int NET_COMMAND_STATE_CANNOT = 0x1;

        /// <summary>
        ///     参数错误
        /// </summary>
        public const int NET_COMMAND_STATE_ARGUMENT_INVALID = -1;

        /// <summary>
        ///     逻辑错误
        /// </summary>
        public const int NET_COMMAND_STATE_LOGICAL_ERROR = -2;

        /// <summary>
        ///     CRC错误
        /// </summary>
        public const int NET_COMMAND_STATE_CRC_ERROR = -3;

        /// <summary>
        ///     未知数据
        /// </summary>
        public const int NET_COMMAND_STATE_UNKNOW_DATA = -4;

        /// <summary>
        ///     已达最大重试次数
        /// </summary>
        public const int NET_COMMAND_STATE_RETRY_MAX = -5;

        /// <summary>
        ///     已超时
        /// </summary>
        public const int NET_COMMAND_STATE_TIME_OUT = -6;


        /// <summary>
        ///     最大重试次数
        /// </summary>
        public const int NET_COMMAND_RETRY_MAX = 5;

        /// <summary>
        ///     令牌字段长度
        /// </summary>
        public const int GUID_LEN = 34;

        /// <summary>
        ///     命令的网络头长度(不包含CRC字段)
        /// </summary>
        public const int NETCOMMAND_BODY_LEN = 76;

        /// <summary>
        ///     命令的网络头长度
        /// </summary>
        public const int NETCOMMAND_HEAD_LEN = 80;

        /// <summary>
        ///     命令的网络数据长度
        /// </summary>
        public const int NETCOMMAND_LEN = 84;

        /// <summary>
        ///     取得命令对象的实际长度
        /// </summary>
        /// <param name="cmd_call"></param>
        /// <returns></returns>
        public static int get_cmd_len(CommandArgument cmd_call)
        {
            return cmd_call.dataLen == 0 ? NETCOMMAND_LEN : cmd_call.dataLen + NETCOMMAND_HEAD_LEN;
        }

        #endregion

        #region 运行状态

        /// <summary>
        ///     客户端用户标识
        /// </summary>
        /// <returns></returns>
        internal static byte[] Token { get; set; }

        /// <summary>
        ///     当前运行状态
        /// </summary>
        protected static volatile ZmqNetStatus m_netState = ZmqNetStatus.None;

        /// <summary>
        ///     运行状态
        /// </summary>
        /// <returns></returns>
        public static ZmqNetStatus NetState => m_netState;

        /// <summary>
        ///     当前启动了多少命令线程
        /// </summary>
        protected static volatile int m_commandThreadCount;

        /// <summary>
        ///     当前启动了多少命令线程
        /// </summary>
        /// <returns></returns>
        public static int CommandThreadCount => m_commandThreadCount;

        /// <summary>
        ///     登记线程开始
        /// </summary>
        public static void set_command_thread_start()
        {
            m_commandThreadCount++;
            Console.WriteLine("网络处理线程数量{0}启动", m_commandThreadCount);
        }

        /// <summary>
        ///     登记线程关闭
        /// </summary>
        public static void set_command_thread_end()
        {
            m_commandThreadCount--;
            Console.WriteLine("网络处理线程数量{0}关闭", m_commandThreadCount);
        }

        #endregion

        #region 系统状态

        /// <summary>
        ///     当前系统处于哪个流程中
        /// </summary>
        public static RpcFlowType CurrentFlow { get; internal set; }

        /// <summary>
        ///     是否用户模式
        /// </summary>
        public static bool OnUserModel => CurrentFlow > RpcFlowType.UserInitialized &&
                                          CurrentFlow < RpcFlowType.EnforceProcessing;

        /// <summary>
        ///     用户是否可操作
        /// </summary>
        public static bool UserCanDo => CurrentFlow == RpcFlowType.Browsing;

        #endregion

        #region 命令缓存

        /// <summary>
        ///     命令缓存字典（RequstId，Argument）
        /// </summary>
        private static readonly Dictionary<string, CommandArgument> m_requestCommands =
            new Dictionary<string, CommandArgument>();

        /// <summary>
        ///     缓存命令
        /// </summary>
        /// <param name="command"></param>
        public static void CacheCommand(CommandArgument command)
        {
            command.TimeSpamp = DateTime.Now;
            if (!m_requestCommands.ContainsKey(command.commandInfo))
                m_requestCommands.Add(command.commandInfo, command);
        }

        /// <summary>
        ///     同步到缓存命令中
        /// </summary>
        /// <param name="command"></param>
        public static CommandArgument SyncCacheCommand(CommandArgument command)
        {
            if (!m_requestCommands.ContainsKey(command.commandInfo))
                return null;
            var old = m_requestCommands[command.commandInfo];
            old.cmdState = command.cmdState;
            old.Data = command.Data;
            command.TimeSpamp = DateTime.Now;
            return old;
        }

        /// <summary>
        ///     删除缓存命令
        /// </summary>
        /// <param name="command"></param>
        public static void RemoveCacheCommand(CommandArgument command)
        {
            m_requestCommands.Remove(command.commandInfo);
        }


        /// <summary>
        ///     删除缓存命令
        /// </summary>
        public static void ClearCacheCommand()
        {
            m_requestCommands.Clear();
        }

        /// <summary>
        ///     超时检测
        /// </summary>
        public static void TimeOutCheckTask()
        {
            set_command_thread_start();
            while (NetState <= ZmqNetStatus.Runing)
            {
                Thread.SpinWait(10000);
                var date = DateTime.Now;
                var array = m_requestCommands.Values.ToArray();
                foreach (var cmd in array)
                {
                    if (cmd.cmdState == NET_COMMAND_STATE_TIME_OUT)
                        continue;
                    if ((date - cmd.TimeSpamp).Minutes < 1)
                        continue;
                    cmd.OnEnd?.Invoke(cmd);
                    cmd.cmdState = NET_COMMAND_STATE_TIME_OUT;
                    cmd.OnRequestStateChanged(cmd);
                }
            }
            set_command_thread_end();
        }

        #endregion
    }
}