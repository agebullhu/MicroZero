using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// 端口配置
    /// </summary>
    public static class HostExtend
    {
        /// <summary>
        /// 配置HTTP
        /// </summary>
        /// <param name="options"></param>
        public static void KestrelOptions(KestrelServerOptions options)
        {
            options.ConfigureEndpointDefaults(listenOptions =>
            {
                listenOptions.UseConnectionLogging();
            });
            options.AddServerHeader = true;
            //将此选项设置为 null 表示不应强制执行最低数据速率。
            options.Limits.MinResponseDataRate = null;
            //options.AllowSynchronousIO = false;
            
            //var httpOptions = ConfigurationManager.Root.GetSection("http").Get<HttpOption[]>();
            //foreach (var option in httpOptions)
            //{
            //    if (option.IsHttps)
            //    {
            //        var filename = option.CerFile[0] == '/'
            //            ? option.CerFile
            //            : Path.Combine(Environment.CurrentDirectory, option.CerFile);
            //        var certificate = new X509Certificate2(filename, option.CerPwd);
            //        options.Listen(IPAddress.Any, option.Port, listenOptions =>
            //        {
            //            listenOptions.UseHttps(certificate);
            //            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            //        });
            //    }
            //    else
            //    {
            //        options.Listen(IPAddress.Any, option.Port, listenOptions =>
            //        {
            //            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            //        });
            //    }
            //}
        }

        /// <summary>
        /// 配置日志
        /// </summary>
        /// <param name="context"></param>
        /// <param name="builder"></param>
        public static void LoggerOptions(WebHostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddConfiguration(ConfigurationManager.Root.GetSection("Logging"));
            LogRecorder.Initialize();
            builder.AddConsole();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TextLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<TextLoggerOption, TextLoggerProvider>(builder.Services);
        }
    }
}