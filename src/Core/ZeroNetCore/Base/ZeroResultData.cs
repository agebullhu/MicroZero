using System;
using System.Linq;
using System.Text;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 返回值
    /// </summary>
    public class ZeroResultData : ZeroNetMessage
    {
        /// <summary>
        /// 与服务器网络交互是否正常（特别注意：不是指逻辑操作是否成功,逻辑成功看InteractiveSuccess=true时的State）
        /// </summary>
        public bool InteractiveSuccess { get; set; }

        /// <summary>
        /// ZMQ错误码
        /// </summary>
        public ZError ZmqError;

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception;

        /// <summary>
        /// 消息
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// 显示到文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append($"{(InteractiveSuccess ? "网络请求成功" : "网络请求失败")} , 状态:{State}({State.Text()})");
            if (Exception != null)
            {
                text.Append($" , 异常：{Exception.Message}");
            }
            if (ZmqError != null)
            {
                text.Append($" , ZmqError:{ZmqError}");
            }

            if (_frames == null || Frames.Count <= 0) return text.ToString();
            foreach (var data in Frames.Values)
            {
                text.Append($" , [{ZeroFrameType.FrameName(data.Type)}] {data.Data}");
            }
            return text.ToString();
        }

        /// <summary>
        /// 命令解包
        /// </summary>
        /// <param name="showError"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static TZeroResultData Unpack<TZeroResultData>(ZMessage frames, bool showError)
            where TZeroResultData : ZeroResultData, new()
        {
            try
            {
                byte[][] messages;
                using (frames)
                {
                    messages = frames.Select(p => p.ReadAll()).ToArray();
                }
                if (!Unpack(true, messages, out TZeroResultData result, null))
                {
                    if (showError)
                        ZeroTrace.WriteError("Unpack", "LaoutError");
                }
                result.InteractiveSuccess = true;
                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Unpack", e);
                return new TZeroResultData
                {
                    State = ZeroOperatorStateType.LocalRecvError,
                    Exception = e
                };
            }
        }
    }


    /// <summary>
    /// 返回值
    /// </summary>
    public class ZeroResult : ZeroResultData
    {
        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">返回值</param>
        /// <returns>是否存在</returns>
        public bool TryGetValue(byte name, out string value)
        {
            return TryGetValue(name, out value, b => Encoding.UTF8.GetString(b));
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>存在返回值，不存在返回空对象</returns>
        public string GetValue(byte name)
        {
            return GetValue(name, b => Encoding.UTF8.GetString(b));
        }
    }
}