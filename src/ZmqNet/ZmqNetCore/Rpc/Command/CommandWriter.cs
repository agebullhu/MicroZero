using System.Diagnostics;
using System.Text;
using Agebull.Common.DataModel;
using Gboxt.Common.DataModel;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 命令序列化器
    /// </summary>
    public class CommandWriter : TsonSerializer
    {
        /// <summary>
        /// 命令
        /// </summary>
        public CommandArgument Command { get; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="arg"></param>
        public CommandWriter(CommandArgument arg)
        {
            Command = arg;
        }

        /// <summary>
        /// 命令对象写入字节
        /// </summary>
        public void WriteCommandToBuffer()
        {
            m_buffer_len = RpcEnvironment.NETCOMMAND_LEN;
            if (Command.Data != null)
            {
                m_buffer_len += Command.Data.SafeBufferLength;
            }
            m_bufer = new byte[m_buffer_len];

            m_postion = 0;
            if (Command.cmdId <= RpcEnvironment.NET_COMMAND_BUSINESS_NOTIFY)
            {
                string token = $"{(Command.cmdId == RpcEnvironment.NET_COMMAND_BUSINESS_NOTIFY ? 'B' : 'S')}:{Command.commandName}";

                var bytes = Encoding.ASCII.GetBytes(token);
                for (int i = 0; i < bytes.Length && i < RpcEnvironment.GUID_LEN; i++)
                {
                    m_bufer[m_postion++] = bytes[i];
                }
                m_postion = RpcEnvironment.GUID_LEN;
                bytes = Encoding.ASCII.GetBytes(Command.userToken);
                for (int i = 0; i < bytes.Length && i < RpcEnvironment.GUID_LEN; i++)
                {
                    m_bufer[m_postion++] = bytes[i];
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Command.userToken))
                {
                    var bytes = Encoding.ASCII.GetBytes(Command.userToken);
                    for (int i = 0; i < bytes.Length && i < RpcEnvironment.GUID_LEN; i++)
                    {
                        m_bufer[m_postion++] = bytes[i];
                    }
                }
                m_postion = RpcEnvironment.GUID_LEN;
                {
                    var bytes = Encoding.ASCII.GetBytes(Command.commandInfo);
                    for (int i = 0; i < bytes.Length && i < RpcEnvironment.GUID_LEN; i++)
                    {
                        m_bufer[m_postion++] = bytes[i];
                    }
                }
            }
            m_postion = RpcEnvironment.GUID_LEN << 1;
            Write(Command.cmdId);
            Write(Command.tryNum);
            Write(Command.cmdState);
            uint crc = CrcHelper.Crc(m_bufer, RpcEnvironment.NETCOMMAND_BODY_LEN);
            Write(crc);
            if (Command.Data == null)
            {
                Command.dataLen = 0;
                return;
            }
            TsonSerializer serializer = new TsonSerializer(m_bufer, m_buffer_len, RpcEnvironment.NETCOMMAND_HEAD_LEN);
            Command.Data.Serialize(serializer);
            Command.dataLen = serializer.DataLen;
            m_end_postion = serializer.EndPostion;
            m_data_len = m_end_postion;
        }
    }
}
