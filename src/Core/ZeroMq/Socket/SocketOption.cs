namespace ZeroMQ
{
    /// <summary>
    /// Socket默认配置
    /// </summary>
    public class SocketOption
    {
        /// <summary>
        /// 构造
        /// </summary>
        public SocketOption()
        {
            PoolTimeOut = 500;

            //ReconnectIvl = 200;
            //ReconnectIvlMax = 1400;

            //ConnectTimeout = 3000;

            //Linger = 3000;
            //RecvTimeout = 5000;
            //SendTimeout = 5000;

            //Backlog = 8192;

            //HeartbeatIvl = 0;
            //HeartbeatTimeout = 200;
            //HeartbeatTtl = 200;

            //TcpKeepalive = 1;
            //TcpKeepaliveIdle = 7200;
            //TcpKeepaliveIntvl = 72;

        }

        /// <summary>
        /// 使用EPool的超时时间
        /// </summary>
        public int PoolTimeOut { get; set; }  //  500;

        /// <summary>
        /// 重新连接时间间隔(最小)
        /// </summary>
        public int ReconnectIvl { get; set; }  //  200;
        /// <summary>
        /// 重新连接时间间隔(最长)(200+400+800)
        /// </summary>
        public int ReconnectIvlMax { get; set; }  //  1400;
        /// <summary>
        /// 连接失败时长
        /// </summary>
        public int ConnectTimeout { get; set; }  //  3000;
        /// <summary>
        /// 自动关闭时长
        /// </summary>
        public int Linger { get; set; }  //  200;
        /// <summary>
        /// 接收超时时长
        /// </summary>
        public int RecvTimeout { get; set; }  //  5000;
        /// <summary>
        /// 发送超时时长
        /// </summary>
        public int SendTimeout { get; set; }  //  5000;
        /// <summary>
        /// 连接队列数量
        /// </summary>
        public int Backlog { get; set; }  //  50000;
        /// <summary>
        /// 心跳间隔时长
        /// </summary>
        public int HeartbeatIvl { get; set; }  //  -1;
        /// <summary>
        /// 心跳超时时长
        /// </summary>
        public int HeartbeatTimeout { get; set; }  //  200;
        /// <summary>
        /// 心跳TTL
        /// </summary>
        public int HeartbeatTtl { get; set; }  //  200;

        /// <summary>
        /// 启用Keeplive
        /// </summary>
        public int TcpKeepalive { get; set; }  //  1;
        /// <summary>
        /// TCP发送keepalive消息的频度,单位秒
        /// </summary>
        public int TcpKeepaliveIdle { get; set; }  //  7200;
        /// <summary>
        ///当探测没有确认时，重新发送探测的频度,单位秒
        /// </summary>
        public int TcpKeepaliveIntvl { get; set; }  //  72;
    }
}