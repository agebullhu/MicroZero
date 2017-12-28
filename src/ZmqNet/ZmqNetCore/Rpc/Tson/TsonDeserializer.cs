using System;
using System.Diagnostics;
using System.Text;

namespace Agebull.Common.DataModel
{
    /// <summary>
    /// TSON反序列化器
    /// </summary>
    public class TsonDeserializer : TsonBase
    {
        #region 状态数据

        /// <summary>
        /// 是否有错
        /// </summary>
        protected bool m_error;
        /// <summary>
        /// 数据类型ID
        /// </summary>
        protected int m_type_id;
        /// <summary>
        /// 数据版本
        /// </summary>
        protected byte m_data_ver;

        /// <summary>
        /// 是否有错
        /// </summary>
        public bool IsError {get { return m_error; } }
        /// <summary>
        /// 数据类型ID
        /// </summary>
        protected int DataTypeId {get { return m_type_id; } }
        /// <summary>
        /// 数据版本
        /// </summary>
        protected byte DataVersion {get { return m_data_ver; } }

        /// <summary>
        /// 是否处于尾部
        /// </summary>
        public bool IsEof {get { return m_data_len > 8192 || m_postion >= EndPostion || m_bufer[m_postion] == EofMark; } }

        /// <summary>
        /// 是否处于头部
        /// </summary>
        public bool IsBof {get {return m_postion == m_start_postion; } }

        #endregion
        #region 构造
        /// <summary>
        /// 构造
        /// </summary>
        public TsonDeserializer()
            : base()
        {
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="buffer">缓存</param>
        /// <param name="len">缓存长度</param>
        /// <param name="firstPostion">起始位置</param>
        public TsonDeserializer(byte[] buffer, int len, int firstPostion = 0)
            : base(buffer, len, firstPostion)
        {
        }
        #endregion

        #region 流程支持

        /// <summary>
        /// 开始反序列化
        /// </summary>
        public void Begin()
        {
            m_error = false;
            m_postion = base.m_start_postion;
            Read(ref m_data_len);
            Read(ref m_type_id);
            Read(ref m_data_ver);
            Read(ref m_data_full);
            m_end_postion = m_start_postion + m_data_len;
        }
        /// <summary>
        /// 结束反序列化
        /// </summary>
        public void End()
        {
            m_postion = EndPostion;
        }
        /// <summary>
        /// 读当前属性的数字索引
        /// </summary>
        /// <returns></returns>
        public int ReadIndex()
        {
            return m_bufer[m_postion++];
        }

        #endregion

        #region 单个读取
        /// <summary>
        /// 读取文本
        /// </summary>
        /// <param name="str"></param>
        public void Read(ref string str)
        {
            ushort len = 0;
            try
            {
                Read(ref len);
                var bytes = new byte[len];
                for (int i = 0; i < len; i++)
                {
                    bytes[i] = m_bufer[m_postion + i];
                }
                str = bytes.FromUtf8Bytes();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, GetType().Name + "Read");
            }
            m_postion += len;
        }
        /// <summary>
        /// 读取布尔值
        /// </summary>
        /// <param name="b"></param>
        public void Read(ref bool b)
        {
            //ReadType(OBJ_TYPE_BOOLEN);
            b = m_bufer[m_postion++] == 1;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="chr"></param>
        public void Read(ref byte chr)
        {
            //ReadType(OBJ_TYPE_CHAR);
            chr = m_bufer[m_postion++];
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="chr"></param>
        public void Read(ref sbyte chr)
        {
            //ReadType(OBJ_TYPE_BYTE);
            chr = (sbyte)(m_bufer[m_postion++]);
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Read(ref short number)
        {
            fixed (short* ptr = &number)
            {
                byte* bf = (byte*)ptr;
                //低位在前,高位在后
                bf[0] = m_bufer[m_postion++];
                bf[1] = m_bufer[m_postion++];
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Read(ref ushort number)
        {
            fixed (ushort* ptr = &number)
            {
                byte* bf = (byte*)ptr;
                //低位在前,高位在后
                bf[0] = m_bufer[m_postion++];
                bf[1] = m_bufer[m_postion++];
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Read(ref int number)
        {
            fixed (int* ptr = &number)
            {
                byte* bf = (byte*)ptr;
                //低位在前,高位在后
                bf[0] = m_bufer[m_postion++];
                bf[1] = m_bufer[m_postion++];
                bf[2] = m_bufer[m_postion++];
                bf[3] = m_bufer[m_postion++];
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Read(ref uint number)
        {
            fixed (uint* ptr = &number)
            {
                byte* bf = (byte*)ptr;
                //低位在前,高位在后
                bf[0] = m_bufer[m_postion++];
                bf[1] = m_bufer[m_postion++];
                bf[2] = m_bufer[m_postion++];
                bf[3] = m_bufer[m_postion++];
            }
        }
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Read(ref long number)
        {
            fixed (long* ptr = &number)
            {
                byte* bf = (byte*)ptr;
                //低位在前,高位在后
                bf[0] = m_bufer[m_postion++];
                bf[1] = m_bufer[m_postion++];
                bf[2] = m_bufer[m_postion++];
                bf[3] = m_bufer[m_postion++];
                bf[4] = m_bufer[m_postion++];
                bf[5] = m_bufer[m_postion++];
                bf[6] = m_bufer[m_postion++];
                bf[7] = m_bufer[m_postion++];
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Read(ref ulong number)
        {
            fixed (ulong* ptr = &number)
            {
                byte* bf = (byte*)ptr;
                //低位在前,高位在后
                bf[0] = m_bufer[m_postion++];
                bf[1] = m_bufer[m_postion++];
                bf[2] = m_bufer[m_postion++];
                bf[3] = m_bufer[m_postion++];
                bf[4] = m_bufer[m_postion++];
                bf[5] = m_bufer[m_postion++];
                bf[6] = m_bufer[m_postion++];
                bf[7] = m_bufer[m_postion++];
            }
        }


        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Read(ref float number)
        {
            fixed (float* ptr = &number)
            {
                byte* bf = (byte*)ptr;
                //低位在前,高位在后
                bf[0] = m_bufer[m_postion++];
                bf[1] = m_bufer[m_postion++];
                bf[2] = m_bufer[m_postion++];
                bf[3] = m_bufer[m_postion++];
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Read(ref double number)
        {
            fixed (double* ptr = &number)
            {
                byte* bf = (byte*)ptr;
                //低位在前,高位在后
                bf[0] = m_bufer[m_postion++];
                bf[1] = m_bufer[m_postion++];
                bf[2] = m_bufer[m_postion++];
                bf[3] = m_bufer[m_postion++];
                bf[4] = m_bufer[m_postion++];
                bf[5] = m_bufer[m_postion++];
                bf[6] = m_bufer[m_postion++];
                bf[7] = m_bufer[m_postion++];
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="number"></param>
        public void Read(ref decimal number)
        {
            long l = 0;
            Read(ref l);
            number = l / 1000000M;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="date"></param>
        public void Read(ref DateTime date)
        {
            var date2 = new DateTime(1970, 1, 1, 8, 0, 0);
            long vl = 0;
            Read(ref vl);
            date = date2.AddSeconds(vl);
        }

        /// <summary>
        /// 读取
        /// </summary>
        public TTson Read<TTson>() where TTson : class, ITson, new()
        {
            //保存环境
            int start = m_start_postion;
            //更换参数做反序列化
            m_start_postion = m_postion;
            var data = new TTson();
            data.Deserialize(this);
            //还原环境做后续的序列化
            m_start_postion = start;
            m_postion = m_end_postion;
            return data;
        }
        #endregion
        #region 数组

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref DateTime[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new DateTime[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref bool[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new bool[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref byte[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new byte[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref sbyte[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new sbyte[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref short[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new short[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref ushort[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new ushort[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref int[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new int[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref uint[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new uint[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref long[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new long[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref ulong[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new ulong[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref float[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new float[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref double[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new double[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="array"></param>
        public void Read(ref decimal[] array)
        {
            ushort len = 0;
            Read(ref len);
            array = new decimal[len];
            for (int i = 0; i < len; i++)
            {
                Read(ref array[i]);
            }
        }
        #endregion

    }
}
