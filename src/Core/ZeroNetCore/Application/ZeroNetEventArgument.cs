using System;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点事件参数
    /// </summary>
    public class ZeroNetEventArgument : EventArgs
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="centerEvent"></param>
        /// <param name="name"></param>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public ZeroNetEventArgument(ZeroNetEventType centerEvent, string name, string context, StationConfig config)
        {
            EventConfig = config;
            Event = centerEvent;
            Context = context;
            Name = name;
        }

        /// <summary>
        /// 站点名称
        /// </summary>
        public readonly ZeroNetEventType Event;

        /// <summary>
        /// 内容
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 内容
        /// </summary>
        public readonly string Context;

        /// <summary>
        /// 配置
        /// </summary>
        public readonly StationConfig EventConfig;
    }
}