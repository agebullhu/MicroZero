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
            ZeroApplication.CheckOption();
            RouteOption.CheckOption();

            services.AddCors(options =>
                options.AddPolicy("AllowSameDomain",
                    builder => builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin().AllowCredentials())
            );

            DoConfigureServices(services);
        }

        /// <summary>
        /// 执行系统配置
        /// </summary>
        /// <param name="services"></param>
        protected virtual void DoConfigureServices(IServiceCollection services)
        {
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

            DoConfigure(app, env);

             RouteApp.Initialize();
            app.Run(RouteApp.Call);
        }

        /// <summary>
        /// 应用配置
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected virtual void DoConfigure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }

}