using System.IO;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZeroNet.Http.Gateway;
using StartupBase = ZeroNet.Http.Gateway.StartupBase;

namespace Xuhui.Internetpro.Gateway
{

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