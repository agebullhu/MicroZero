## 在Asp.core.net中使用ZeroNet

###  一 新建Project一个Asp.net core项目
> Core版本 : 不低于于2.0.3

###  二 引用Nuget包

1. ZeroNet.core
2. AgebullExtend.core : 隐式引用
3. Agebull.LogRecorder : 隐式引用
4. Agebull.EntityModel.Core : 隐式引用
5. ... 其它依赖

### 三 编码
####  Program.cs:
主进程的入口
``` Csharp
using Agebull.Common.Configuration;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
            ZeroApplication.Shutdown();//结束时关闭Zeronet连接及销毁实例，注1
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(ConfigurationManager.Root)//使用相同的配置项，注2
                .UseStartup<Startup>()
                .Build();
        }
    }
}
```
代码说明:

1 注1：因Aspnet.core全局关闭并不能主动关闭ZeroNet，故需要手动销毁

2 注2：虽然Zeronet内部是使用标准的NetCore的配置对象，但由于实例不同，如不使用这一句，会导致内部两个配置实例而产生配置内容差异化。

#### Startup.cs
Startup过程中使ZeroNet按预定流程正确启动
1 Startup构造时：初始化配置对象
2 ConfigureServices时：对象依赖发现与初始化ZeroNet
3 Configure时：启动ZeroNet主线程

``` csharp
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZeroNet.Http.Route;

namespace WebMonitor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //初始化配置，注1
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
            services.AddMvc();
            //对象依赖发现
            IocHelper.AddSingleton<PlanManage>();
            ZeroApplication.RegistZeroObject<ApiCounter>();
            ZeroApplication.RegistZeroObject<PlanSubscribe>();
            ZeroApplication.RegistZeroObject<EventSub>();
            
            //初始化ZeroNet，注2
            ZeroApplication.Initialize(); 
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //.AddJsonOptions(options =>
            // {
            //     //忽略循环引用
            //     options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //     //不使用驼峰样式的key
            //     options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            //     //设置时间格式
            //     options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //启动ZeroNet主线程，注3
            Task.Factory.StartNew(ZeroApplication.Run);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            WebSocketNotify.Binding(app);

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "api",
                    template: "{controller}/{action}/{station}");
            });
        }
    }
}
``` 
代码说明:

1 注1：虽然Zeronet内部是使用标准的NetCore的配置对象，但由于实例不同，如不使用这一句，会导致内部两个配置实例而产生配置内容差异化。

2 注2、注3：Zeronet内部需要一个完整的线程循环，故必须进行初始化和运行方可正常运行。

#### ZeroNet的Api调用

> ZeroNet的Api调用分为两种方式，内部调用与外部调用，内部调用直接通过ApiClient走ZeroMQ通讯调用，外部调用使用标准HTTP通讯协议，
通过HttpGateway与API进行通讯 

##### 内部调用
> 使用ApiClient类

调用的方法

``` csharp
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static TResult Call<TArgument, TResult>(string station, string api, TArgument arg)；
        
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static ApiResult<TResult> CallApi<TArgument, TResult>(string station, string api, TArgument arg)；
        
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static ApiResult CallApi<TArgument>(string station, string api, TArgument arg)；
        
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static ApiResult CallApi(string station, string api)；
        
```

##### 外部调用

> 使用标准的HTTP Rustful方式调用。

特殊说明：

1 接口名称规范 http://[host]/[station]/[apiName]

 - host: HttpGateway的部署根地址,如 www.agebull.com
 
 - sttion:Api站点的部署名称
 
 - apiName:Api定义的路由名称
 
2 Post?Get？由于是进行了映射，所以两种方式均可，但由于Get方式受限比较多，不建议使用

3 Auth2.0：如Api定义为需要登录用户，则必须在Authorization头上加入Bear [token]











