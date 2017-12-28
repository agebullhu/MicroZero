namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 表示一个远程调用命令处理器
    /// </summary>
    public interface IEventWorker
    {
        /// <summary>
        /// 处理器名称
        /// </summary>
        string WorkerName { get; }

        /// <summary>
        /// 事件处理器
        /// </summary>
        /// <param name="eventName">当前完整事件名称</param>
        /// <param name="argument">事件参数</param>
        void Process(string eventName, RpcArgument argument);

        /// <summary>
        /// JSON文本反序列化为参数
        /// </summary>
        /// <param name="argument">事件名称</param>
        /// <param name="arg">JSON文本</param>
        /// <returns>参数</returns>
        RpcArgument ToArgument(CommandArgument argument, string arg);
    }
}