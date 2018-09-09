namespace ZeroMQ
{
    /// <summary>
    /// Socket默认配置
    /// </summary>
    public class SocketOption
    {
        /// <summary>
        /// 重新连接时间间隔
        /// </summary>
        public int ReconnectIvl = 10;
        /// <summary>
        /// 重新连接失败时长
        /// </summary>
        public int ReconnectIvlMax = 500;
        /// <summary>
        /// 连接失败时长
        /// </summary>
        public int ConnectTimeout = 500;
        /// <summary>
        /// 自动关闭时长
        /// </summary>
        public int Linger = 200;
        /// <summary>
        /// 接收超时时长
        /// </summary>
        public int RecvTimeout = 5000;
        /// <summary>
        /// 发送超时时长
        /// </summary>
        public int SendTimeout = 5000;
        /// <summary>
        /// 连接队列数量
        /// </summary>
        public int Backlog = 50000;
        /// <summary>
        /// 心跳间隔时长
        /// </summary>
        public int HeartbeatIvl = 1000;
        /// <summary>
        /// 心跳超时时长
        /// </summary>
        public int HeartbeatTimeout = 200;
        /// <summary>
        /// 心跳TTL
        /// </summary>
        public int HeartbeatTtl = 200;
        /// <summary>
        /// 启用Keeplive
        /// </summary>
        public int TcpKeepalive = 1;
        /// <summary>
        /// TCP发送keepalive消息的频度,单位秒
        /// </summary>
        public int TcpKeepaliveIdle = 4096;
        /// <summary>
        ///当探测没有确认时，重新发送探测的频度,单位秒
        /// </summary>
        public int TcpKeepaliveIntvl = 4096;
    }
}