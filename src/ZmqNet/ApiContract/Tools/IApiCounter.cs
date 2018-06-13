namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public interface IApiCounter
    {
        /// <summary>
        /// 设置Api调用注入
        /// </summary>
        void HookApi();

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnable { get; }

        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="data"></param>
        void Count(CountData data);
    }
}