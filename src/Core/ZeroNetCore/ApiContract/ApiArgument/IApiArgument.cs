namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 表示API请求参数
    /// </summary>
    public interface IApiArgument
    {
        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="message">返回的消息</param>
        /// <returns>成功则返回真</returns>
        bool Validate(out string message);
    }
}
