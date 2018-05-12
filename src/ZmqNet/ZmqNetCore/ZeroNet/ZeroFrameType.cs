namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 帧定义
    /// </summary>
    public class ZeroFrameType
    {
        /// <summary>
        /// 终止符号
        /// </summary>
        public const byte End = (byte) 'E';
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