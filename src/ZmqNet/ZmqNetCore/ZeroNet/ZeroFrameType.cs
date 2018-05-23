namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 请求行为类型
    /// </summary>
    public enum ZeroActionType : sbyte
    {
        /// <summary>
        /// 无特殊说明
        /// </summary>
        None = 0,
        /// <summary>
        /// 工作站点加入
        /// </summary>
        WorkerJoin = 1,
        /// <summary>
        /// 工作站点退出
        /// </summary>
        WorkerLeft = 2,
        /// <summary>
        /// 工作站点等待工作
        /// </summary>
        WrokerListen = 3
    }
    /// <summary>
    /// 标准状态
    /// </summary>
    public enum ZeroStateType : sbyte
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        Success = 0,
        /// <summary>
        /// 计划执行
        /// </summary>
        Plan = 1,
        /// <summary>
        /// 执行中
        /// </summary>
        VoteRuning = 2,
        /// <summary>
        /// 投票已退出
        /// </summary>
        VoteBye = 3,
        /// <summary>
        /// 投票欢迎
        /// </summary>
        VoteWecome = 4,
        /// <summary>
        /// 投票已发送
        /// </summary>
        VoteSend = 5,
        /// <summary>
        /// 投票等待执行
        /// </summary>
        VoteWaiting = 6,
        /// <summary>
        /// 投票已开始
        /// </summary>
        VoteStart = 7,
        /// <summary>
        /// 投票结束
        /// </summary>
        VoteEnd = 8,
        /// <summary>
        /// 发生错误
        /// </summary>
        Error = -1,
        /// <summary>
        /// 操作失败
        /// </summary>
        Failed = -2,
        /// <summary>
        /// 找不到站点
        /// </summary>
        NoFind = -3,
        /// <summary>
        /// 不支持的操作
        /// </summary>
        NoSupport = -4,
        /// <summary>
        /// 参数校验失败
        /// </summary>
        Invalid = -5,
        /// <summary>
        /// 超时失败
        /// </summary>
        TimeOut = -6,
        /// <summary>
        /// 网络错误
        /// </summary>
        NetError = -7,
        /// <summary>
        /// 没有工作对象
        /// </summary>
        NoWorker = -8,
        /// <summary>
        /// 管理命令格式错误
        /// </summary>
        CommandArgumentError = -9,
        /// <summary>
        /// 安装命令格式错误
        /// </summary>
        InstallArgumentError = -10
    }

    /// <summary>
    /// 帧定义
    /// </summary>
    public class ZeroFrameType
    {
        /// <summary>
        /// 终止符号
        /// </summary>
        public const byte End = (byte)'E';
        /// <summary>
        /// 执行计划
        /// </summary>
        public const byte Plan = (byte)'P';
        /// <summary>
        /// 参数
        /// </summary>
        public const byte Argument = (byte)'A';
        /// <summary>
        /// 请求ID
        /// </summary>
        public const byte RequestId = (byte)'I';
        /// <summary>
        /// 请求者
        /// </summary>
        public const byte Requester = (byte)'R';
        /// <summary>
        /// 发布者/生产者
        /// </summary>
        public const byte Publisher = Requester;
        /// <summary>
        /// 回复者
        /// </summary>
        public const byte Responser = (byte)'G';
        /// <summary>
        /// 订阅者/浪费者
        /// </summary>
        public const byte Subscriber = Responser;
        //广播主题
        //#define zero_pub_title  '*'
        /// <summary>
        /// 广播副题
        /// </summary>
        public const byte SubTitle = (byte)'S';
        /// <summary>
        /// 广播副题
        /// </summary>
        public const byte Status = (byte)'S';
        /// <summary>
        /// 网络上下文信息
        /// </summary>
        public const byte Context = (byte)'T';
        /// <summary>
        /// 网络上下文信息
        /// </summary>
        public const byte Command = (byte)'C';
        /// <summary>
        /// 广播副题
        /// </summary>
        public const byte TextValue = (byte)'T';
        /// <summary>
        /// 网络上下文信息
        /// </summary>
        public const byte JsonValue = (byte)'J';
        /// <summary>
        /// 网络上下文信息
        /// </summary>
        public const byte BinaryValue = (byte)'B';
    }
}