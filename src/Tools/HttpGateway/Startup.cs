using System.IO;
using Agebull.Common.Ioc;
using Agebull.MicroZero;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroZero.Http.Gateway
{
    public abstract class StartupBase
    {
        protected StartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        //
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IocHelper.SetServiceCollection(services);
            DoConfigureServices(services);
        }

        protected abstract void DoConfigureServices(IServiceCollection services);

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseFileServer();

            DoConfigure(app, env);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        protected abstract void DoConfigure(IApplicationBuilder app, IHostingEnvironment env);
    }

    internal sealed class Startup : StartupBase
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        //
        // This method gets called by the runtime. Use this method to add services to the container.
        protected override void DoConfigureServices(IServiceCollection services)
        {
            ZeroApplication.CheckOption();
            RouteOption.ConfigFileName = Path.Combine(ZeroApplication.Config.ConfigFolder, "route_config.json");
            ZeroTrace.SystemLog("HttpGateway", RouteOption.ConfigFileName);
            RouteOption.CheckOption();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        protected override void DoConfigure(IApplicationBuilder app, IHostingEnvironment env)
        {
            RouteApp.Initialize();
            app.Run(RouteApp.Call);
        }
    }
}