using System.IO;
using Agebull.Common.Ioc;
using Agebull.MicroZero;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    /// 启动基类
    /// </summary>
    public class GatewayStartup
    {

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="configuration"></param>
        public GatewayStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 配置对象
        /// </summary>
        public static IConfiguration Configuration { get; private set; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            IocHelper.SetServiceCollection(services);
            ZeroApplication.WorkModel = ZeroWorkModel.Client;

            ZeroApplication.CheckOption();
            GatewayOption.CheckOption();
            //services.AddCors(options =>
            //    options.AddPolicy("AllowSameDomain",
            //        builder => builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin().AllowCredentials())
            //);
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseFileServer();

            RouteApp.Initialize();
            app.Run(RouteApp.Call);
        }
    }

}