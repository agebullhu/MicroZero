using Agebull.MicroZero.ZeroApis;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// ApiResult的虚拟化
    /// </summary>
    public interface IApiResultDefault
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        IApiResult DeserializeObject(string json);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        IApiResult<T> DeserializeObject<T>(string json);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        IApiResult Error(int errCode);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe);

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult<TData> Succees<TData>(TData data);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe);

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult Error();

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult<TData> Error<TData>();

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult Succees();

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult<TData> Succees<TData>();

        /// <summary>成功</summary>
        /// <remarks>成功</remarks>
        IApiResult Ok { get; }

        /// <summary>页面不存在</summary>
        IApiResult NoFind { get; }

        /// <summary>不支持的操作</summary>
        IApiResult NotSupport { get; }

        /// <summary>参数错误字符串</summary>
        IApiResult ArgumentError { get; }

        /// <summary>逻辑错误字符串</summary>
        IApiResult LogicalError { get; }

        /// <summary>拒绝访问</summary>
        IApiResult DenyAccess { get; }

        /// <summary>服务器无返回值的字符串</summary>
        IApiResult RemoteEmptyError { get; }

        /// <summary>服务器访问异常</summary>
        IApiResult NetworkError { get; }

        /// <summary>本地错误</summary>
        IApiResult LocalError { get; }

        /// <summary>本地访问异常</summary>
        IApiResult LocalException { get; }

        /// <summary>系统未就绪</summary>
        IApiResult NoReady { get; }

        /// <summary>暂停服务</summary>
        IApiResult Pause { get; }

        /// <summary>未知错误</summary>
        IApiResult UnknowError { get; }

        /// <summary>网络超时</summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        IApiResult TimeOut { get; }

        /// <summary>内部错误</summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        IApiResult InnerError { get; }

        /// <summary>服务不可用</summary>
        IApiResult Unavailable { get; }

    }
}