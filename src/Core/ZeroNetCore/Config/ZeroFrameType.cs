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
        ///     内部命令
        /// </summary>
        public const byte InnerCommand = Status;

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
        public const byte StationType = 9;

        /// <summary>
        /// 调用方的全局标识
        /// </summary>
        public const byte CallId = 0xB;
        /// <summary>
        /// 数据方向
        /// </summary>
        public const byte DataDirection = 0xC;

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
                //终止符号
                case End: return nameof(End); 
                //终止符号
                case ExtendEnd: return nameof(ExtendEnd); 
                //终止符号
                case ResultEnd: return nameof(ResultEnd); 
                //全局标识
                case GlobalId: return nameof(GlobalId); 
                //站点名称帧
                case Station: return nameof(Station); 
                //状态帧
                case Status: return nameof(Status) + "/" + nameof(InnerCommand); 
                //请求ID
                case RequestId: return nameof(RequestId); 
                //执行计划
                case Plan: return nameof(Plan); 
                //计划时间
                case PlanTime: return nameof(PlanTime); 
                //服务认证标识
                case SerivceKey: return nameof(SerivceKey); 
                //服务认证标识
                case LocalId: return nameof(LocalId); 
                //调用方的站点类型
                case StationType: return nameof(StationType); 
                //调用方的全局标识
                case CallId: return nameof(CallId); 
                //数据方向
                case DataDirection: return nameof(DataDirection); 
                //原样参数
                case Original1: return nameof(Original1); 
                //原样参数
                case Original2: return nameof(Original2); 
                //原样参数
                case Original3: return nameof(Original3); 
                //原样参数
                case Original4: return nameof(Original4); 
                //原样参数
                case Original5: return nameof(Original5); 
                //原样参数
                case Original6: return nameof(Original6); 
                //原样参数
                case Original7: return nameof(Original7); 
                //原样参数
                case Original8: return nameof(Original8); 
                //参数
                case Argument: return nameof(Argument); 
                //请求者
                case Requester: return nameof(Requester) + "/" + nameof(Publisher); 
                //回复者
                case Responser: return nameof(Responser) + "/" + nameof(Subscriber);
                //网络上下文信息
                case Context: return nameof(Context);
                //请求命令
                case Command: return nameof(Command) + "/" + nameof(SubTitle);
                //广播主题
                case PubTitle: return nameof(PubTitle);
                //一般文本内容
                case TextContent: return nameof(TextContent);
                //JSON文本内容
                case JsonValue: return nameof(JsonValue);
                //二进制内容
                case BinaryValue: return nameof(BinaryValue);
                //二进制内容
                case TsonValue: return nameof(TsonValue);
                default: return "Error";
            }
        }

    }

}