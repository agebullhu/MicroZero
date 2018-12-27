using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ZeroNet.Http.Gateway
{
    public interface IStartup
    {
        IConfiguration Configuration { get; set; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        void ConfigureServices(IServiceCollection services);

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        void Configure(IApplicationBuilder app, IHostingEnvironment env);
    }
}