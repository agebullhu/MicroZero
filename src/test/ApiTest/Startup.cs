using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Agebull.MicroZero.ZeroApis;
using Agebull.MicroZero;
using Microsoft.OpenApi.Models;
using System.IO;
using Service;
using Agebull.Common;

namespace WebApplication2
{
    /// <summary>
    /// 启动配置
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 配置对象
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new JsonNumberConverter());
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
                    options.SerializerSettings.ContractResolver =
                                    new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });


            services.AddScoped<GlobalContext, GlobalContext>();
            services.AddTransient<IToken2User, AuthHelper>();

            if (IocHelper.ServiceCollection != services)
                IocHelper.ServiceCollection = services;

            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                // 为 Swagger JSON and UI设置xml文档注释路径
                var xmls = IOHelper.GetAllFiles(Environment.CurrentDirectory, "*.xml");
                foreach (var xml in xmls)
                    c.IncludeXmlComments(xml);
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<MicroZeroMiddleware>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            //app.UseCookiePolicy();
            //app.UseFileServer();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
