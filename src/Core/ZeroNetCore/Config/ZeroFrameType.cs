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
        ///     终止符号
        /// </summary>
        public const byte ExtendEnd = 0xFF;
        /// <summary>
        ///     终止符号
        /// </summary>
        public const byte ResultEnd = 0xFE;

        /// <summary>
        ///     全局标识
        /// </summary>
        public const byte GlobalId = 1;

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
        ///     服务认证标识
        /// </summary>
        public const byte LocalId = 8;

        /// <summary>
        /// 调用方的站点类型
        /// </summary>
        public const byte   StationType = 9;

        /// <summary>
        /// 调用方的全局标识
        /// </summary>
        public const byte   CallId = 0xB;
        /// <summary>
        /// 数据方向
        /// </summary>
        public const byte   DataDirection = 0xC;

        /// <summary>
        /// 原样参数
        /// </summary>
        public const byte Original1 = 0x10;
        /// <summary>
        /// 原样参数
        /// </summary>
        public const byte Original2 = 0x11;
        /// <summary>
        /// 原样参数
        /// </summary>
        public const byte Original3 = 0x12;
        /// <summary>
        /// 原样参数
        /// </summary>
        public const byte Original4 = 0x13;
        /// <summary>
        /// 原样参数
        /// </summary>
        public const byte Original5 = 0x14;
        /// <summary>
        /// 原样参数
        /// </summary>
        public const byte Original6 = 0x15;
        /// <summary>
        /// 原样参数
        /// </summary>
        public const byte Original7 = 0x16;
        /// <summary>
        /// 原样参数
        /// </summary>
        public const byte Original8 = 0x17;
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
        public const byte TextContent = (byte)'T';

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
                    return @"请求终止符号";
                case ExtendEnd:// 0;
                    return @"命令终止符号";
                case ResultEnd:// 0;
                    return @"返回终止符号";
                case GlobalId:// 1;
                    return @"全局标识";
                case Station:// 2;
                    return @"站点名称帧";
                case Status:// 3;
                    return @"状态帧";
                case CallId:// 3;
                    return @"调用方的全局标识";
                case StationType:// 3;
                    return @"调用方的站点类型";
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
                case TextContent:// (byte)'T';
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