namespace Agebull.MicroZero.Helpers
{
    /// <summary>
    /// 运行时警告
    /// </summary>
    public interface IRuntimeWaring
    {
        /// <summary>
        /// 运行时警告
        /// </summary>
        /// <param name="host"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        void Waring(string host, string api, string message);
    }
}