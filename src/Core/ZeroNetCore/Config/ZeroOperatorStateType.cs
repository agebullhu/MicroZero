namespace Agebull.MicroZero
{
    /// <summary>
    /// 标准操作状态
    /// </summary>
    public enum ZeroOperatorStateType : byte
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        None = 0x0,
        /// <summary>
        /// 操作成功
        /// </summary>
        Ok = 0x1,
        /// <summary>
        /// 计划执行
        /// </summary>
        Plan = 0x2,
        /// <summary>
        /// 执行中
        /// </summary>
        Runing = 0x3,
        /// <summary>
        /// 告别
        /// </summary>
        Bye = 0x4,
        /// <summary>
        /// 投票欢迎
        /// </summary>
        Wecome = 0x5,
        /// <summary>
        /// 投票欢迎
        /// </summary>
        Waiting = 0x6,
        /// <summary>
        /// 投票已发送
        /// </summary>
        VoteSend = 0x70,
        /// <summary>
        /// 投票告别
        /// </summary>
        VoteBye = 0x71,
        /// <summary>
        /// 投票等待执行
        /// </summary>
        VoteWaiting = 0x72,
        /// <summary>
        /// 投票已开始
        /// </summary>
        VoteStart = 0x73,
        /// <summary>
        /// 投票结束
        /// </summary>
        VoteEnd = 0x74,
        /// <summary>
        /// 投票关闭
        /// </summary>
        VoteClose = 0x75,

        /// <summary>
        /// 操作失败
        /// </summary>
        Failed = 0x80,

        /// <summary>
        /// 站点被暂停
        /// </summary>
        Pause = 0x81,

        /// <summary>
        /// 逻辑BUG
        /// </summary>
        Bug = 0xD0,
        /// <summary>
        /// 数据帧错误
        /// </summary>
        FrameInvalid = 0xD1,
        /// <summary>
        /// 参数错误
        /// </summary>
        ArgumentInvalid = 0xD2,
        
        /// <summary>
        /// 发生错误
        /// </summary>
        Error = 0xF0,
        /// <summary>
        /// 找不到站点
        /// </summary>
        NotFind = 0xF1,
        /// <summary>
        /// 没有工作对象
        /// </summary>
        NoWorker = 0xF2,
        /// <summary>
        /// 不支持的操作
        /// </summary>
        NotSupport = 0xF3,
        /// <summary>
        /// 超时失败
        /// </summary>
        TimeOut = 0xF4,
        /// <summary>
        /// 网络错误
        /// </summary>
        NetError = 0xF5,
        /// <summary>
        /// 计划格式错误
        /// </summary>
        PlanError = 0xF6,
        /// <summary>
        /// 远端发送错误
        /// </summary>
        RemoteSendError = 0xF7,
        
        /// <summary>
        /// 远端接收错误
        /// </summary>
        RemoteRecvError = 0xF8,
        /// <summary>
        /// 拒绝访问
        /// </summary>
        DenyAccess = 0xF9,
        /// <summary>
        /// 拒绝服务
        /// </summary>
        Unavailable = 0xFA, 
        /// <summary>
        /// 未准备好
        /// </summary>
        LocalNoReady = 0xFB,
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
        LocalException = 0xFF
    }
}