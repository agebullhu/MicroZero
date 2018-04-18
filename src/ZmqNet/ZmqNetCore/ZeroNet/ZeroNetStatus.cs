using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Zmq帮助类
    /// </summary>
    public static class ZeroNetStatus
    {
        /// <summary>
        /// 正常状态
        /// </summary>
        public const byte ZeroStatusSuccess = (byte) '+';

        /// <summary>
        /// 错误状态
        /// </summary>
        public const byte ZeroStatusBad = (byte) '-';

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

        /// <summary>
        /// 参数错误
        /// </summary>
        public const string ZeroCommandArgError = "-ArgumentError! must like : call[name][command][argument]";

        /// <summary>
        /// 安装时参数错误
        /// </summary>
        public const string ZeroCommandInstallArgError = "-ArgumentError! must like :install [type] [name]";

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
        /// 参数错误的Json文本
        /// </summary>
        /// <remarks>参数校验不通过</remarks>
        public static readonly string SucceesJson = JsonConvert.SerializeObject(ApiResult.Succees());


        /// <summary>
        /// 参数错误的Json文本
        /// </summary>
        /// <remarks>参数校验不通过</remarks>
        public static readonly string ArgumentErrorJson = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.ArgumentError));

        /// <summary>
        /// 拒绝访问的Json文本
        /// </summary>
        /// <remarks>权限校验不通过</remarks>
        public static readonly string DenyAccessJson = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.DenyAccess));

        /// <summary>
        /// 未知错误的Json文本
        /// </summary>
        public static readonly string UnknowErrorJson = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.UnknowError));

        /// <summary>
        /// 网络错误的Json文本
        /// </summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static readonly string NetworkErrorJson = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.NetworkError));

        /// <summary>
        /// 内部错误的Json文本
        /// </summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static readonly string InnerErrorJson = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.InnerError));

        /// <summary>
        /// 找不到方法的Json文本
        /// </summary>
        /// <remarks>方法未注册</remarks>
        public static readonly string NoFindJson = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.NoFind));

    }
}