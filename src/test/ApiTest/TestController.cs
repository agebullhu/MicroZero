using Agebull.MicroZero.ZeroApis;
using System.Collections.Generic;
using System.Threading;

namespace ApiTest
{

    public class TestItem
    {
        public IEnumerable<long> Test { get; set; }
    }

    public class TestItems
    {
        IEnumerable<TestItem> Items { get; set; }
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
            Thread.Sleep(61000);
            return new ApiResult<TestItems>
            {
                Success = true
            };
        }
    }
}
