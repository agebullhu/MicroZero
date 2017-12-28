using System;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 事件处理器容器
    /// </summary>
    public class EventWorkerContainer
    {
        /// <summary>
        /// 订阅的消息类型名称
        /// </summary>
        internal static string SubName { get; private set; }

        /// <summary>
        /// 生成处理器
        /// </summary>
        internal static Func<IEventWorker> CreateWorker { get; private set; }

        /// <summary>
        /// 注册处理器
        /// </summary>
        /// <typeparam name="TWorker"></typeparam>
        public static void Regist<TWorker>(string subName) where TWorker : IEventWorker, new()
        {
            SubName = subName;
            CreateWorker = () => new TWorker();
        }
    }
}