using System;
using System.Diagnostics;
using System.Text;
using Agebull.Common.DataModel;
using Gboxt.Common.DataModel;
using Yizuan.Service.Api;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///     命令的网络调用参数结构
    /// </summary>
    public class CommandArgument
    {
        #region 其它

        /// <summary>
        ///     到文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("userToken:{0}", userToken);
            sb.AppendFormat(",requestId:{0}", requestId);
            sb.AppendFormat(",commandName:{0}", commandName);
            sb.AppendFormat(",id:{0}", cmdId);
            sb.AppendFormat(",try_num:{0}", tryNum);
            sb.AppendFormat(",cmd_state:{0}", cmdState);
            sb.AppendFormat(",crc_code:{0}", crcCode);
            sb.AppendFormat(",data_len:{0}", dataLen);
            sb.AppendFormat(",data_type_id:{0}", dataTypeId);
            return sb.ToString();
        }

        #endregion

        #region 请求参数

        /// <summary>
        ///     服务标识
        /// </summary>
        public string userToken;
        
        /// <summary>
        ///     命令标识(用于异步回发后的命令状态对应)长度12位
        /// </summary>
        public string requestId;

        /// <summary>
        /// 
        /// </summary>
        public string _commandInfo;

        /// <summary>
        ///     命令(命令名称 | 命令标识)
        /// </summary>
        public string commandInfo
        {
            get
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(commandName) && commandName.Length <= 20);
                if (requestId == null)
                    requestId = RandomOperate.Generate(12);
                return $"{commandName}|{requestId}";
            }
            set
            {
                if (value == null)
                {
                    commandName = null;
                    requestId = null;
                    return;
                }
                var words = value.Split('|');
                if (words.Length == 2)
                {
                    commandName = words[0];
                    requestId = words[1];
                }
                else
                {
                    commandName = null;
                    requestId = null;
                }
            }
        }

        /// <summary>
        ///     命令名称(小于20字符的文字字母)
        /// </summary>
        public string commandName;
        
        /// <summary>
        ///     命令标识(调用方设置)
        /// </summary>
        public ushort cmdId;

        /// <summary>
        ///     命令重发次数,在事件中要设置为-1而取消重试
        /// </summary>
        public short tryNum;

        /// <summary>
        ///     命令状态(0为数据)
        /// </summary>
        public int cmdState;

        /// <summary>
        ///     命令头的CRC16校验码
        /// </summary>
        public uint crcCode;

        /// <summary>
        ///     参数长度--此处为TSON的头部
        /// </summary>
        public int dataLen;

        /// <summary>
        ///     参数数据类型
        /// </summary>
        public int dataTypeId;

        /// <summary>
        ///     数据
        /// </summary>
        public ITson Data { get; set; }

        #endregion

        #region 本地状态数据

        /// <summary>
        ///     命令消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     其它数据
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        ///     其它数据
        /// </summary>
        public object Tag1 { get; set; }

        /// <summary>
        ///     其它数据
        /// </summary>
        public object Tag2 { get; set; }

        /// <summary>
        ///     结束回调
        /// </summary>
        public Action<CommandArgument> OnEnd { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime TimeSpamp { get; set; }

        #endregion

        #region 事件反馈

        /// <summary>
        ///     请求状态变化
        /// </summary>
        public event CommandHandler RequestStateChanged;

        /// <summary>
        ///     发出状态修改事件
        /// </summary>
        public void OnRequestStateChanged(CommandArgument argument)
        {
            RequestStateChanged?.Invoke(this, argument);
        }

        #endregion
    }

}