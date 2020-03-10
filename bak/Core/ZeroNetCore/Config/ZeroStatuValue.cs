using Agebull.MicroZero.ZeroApis;

namespace Agebull.MicroZero
{
    /// <summary>
    /// MicroZero状态值
    /// </summary>
    public static class ZeroStatuValue
    {
        /// <summary>
        /// 正常状态
        /// </summary>
        public const byte ZeroStatusSuccess = (byte)'+';

        /// <summary>
        /// 错误状态
        /// </summary>
        public const byte ZeroStatusBad = (byte)'-';

        /// <summary>
        /// 成功
        /// </summary>
        public const string ZeroCommandOk = "+ok";

        /// <summary>
        /// 计划执行
        /// </summary>
        public const string ZeroCommandPlan = "+plan";

        /// <summary>
        /// 错误
        /// </summary>
        public const string ZeroCommandError = "-error";

        /// <summary>
        /// 正在执行
        /// </summary>
        public const string ZeroCommandRuning = "+runing";

        /// <summary>
        /// 成功退出
        /// </summary>
        public const string ZeroCommandBye = "+bye";

        /// <summary>
        /// 成功加入
        /// </summary>
        public const string ZeroCommandWecome = "+wecome";

        /// <summary>
        /// 投票已发出
        /// </summary>
        public const string ZeroVoteSended = "+send";

        /// <summary>
        /// 投票已关闭
        /// </summary>
        public const string ZeroVoteClosed = "+close";

        /// <summary>
        /// 已退出投票
        /// </summary>
        public const string ZeroVoteBye = "+bye";

        /// <summary>
        /// 投票正在进行中
        /// </summary>
        public const string ZeroVoteWaiting = "+waiting";

        /// <summary>
        /// 投票已开始
        /// </summary>
        public const string ZeroVoteStart = "+start";

        /// <summary>
        /// 投票已完成
        /// </summary>
        public const string ZeroVoteEnd = "+end";

        /// <summary>
        /// 找不到主机
        /// </summary>
        public const string ZeroCommandNoFind = "-no find";

        /// <summary>
        /// 帧错误
        /// </summary>
        public const string ZeroCommandInvalid = "-invalid";

        /// <summary>
        /// 不支持的操作
        /// </summary>
        public const string ZeroCommandNoSupport = "-no support";

        /// <summary>
        /// 执行失败
        /// </summary>
        public const string ZeroCommandFailed = "-failes";

        /*// <summary>
        /// 参数错误
        /// </summary>
        public const string ZeroCommandArgError = "-invalid. argument error, must like : call [name] [command] [argument]";

        /// <summary>
        /// 安装时参数错误
        /// </summary>
        public const string ZeroCommandInstallArgError = "-invalid. argument error, must like :install [type] [name]";*/

        /// <summary>
        /// 执行超时
        /// </summary>
        public const string ZeroCommandTimeout = "-time out";

        /// <summary>
        /// 发生网络异常
        /// </summary>
        public const string ZeroCommandNetError = "-net error";

        /// <summary>
        /// 找不到实际处理者
        /// </summary>
        public const string ZeroCommandNotWorker = "-not work";

        /// <summary>
        /// 未知错误
        /// </summary>
        public const string ZeroUnknowError = "-error";

        /// <summary>
        /// 拒绝访问
        /// </summary>
        public const string DenyAccess = "-deny";

        /// <summary>
        /// 状态原始文本
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string Text(this ZeroOperatorStateType state)
        {
            switch (state)
            {
                case ZeroOperatorStateType.Ok:
                    return ZeroCommandOk;
                case ZeroOperatorStateType.Plan:
                    return ZeroCommandPlan;
                case ZeroOperatorStateType.Runing:
                    return ZeroCommandRuning;
                case ZeroOperatorStateType.Bye:
                    return ZeroCommandBye;
                case ZeroOperatorStateType.Wecome:
                    return ZeroCommandWecome;
                case ZeroOperatorStateType.VoteSend:
                    return ZeroVoteSended;
                case ZeroOperatorStateType.Waiting:
                case ZeroOperatorStateType.VoteWaiting:
                    return ZeroVoteWaiting;
                case ZeroOperatorStateType.VoteBye:
                    return ZeroCommandBye;
                case ZeroOperatorStateType.VoteStart:
                    return ZeroVoteStart;
                case ZeroOperatorStateType.VoteClose:
                    return ZeroVoteClosed;
                case ZeroOperatorStateType.VoteEnd:
                    return ZeroVoteEnd;

                case ZeroOperatorStateType.Error:
                    return ZeroCommandError;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    return ZeroCommandFailed;
                case ZeroOperatorStateType.NotFind:
                    return ZeroCommandNoFind;
                case ZeroOperatorStateType.NotSupport:
                    return ZeroCommandNoSupport;
                case ZeroOperatorStateType.FrameInvalid:
                    return ZeroCommandInvalid;
                case ZeroOperatorStateType.TimeOut:
                    return ZeroCommandTimeout;
                case ZeroOperatorStateType.NetError:
                    return ZeroCommandNetError;
                case ZeroOperatorStateType.NoWorker:
                    return ZeroCommandNotWorker;
                //case ZeroOperatorStateType.CommandArgumentError:
                //    return ZeroCommandArgError;
                //case ZeroOperatorStateType.InstallArgumentError:
                //    return ZeroCommandInstallArgError;
                case ZeroOperatorStateType.LocalRecvError:
                    return "-error. local can't recv data";
                case ZeroOperatorStateType.LocalSendError:
                    return "-error. local can't send data";
                case ZeroOperatorStateType.LocalException:
                    return "-error. local throw exception";
                case ZeroOperatorStateType.None:
                    return "-unknow";
                case ZeroOperatorStateType.PlanError:
                    return "-error. plan argument error";
                case ZeroOperatorStateType.RemoteSendError:
                    return "-error. remote station or ZeroCenter send error";
                case ZeroOperatorStateType.RemoteRecvError:
                    return "-error. remote station or ZeroCenter recv error";
                case ZeroOperatorStateType.LocalNoReady:
                    return "-error. ZeroApplication no ready.";
                case ZeroOperatorStateType.LocalZmqError:
                    return "-error. ZeroMQ  error.";
                case ZeroOperatorStateType.DenyAccess:
                    return DenyAccess;
                default:
                    return ZeroCommandError;
            }
        }

        /// <summary>
        /// 状态转换
        /// </summary>
        /// <param name="state"></param>
        /// <param name="remote">是否远程状态</param>
        /// <returns></returns>
        public static UserOperatorStateType ToOperatorStatus(this ZeroOperatorStateType state,bool remote)
        {
            if (state < ZeroOperatorStateType.Failed)
                return UserOperatorStateType.Success;
            if (state < ZeroOperatorStateType.Bug)
                return UserOperatorStateType.LogicalError;
            if (state < ZeroOperatorStateType.Error)
                return UserOperatorStateType.FormalError;
            if (state <= ZeroOperatorStateType.NotSupport)
                return UserOperatorStateType.NotFind;
            if (state == ZeroOperatorStateType.DenyAccess)
                return UserOperatorStateType.DenyAccess;
            if (state == ZeroOperatorStateType.Unavailable)
                return UserOperatorStateType.Unavailable;
            if (state == ZeroOperatorStateType.LocalException)
                return remote ? UserOperatorStateType.RemoteException : UserOperatorStateType.LocalException;
            if (state >= ZeroOperatorStateType.LocalNoReady || state == ZeroOperatorStateType.TimeOut)
                return remote ? UserOperatorStateType.RemoteError : UserOperatorStateType.LocalError;
            return UserOperatorStateType.RemoteError;
        }
    }

}