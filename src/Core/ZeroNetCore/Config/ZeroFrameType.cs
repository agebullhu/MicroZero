namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     帧定义
    /// </summary>
    public class ZeroFrameType
    {
        /// <summary>
        ///     终止符号
        /// </summary>
        public const byte End = 0;

        /// <summary>
        ///     全局标识
        /// </summary>
        public const byte GlobalId = 1;

        /// <summary>
        ///     站点ID
        /// </summary>
        public const byte StationId = 8;

        /// <summary>
        ///     站点名称帧
        /// </summary>
        public const byte Station = 2;

        /// <summary>
        ///     状态帧
        /// </summary>
        public const byte Status = 3;

        /// <summary>
        ///     请求ID
        /// </summary>
        public const byte RequestId = 4;

        /// <summary>
        ///     执行计划
        /// </summary>
        public const byte Plan = 5;

        /// <summary>
        ///     计划时间
        /// </summary>
        public const byte PlanTime = 6;

        /// <summary>
        ///     服务认证标识
        /// </summary>
        public const byte SerivceKey = 7;

        /// <summary>
        ///     参数
        /// </summary>
        public const byte Argument = (byte)'%';

        /// <summary>
        ///     请求者
        /// </summary>
        public const byte Requester = (byte)'>';

        /// <summary>
        ///     发布者/生产者
        /// </summary>
        public const byte Publisher = Requester;

        /// <summary>
        ///     回复者
        /// </summary>
        public const byte Responser = (byte)'<';

        /// <summary>
        ///     订阅者/浪费者
        /// </summary>
        public const byte Subscriber = Responser;

        /// <summary>
        ///     网络上下文信息
        /// </summary>
        public const byte Context = (byte)'#';

        /// <summary>
        ///     请求命令
        /// </summary>
        public const byte Command = (byte)'$';

        /// <summary>
        ///     广播主题
        /// </summary>
        public const byte PubTitle = (byte)'*';

        /// <summary>
        ///     广播副题
        /// </summary>
        public const byte SubTitle = Command;


        /// <summary>
        ///     一般文本内容
        /// </summary>
        public const byte Content = (byte)'T';

        /// <summary>
        ///     JSON文本内容
        /// </summary>
        public const byte JsonValue = (byte)'J';

        /// <summary>
        ///     二进制内容
        /// </summary>
        public const byte BinaryValue = (byte)'B';

        /// <summary>
        ///     二进制内容
        /// </summary>
        public const byte TsonValue = (byte)'V';

        /// <summary>
        ///     说明帧解析
        /// </summary>
        public static string FrameName(byte value)
        {
            switch (value)
            {
                case End:// 0;
                    return @"终止符号";
                case GlobalId:// 1;
                    return @"全局标识";
                case Station:// 2;
                    return @"站点名称帧";
                case Status:// 3;
                    return @"状态帧";
                case RequestId:// 4;
                    return @"请求ID";
                case Plan:// 5;
                    return @"执行计划";
                case SerivceKey:// 5;
                    return @"服务认证标识"; 
                case PlanTime:// 5;
                    return @"计划时间"; 
                case Argument:// (byte)'%';
                    return @"参数";
                case Requester:// (byte)'>';
                    return @"请求者/发布者/生产者";
                case Responser:// (byte)'<';
                    return @"回复者/订阅者/浪费者";
                case PubTitle:// (byte)'*';
                    return @"广播主题";
                case Context:// (byte)'#';
                    return @"网络上下文信息";
                case Command:// (byte)'$';
                    return @"请求命令";
                case Content:// (byte)'T';
                    return @"一般文本内容";
                case JsonValue:// (byte)'J';
                    return @"JSON文本内容";
                case BinaryValue:// (byte)'B';
                    return @"二进制内容";
                default:
                    return "Error";
            }
        }
    }
}