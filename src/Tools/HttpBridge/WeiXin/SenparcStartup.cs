using Agebull.Common.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Cache.Redis;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.RegisterServices;

namespace MicroZero.Http.Gateway
{
    public class SenparcStartup : GatewayStartup
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="configuration"></param>
        public SenparcStartup(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// 执行系统配置
        /// </summary>
        /// <param name="services"></param>
        protected override void DoConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                .AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册
        }

        /// <summary>
        /// 应用配置
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected override void DoConfigure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var senparcSetting = ConfigurationManager.Root.GetSection("SenparcSetting")?.Get<SenparcSetting>();
            var senparcWeixinSetting = ConfigurationManager.Root.GetSection("SenparcWeixinSetting")?.Get<SenparcWeixinSetting>();

            // 启动 CO2NET 全局注册，必须！
            IRegisterService register = RegisterService.Start(env, senparcSetting).UseSenparcGlobal();

            //如果需要自动扫描自定义扩展缓存，可以这样使用：
            //register.UseSenparcGlobal(true);
            //如果需要指定自定义扩展缓存，可以这样用：
            //register.UseSenparcGlobal(false, GetExCacheStrategies);

            #region CO2NET 全局配置

            #region 全局缓存配置（按需）

            //当同一个分布式缓存同时服务于多个网站（应用程序池）时，可以使用命名空间将其隔离（非必须）
            register.ChangeDefaultCacheNamespace("DefaultCO2NETCache");

            #region 配置和使用 Redis 
            Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(senparcSetting.Cache_Redis_Configuration);
            Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow();//键值对缓存策略（推荐）
            app.UseSenparcWeixinCacheRedis();
            #endregion                        // PDBMARK_END

            #endregion

            #endregion

            #region 微信相关配置



            //开始注册微信信息，必须！
            register.UseSenparcWeixin(senparcWeixinSetting, senparcSetting)
                //注册公众号（可注册多个）                                                -- PDBMARK MP
                .RegisterMpAccount(senparcWeixinSetting, "公众号");// PDBMARK_END


            #endregion
        }
    }
}