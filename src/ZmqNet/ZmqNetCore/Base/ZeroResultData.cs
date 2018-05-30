using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.Common.DataModel;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 返回值
    /// </summary>
    public class ZeroResultData<TValue>
    {
        /// <summary>
        /// 与服务器网络交互是否正常（特别注意：不是指逻辑操作是否成功,逻辑成功看InteractiveSuccess=true时的State）
        /// </summary>
        public bool InteractiveSuccess;

        /// <summary>
        /// 逻辑操作状态
        /// </summary>
        public ZeroOperatorStateType State;

        /// <summary>
        /// ZMQ错误码
        /// </summary>
        public int ZmqErrorCode;

        /// <summary>
        /// 逻辑操作状态
        /// </summary>
        public string ZmqErrorMessage;

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception;

        /// <summary>
        /// 返回数据
        /// </summary>
        public readonly List<NameValue<byte, TValue>> Datas = new List<NameValue<byte, TValue>>();

        /// <summary>
        /// 加入数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(byte name, TValue value)
        {
            Datas.Add(new NameValue<byte, TValue>
            {
                name = name,
                value = value
            });
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">返回值</param>
        /// <returns>是否存在</returns>
        public bool TryGetValue(byte name, out TValue value)
        {
            if (Datas.Count == 0)
            {
                value = default(TValue);
                return false;
            }
            var vl = Datas.FirstOrDefault(p => p.name == name);
            value = vl == null ? default(TValue) : vl.value;
            return vl != null;
        }
        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>存在返回值，不存在返回空对象</returns>
        public TValue GetValue(byte name)
        {
            if (Datas.Count == 0)
                return default(TValue);
            var vl = Datas.FirstOrDefault(p => p.name == name);
            return vl == null ? default(TValue) : vl.value;
        }

        /// <summary>
        /// 显示到文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Exception != null)
            {
                return $"发生异常：{Exception.Message}";
            }

            if (TryGetValue(ZeroFrameType.TextValue, out var value))
            {
                return InteractiveSuccess ? $"处理成功：{value}" : $"处理失败：{value}";
            }
            return InteractiveSuccess ? $"处理成功：{State.Text()}" : $"处理失败：{State.Text()}";
        }
    }
}