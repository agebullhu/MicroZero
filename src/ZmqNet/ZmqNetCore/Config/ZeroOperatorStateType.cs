namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 标准操作状态
    /// </summary>
    public enum ZeroOperatorStateType : byte
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        Ok = 0,
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
        Error = 0x81,
        /// <summary>
        /// 操作失败
        /// </summary>
        Failed = 0x82,
        /// <summary>
        /// 找不到站点
        /// </summary>
        NoFind = 0x83,
        /// <summary>
        /// 不支持的操作
        /// </summary>
        NoSupport = 0x84,
        /// <summary>
        /// 参数校验失败
        /// </summary>
        Invalid = 0x85,
        /// <summary>
        /// 超时失败
        /// </summary>
        TimeOut = 0x86,
        /// <summary>
        /// 网络错误
        /// </summary>
        NetError = 0x87,
        /// <summary>
        /// 没有工作对象
        /// </summary>
        NoWorker = 0x88,
        /// <summary>
        /// 管理命令格式错误
        /// </summary>
        CommandArgumentError = 0x89,
        /// <summary>
        /// 安装命令格式错误
        /// </summary>
        InstallArgumentError = 0x8A,
        /// <summary>
        /// 未准备好
        /// </summary>
        NoReady = 0xF0,
        /// <summary>
        /// 本地ZMQ发生错误
        /// </summary>
        LocalZmqError = 0xFC,
        /// <summary>
        /// 本地发送错误
        /// </summary>
        LocalSendError = 0xFD,
        /// <summary>
        /// 本地接收错误
        /// </summary>
        LocalRecvError = 0xFE,
        /// <summary>
        /// 发生异常
        /// </summary>
        Exception = 0xFF
    }
}