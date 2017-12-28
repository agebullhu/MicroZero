using System;
using Agebull.Zmq.Rpc;

namespace Agebull.Common.DataModel
{
    /// <summary>
    /// TSON序列化器
    /// </summary>
    public class TsonSerializer : TsonBase
    {
        #region 基本


        /// <summary>
        /// 构造
        /// </summary>
        public TsonSerializer()
        {
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="len"></param>
        /// <param name="firstPostion"></param>
        public TsonSerializer(byte[] buffer, int len, int firstPostion = 0)
            : base(buffer, len, firstPostion)
        {
            m_end_postion = firstPostion;
        }
        /// <summary>
        /// 初始化缓冲区
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public bool CreateBuffer(int len)
        {
            if (len <= RpcEnvironment.SERIALIZE_BASE_LEN)
                return false;
            m_buffer_len = len + 32;
            m_bufer = new byte[m_buffer_len];
            m_start_postion = 0;
            m_end_postion = 0;
            return true;
        }
        /// <summary>
        /// 开始写入
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ver"></param>
        public void Begin(int type, byte ver)
        {
            m_postion = BeginPostion + 4;//预留写入总长度
            Write(type);//数据类型
            Write(ver);//数据版本
            Write(m_data_full);//数据是否增量
        }
        /// <summary>
        /// 完成写入
        /// </summary>
        public void End()
        {
            //对齐
            var mod = m_postion % 4;
            for (int i = 0; i < 3 - mod; i++)
                m_bufer[m_postion++] = EofMark;

            m_bufer[m_postion++] = EndMark;

            m_end_postion = m_postion;
            m_data_len = m_end_postion - m_start_postion;
            m_postion = BeginPostion;
            Write(DataLen);
            m_postion = BeginPostion;
        }
        /// <summary>
        /// 写入字段索引号
        /// </summary>
        /// <param name="index"></param>
        public void WriteIndex(byte index)
        {
            m_bufer[m_postion++] = index;
        }

        #endregion

        /// <summary>
        /// 空值检测
        /// </summary>
        /// <param name="str"></param>
        public bool IsEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }
        /// <summary>
        /// 空值检测
        /// </summary>
        /// <param name="array"></param>
        public bool IsEmpty<T>(T[] array) where T : struct
        {
            return array == null || array.Length == 0;
        }
        /// <summary>
        /// 空值检测
        /// </summary>
        /// <param name="t"></param>
        public bool IsEmpty<T>(T t)
        {
            return Equals(t, default(T));
        }
        /// <summary>
        /// 空值检测
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsEmpty(DateTime time)
        {
            return time.Year <= 1970;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="str"></param>
        public void Write(string str)
        {
            var bytes = str.ToUtf8Bytes();
            Write((ushort)bytes.Length);
            
            foreach (var item in bytes)
                m_bufer[m_postion++] = item;
        }
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="b"></param>
        public void Write(bool b)
        {
            //WriteType(OBJ_TYPE_BOOLEN);
            if (b)
                m_bufer[m_postion++] = 1;
            else
                m_bufer[m_postion++] = 0;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="chr"></param>
        public void Write(byte chr)
        {
            //WriteType(OBJ_TYPE_CHAR);
            m_bufer[m_postion++] = chr;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="chr"></param>
        public void Write(sbyte chr)
        {
            //WriteType(OBJ_TYPE_BYTE);
            m_bufer[m_postion++] = (byte)(chr);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Write(short number)
        {
            byte* bf = (byte*)&number;
            //低位在前,高位在后
            m_bufer[m_postion++] = bf[0];
            m_bufer[m_postion++] = bf[1];
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Write(ushort number)
        {
            byte* bf = (byte*)&number;
            //低位在前,高位在后
            m_bufer[m_postion++] = bf[0];
            m_bufer[m_postion++] = bf[1];
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Write(int number)
        {
            byte* bf = (byte*)&number;
            //低位在前,高位在后
            m_bufer[m_postion++] = bf[0];
            m_bufer[m_postion++] = bf[1];
            m_bufer[m_postion++] = bf[2];
            m_bufer[m_postion++] = bf[3];
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Write(uint number)
        {
            byte* bf = (byte*)&number;
            //低位在前,高位在后
            m_bufer[m_postion++] = bf[0];
            m_bufer[m_postion++] = bf[1];
            m_bufer[m_postion++] = bf[2];
            m_bufer[m_postion++] = bf[3];
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Write(long number)
        {
            byte* bf = (byte*)&number;
            //低位在前,高位在后
            m_bufer[m_postion++] = bf[0];
            m_bufer[m_postion++] = bf[1];
            m_bufer[m_postion++] = bf[2];
            m_bufer[m_postion++] = bf[3];
            m_bufer[m_postion++] = bf[4];
            m_bufer[m_postion++] = bf[5];
            m_bufer[m_postion++] = bf[6];
            m_bufer[m_postion++] = bf[7];
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Write(ulong number)
        {
            byte* bf = (byte*)&number;
            //低位在前,高位在后
            m_bufer[m_postion++] = bf[0];
            m_bufer[m_postion++] = bf[1];
            m_bufer[m_postion++] = bf[2];
            m_bufer[m_postion++] = bf[3];
            m_bufer[m_postion++] = bf[4];
            m_bufer[m_postion++] = bf[5];
            m_bufer[m_postion++] = bf[6];
            m_bufer[m_postion++] = bf[7];
        }


        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Write(float number)
        {
            byte* bf = (byte*)&number;
            //低位在前,高位在后
            m_bufer[m_postion++] = bf[0];
            m_bufer[m_postion++] = bf[1];
            m_bufer[m_postion++] = bf[2];
            m_bufer[m_postion++] = bf[3];
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public unsafe void Write(double number)
        {
            byte* bf = (byte*)&number;
            //低位在前,高位在后
            m_bufer[m_postion++] = bf[0];
            m_bufer[m_postion++] = bf[1];
            m_bufer[m_postion++] = bf[2];
            m_bufer[m_postion++] = bf[3];
            m_bufer[m_postion++] = bf[4];
            m_bufer[m_postion++] = bf[5];
            m_bufer[m_postion++] = bf[6];
            m_bufer[m_postion++] = bf[7];
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="number"></param>
        public void Write(decimal number)
        {
            Write((long)(number * 1000000));
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="date"></param>
        public void Write(DateTime date)
        {
            var date2 = new DateTime(1970, 0, 0);
            var sp = date - date2;
            Write((long)sp.Seconds);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(DateTime[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(bool[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(byte[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(sbyte[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(short[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(ushort[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(int[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(uint[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(long[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(ulong[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }


        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(float[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(double[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="array"></param>
        public void Write(decimal[] array)
        {
            Write((ushort)array.Length);
            foreach (var item in array)
                Write(item);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="tson"></param>
        public void Write(ITson tson)
        {
            //保存环境
            int start = m_start_postion;
            //序列化
            m_start_postion = m_postion;
            tson.Serialize(this);
            //还原环境
            m_start_postion = start;
            m_postion = m_end_postion;
        }
    }
}
