using System;
using System.Collections.Generic;

namespace MicroZero.Http.Route
{
    /// <summary>
    /// 运行时警告节点
    /// </summary>
    public class RuntimeWaringItem
    {
        /*起止时间*/
        public DateTime StartTime, LastTime;
        /// <summary>
        /// 最后一次发短信时间
        /// </summary>
        public DateTime MessageTime;
        /// <summary>
        /// 发生次数，发送次数，发送后发生次数
        /// </summary>
        public int WaringCount, SendCount, LastCount;
        /// <summary>
        /// 发生问题的API
        /// </summary>
        public Dictionary<string, List<string>> Apis = new Dictionary<string, List<string>>();
    }
}