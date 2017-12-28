#if !NETSTANDARD2_0
using System;
using System.Globalization;
using System.Net.Http.Formatting;
using Agebull.Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yizuan2.Service.Api.WebApi;
using System.Web.Http;
using System.Web.Mvc;

namespace Yizuan.Service.Api.WebApi
{
    /// <summary>
    /// WebApi应用辅助类
    /// </summary>
    public static class ApplicationHelper
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialize
        {
            get;
            private set;
        }
        /// <summary>
        /// 初始化，必须先调用
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialize)
                return;
            IsInitialize = true;

            // 调用ID的取得
            LogRecorder.GetRequestIdFunc = () => ApiContext.RequestContext?.RequestId ?? Guid.NewGuid();
            // 跨域支持
            HttpHandler.Handlers.Add(new CorsHandler());
            // 日志支持
            HttpHandler.Handlers.Add(new HttpIoLogHandler());
            // 身份验证上下文校验与处理
            HttpHandler.Handlers.Add(new BearerHandler());
        }
        /// <summary>
        /// 初始化，必须先调用
        /// </summary>
        public static void InitializeNoBearer()
        {
            if (IsInitialize)
                return;
            IsInitialize = true;

            // 调用ID的取得
            LogRecorder.GetRequestIdFunc = () => ApiContext.RequestContext?.RequestId ?? Guid.NewGuid();
            // 跨域支持
            HttpHandler.Handlers.Add(new CorsHandler());
            // 日志支持
            HttpHandler.Handlers.Add(new HttpIoLogHandler());
            // 身份验证上下文校验与处理
            //HttpHandler.Handlers.Add(new BearerHandler());
        }
        /// <summary>
        /// 注册系统处理器
        /// </summary>
        /// <param name="handler"></param>
        public static void RegistSystemHandler(IHttpSystemHandler handler)
        {
            if (!IsInitialize)
                Initialize();
            if (handler != null && !HttpHandler.Handlers.Contains(handler))
                HttpHandler.Handlers.Add(handler);
        }

        /// <summary>
        /// Application_Start时调用
        /// </summary>
        public static void OnApplicationStart()
        {
            AreaRegistration.RegisterAllAreas();
            //RegistFormatter();
            GlobalConfiguration.Configure(Regist);
            GlobalConfiguration.Configuration.MessageHandlers.Add(new HttpHandler());
        }

        /// <summary>
        /// Application_Start时调用(内部使用)
        /// </summary>
        public static void OnApplicationStartInner()
        {
            AreaRegistration.RegisterAllAreas();
            //RegistFormatter();
            GlobalConfiguration.Configure(Regist);
            GlobalConfiguration.Configuration.MessageHandlers.Add(new HttpHandler());
        }

        /// <summary>
        /// 注册
        /// </summary>
        private static void Regist(HttpConfiguration config)
        {
            RegistFilter(config);
        }
        /// <summary>
        /// 注册过滤器
        /// </summary>
        public static void RegistFilter(HttpConfiguration config)
        {
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
        }

        /// <summary>
        /// 注册格式化器
        /// </summary>
        public static void RegistFormatter()
        {
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            //默认返回 json  
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SerializerSettings.Formatting = Formatting.None;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            json.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            json.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            json.SerializerSettings.Culture = CultureInfo.GetCultureInfo("zh-cn");
            json.MediaTypeMappings.Add(new QueryStringMapping("datatype", "json", "application/json"));
        }

    }
}

#endif