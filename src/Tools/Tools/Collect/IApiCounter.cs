namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public interface IApiCounter
    {
        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="data"></param>
        void Count(CountData data);
    }
}