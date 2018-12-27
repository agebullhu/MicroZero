using System.Collections.Generic;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Cache;
using Senparc.Weixin.Cache.Redis;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.RegisterServices;

namespace ZeroNet.Http.Gateway
{
    public class SenparcStartup 
    {
        public IConfiguration Configuration { get; set; }

        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                .AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册

            var senparcSetting = ConfigurationManager.Root.GetSection("SenparcSetting")?.Get<SenparcSetting>();
            var senparcWeixinSetting = ConfigurationManager.Root.GetSection("Weixin")?.Get<SenparcWeixinSetting>();

            IRegisterService register = RegisterService.Start(null, senparcSetting).UseSenparcGlobal(false, GetExCacheStrategies);

            //开始注册微信信息，必须！
            register.UseSenparcWeixin(senparcWeixinSetting, senparcSetting).RegisterMpAccount(senparcWeixinSetting);

        }
        /// <summary>
        /// 获取扩展缓存策略
        /// </summary>
        /// <returns></returns>
        private IList<IDomainExtensionCacheStrategy> GetExCacheStrategies()
        {
            return new List<IDomainExtensionCacheStrategy> {LocalContainerCacheStrategy.Instance};
        }
    }
}