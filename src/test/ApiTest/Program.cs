using Agebull.Common.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using WebApplication2;
using Agebull.MicroZero.ZeroApis;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Service
{
    class Program
    {
        /// <summary>
        /// 入口
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseLibuv()
                    .UseConfiguration(ConfigurationManager.Root)
                    .ConfigureLogging(HostExtend.LoggerOptions)
                    .UseUrls(ConfigurationManager.Root.GetSection("Kestrel.Endpoints.Http.Url").Value)
                    //.ConfigureServices((context, services) =>
                    //{
                    //    services.Configure<KestrelServerOptions>(context.Configuration.GetSection("Kestrel"));
                    //})
                    //.ConfigureKestrel(HostExtend.KestrelOptions)//HTTPS端口配置
                    .UseKestrel(HostExtend.KestrelOptions)//HTTPS端口配置
                    .UseStartup<Startup>();
                });
    }
}
