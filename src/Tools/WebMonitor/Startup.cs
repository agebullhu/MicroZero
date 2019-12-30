using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.MicroZero;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MicroZero.Devops.ZeroTracer.DataAccess;
using MicroZero.Http.Route;

namespace WebMonitor
{


    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigurationManager.SetConfiguration(configuration);
            ZeroApplication.CheckOption();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews();

            IocHelper.SetServiceCollection(services);
            IocHelper.AddSingleton<PlanManage>();
            IocHelper.AddScoped<ZeroTracerDb, ZeroTracerDb>();
            //ZeroApplication.RegistZeroObject<FlowTracer>();//ApiCounter
            ZeroApplication.RegistZeroObject<PlanSubscribe>();
            ZeroApplication.Initialize();

        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Task.Factory.StartNew(ZeroApplication.Run,TaskCreationOptions.LongRunning);
            WebSocketNotify.Binding(app);
            StationCounter.Start();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "{controller}/{action}/{station}");
            });
        }
    }
}
