using Agebull.Common.Ioc;
using Newtonsoft.Json;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>API返回基类</summary>
    public static class ApiResultIoc
    {
        /// <summary>
        /// ApiResult的抽象
        /// </summary>
        private static IApiResultDefault _ioc;

        /// <summary>
        /// ApiResult的抽象
        /// </summary>
        public static IApiResultDefault Ioc =>
            _ioc ?? (_ioc = IocHelper.Create<IApiResultDefault>() ?? new ApiResultDefault());
        


        /// <summary>成功的Json字符串</summary>
        /// <remarks>成功</remarks>
        public static string SucceesJson => JsonHelper.SerializeObject(Ioc.Ok);

        /// <summary>页面不存在的Json字符串</summary>
        public static string NoFindJson => JsonHelper.SerializeObject(Ioc.NoFind);

        /// <summary>系统不支持的Json字符串</summary>
        public static string NotSupportJson => JsonHelper.SerializeObject(Ioc.NotSupport);

        /// <summary>参数错误字符串</summary>
        public static string ArgumentErrorJson => JsonHelper.SerializeObject(Ioc.ArgumentError);

        /// <summary>逻辑错误字符串</summary>
        public static string LogicalErrorJson => JsonHelper.SerializeObject(Ioc.LogicalError);

        /// <summary>拒绝访问的Json字符串</summary>
        public static string DenyAccessJson => JsonHelper.SerializeObject(Ioc.DenyAccess);

        /// <summary>服务器无返回值的字符串</summary>
        public static string RemoteEmptyErrorJson => JsonHelper.SerializeObject(Ioc.RemoteEmptyError);

        /// <summary>服务器访问异常</summary>
        public static string NetworkErrorJson => JsonHelper.SerializeObject(Ioc.NetworkError);

        /// <summary>本地错误</summary>
        public static string LocalErrorJson => JsonHelper.SerializeObject(Ioc.LocalError);

        /// <summary>本地访问异常的Json字符串</summary>
        public static string LocalExceptionJson => JsonHelper.SerializeObject(Ioc.LocalException);

        /// <summary>系统未就绪的Json字符串</summary>
        public static string NoReadyJson => JsonHelper.SerializeObject(Ioc.NoReady);

        /// <summary>暂停服务的Json字符串</summary>
        public static string PauseJson => JsonHelper.SerializeObject(Ioc.Pause);

        /// <summary>未知错误的Json字符串</summary>
        public static string UnknowErrorJson => JsonHelper.SerializeObject(Ioc.UnknowError);

        /// <summary>网络超时的Json字符串</summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static string TimeOutJson => JsonHelper.SerializeObject(Ioc.NetTimeOut);

        /// <summary>执行超时</summary>
        /// <remarks>Api执行超时</remarks>
        public static string ExecTimeOut => JsonHelper.SerializeObject(Ioc.ExecTimeOut);
        
        /// <summary>内部错误的Json字符串</summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static string InnerErrorJson => JsonHelper.SerializeObject(Ioc.InnerError);

        /// <summary>服务不可用的Json字符串</summary>
        public static string UnavailableJson => JsonHelper.SerializeObject(Ioc.Unavailable);
    }
}