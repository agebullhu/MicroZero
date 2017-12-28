namespace Agebull.Common.DataModel
{
    /// <summary>
    /// Tson基类
    /// </summary>
    public class TsonBase
    {
        /// <summary>
        /// 开始标记
        /// </summary>
        public const byte BofCode = 0x0;
        /// <summary>
        /// 尾部标记
        /// </summary>
        public const byte EofMark = 0xFF;
        /// <summary>
        /// 结束标记
        /// </summary>
        public const byte EndMark = 0xFF;
        /// <summary>
        /// 缓存
        /// </summary>
        protected byte[] m_bufer;
        /// <summary>
        /// 当前位置
        /// </summary>
        protected int m_postion;
        /// <summary>
        /// 缓存长度
        /// </summary>
        protected int m_buffer_len;
        /// <summary>
        /// 是否全量写入
        /// </summary>
        protected bool m_data_full;
        /// <summary>
        /// 起始位
        /// </summary>
        protected int m_start_postion;

        /// <summary>
        /// 结束位(最后一位的下一位)
        /// </summary>
        protected int m_end_postion;

        /// <summary>
        /// 实际长度
        /// </summary>
        protected int m_data_len;

        /// <summary>
        /// 默认构造
        /// </summary>
        protected TsonBase()
        {
            m_data_full = true;
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="buffer">缓存</param>
        /// <param name="len">缓存长度</param>
        /// <param name="firstPostion">起始位置</param>
        protected TsonBase(byte[] buffer, int len, int firstPostion)
        {
            m_bufer = buffer;
            m_buffer_len = len;
            m_start_postion = firstPostion;
            m_end_postion = firstPostion;
            m_data_full = true;
        }
        /// <summary>
        /// 重置位置
        /// </summary>
        public void Reset()
        {
            m_postion = m_start_postion;
        }

        /// <summary>
        /// 全量写入
        /// </summary>
        public bool IsFull
        {
            get { return m_data_full; }
            set { m_data_full = value; }
        }

        /// <summary>
        /// 数据长度
        /// </summary>
        public int DataLen {get { return m_data_len; } }

        /// <summary>
        /// 缓冲区长度
        /// </summary>
        public int BufferLen { get { return m_buffer_len; } } 

        /// <summary>
        /// 缓冲区
        /// </summary>
        public byte[] Buffer { get { return m_bufer; } } 

        /// <summary>
        /// 结束位置
        /// </summary>
        public int EndPostion { get { return m_end_postion; } }
        /// <summary>
        /// 起始位置
        /// </summary>
        public int BeginPostion {get { return m_start_postion; } }
        /// <summary>
        /// 起始位置
        /// </summary>
        public int Postion
        {
            get { return m_postion; }
            set { m_postion = value; }
        }

    }
}
