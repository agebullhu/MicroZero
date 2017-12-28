using System.Configuration;
using Agebull.Common.Logging;
using ExternalStation.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yizuan.Service.Host;

namespace ExternalStation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            HttpApplication.Initialize();
            //services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseBrowserLink();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}

            //app.UseStaticFiles();
            app.Run(HttpApplication.Call);
            /*app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Default",
                    template: "{h}",
                    defaults: new { controller = "Home", action = "Call" }
                );
                routes.MapRoute(
                    name: "d1",
                    template: "{h}/{a}",
                    defaults: new { controller = "Home", action = "Call" }
                );
                routes.MapRoute(
                    name: "d2",
                    template: "{h}/{a}/{b}",
                    defaults: new { controller = "Home", action = "Call" }
                );
                routes.MapRoute(
                    name: "d3",
                    template: "{h}/{a}/{b}/{c}",
                    defaults: new { controller = "Home", action = "Call" }
                );
                routes.MapRoute(
                    name: "d4",
                    template: "{h}/{a}/{b}/{c}/{d}",
                    defaults: new { controller = "Home", action = "Call" }
                );
            });*/
        }
    }
}
