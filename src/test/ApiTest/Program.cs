using System;
using System.Collections.Generic;
using System.Reflection;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;
using Agebull.MicroZero.ZeroManagemant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.RegisterServices;



namespace ApiTest
{
    partial class Program
    {


        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            //ZeroApplication.Discove(Assembly.GetExecutingAssembly());
            ZeroApplication.Initialize();
            ZeroApplication.Run();

            Console.ReadKey();
        }

        //static void Weixin()
        //{
        //    IocHelper.ServiceCollection.AddMemoryCache();

        //    IocHelper.ServiceCollection
        //        .AddSenparcGlobalServices(ConfigurationManager.Root)//Senparc.CO2NET 全局注册
        //        .AddSenparcWeixinServices(ConfigurationManager.Root);//Senparc.Weixin 注册

        //    var senparcSetting = ConfigurationManager.Root.GetSection("SecurityHeaderOptions").Get<SenparcSetting>();
        //    var senparcWeixinSetting = ConfigurationManager.Root.GetSection("SenparcWeixinSetting").Get<SenparcWeixinSetting>();
        //    RegisterService.Start(null, senparcSetting)
        //        .UseSenparcGlobal()// 启动 CO2NET 全局注册
        //        .UseSenparcWeixin(senparcWeixinSetting, senparcSetting);//微信全局注册
        //    //注册AppId
        //    AccessTokenContainer.Register(senparcWeixinSetting.WeixinAppId, senparcWeixinSetting.WeixinAppSecret);
        //}
    }

    public class TestItem
    {
        public IEnumerable<long> Test { get; set; }
    }

    public class TestItems
    {
        IEnumerable<TestItem> Items { get; set; }
    }

    /// <summary>
    /// Weixin服务
    /// </summary>
    public class TestController : ApiController
    {
        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        [Route("v1/test")]
        public ApiResult<TestItems> OnTextRequest()
        {
            return new ApiResult<TestItems>
            {
                Success = true
            };
        }
    }
}
