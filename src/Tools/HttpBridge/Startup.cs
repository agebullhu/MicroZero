using System.IO;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin.RegisterServices;

namespace MicroZero.Http.Gateway
{
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IocHelper.SetServiceCollection(services);

            services.AddMemoryCache();
            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET È«¾Ö×¢²á
                    .AddSenparcWeixinServices(Configuration);//Senparc.Weixin ×¢²á
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseFileServer();

            string ext = ConfigurationManager.AppSettings["WechatPath"];
            LogRecorder.SystemLog(ext);
            RouteApp.Extends.Add(ext, () => new WechatRouter());
            RouteApp.Extends.Add(ext + "/", () => new WechatRouter());
            RouteApp.Initialize();
            app.Run(RouteApp.Call);
        }
    }
}