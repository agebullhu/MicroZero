using Agebull.Common.DataModel;
using NetMQ;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 命令反序列化器
    /// </summary>
    public class CommandReader : TsonDeserializer
    {
        /// <summary>
        /// 命令
        /// </summary>
        public CommandArgument Command { get; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="zMsg"></param>
        public CommandReader(NetMQFrame zMsg)
                    : base(zMsg.Buffer, zMsg.BufferSize)
        {
            Command = new CommandArgument();
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="bytes"></param>
        public CommandReader(byte[] bytes)
            : base(bytes, bytes.Length)
        {
            Command = new CommandArgument();
        }
        /// <summary>
        /// 字节解析为命令对象
        /// </summary>
        public void ReadCommandFromBuffer()
        {
            string h1 = System.Text.Encoding.ASCII.GetString(m_bufer, 0, RpcEnvironment.GUID_LEN).TrimEnd('\0');
            string h2 = System.Text.Encoding.ASCII.GetString(m_bufer, RpcEnvironment.GUID_LEN, RpcEnvironment.GUID_LEN).TrimEnd('\0');

            m_postion = RpcEnvironment.GUID_LEN << 1;
            Read(ref Command.cmdId);
            if (Command.cmdId > RpcEnvironment.NET_COMMAND_BUSINESS_NOTIFY)
            {
                Command.userToken = h1;
                Command.commandInfo = h2;
            }
            else
            {
                Command.userToken = h2;
                Command.commandName = h1.Split(':')[1];
            }
            Read(ref Command.tryNum);
            Read(ref Command.cmdState);
            Read(ref Command.crcCode);
            if (Command.crcCode != CrcHelper.Crc(m_bufer, RpcEnvironment.NETCOMMAND_BODY_LEN))
            {
                m_error = true;
                Command.cmdState = RpcEnvironment.NET_COMMAND_STATE_CRC_ERROR;
                return;
            }
            Read(ref Command.dataLen);
            if (Command.dataLen == 0)
                return;
            m_start_postion = RpcEnvironment.NETCOMMAND_HEAD_LEN;
            Begin();
            Command.dataTypeId = this.m_type_id;
            Command.Data = TsonTypeRegister.CreateType(Command.dataTypeId);
            if (Command.Data == null)
            {
                m_error = true;
                Command.cmdState = RpcEnvironment.NET_COMMAND_STATE_UNKNOW_DATA;
                return;
            }
            Command.Data.Deserialize(this);
        }
    }
}
