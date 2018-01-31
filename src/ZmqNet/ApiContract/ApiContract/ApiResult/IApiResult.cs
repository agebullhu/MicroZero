namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// API返回基类
    /// </summary>
    public interface IApiResult
    {
        /// <summary>
        /// 成功或失败标记
        /// </summary>
        bool Result { get; set; }

        /// <summary>
        /// API执行状态（为空表示状态正常）
        /// </summary>
        IApiStatusResult Status { get; }
    }
    /// <summary>
    /// API返回基类
    /// </summary>
    public interface IApiResult<TData> : IApiResult
        where TData : IApiResultData
    {
        /// <summary>
        /// 返回值
        /// </summary>
        TData ResultData { get; set; }
    }
}