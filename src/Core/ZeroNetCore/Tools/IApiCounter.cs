namespace Agebull.MicroZero.ZeroApi
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public interface IApiCounter
    {
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