using System;

namespace Agebull.Common.DataModel
{
    /// <summary>
    /// 事件参数
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    public class EventArgs<TArgument> : EventArgs
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="arg"></param>
        public EventArgs(TArgument arg)
        {
            Argument = arg;
        }
        /// <summary>
        /// 参数
        /// </summary>
        public TArgument Argument { get; }
    }
}