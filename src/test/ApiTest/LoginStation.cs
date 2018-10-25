using System;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using System.ComponentModel;

namespace ApiTest
{
    /// <summary>
    /// 登录服务
    /// </summary>
    public abstract class ApiControlleraaa : ApiController
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [Route("api/test"), Category("登录")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult Login(LoginArg arg)
        {
            Test();
            return new ApiResult
            {
                Success = true,
                Status = new ApiStatusResult
                {
                    ErrorCode = 0,
                    //ClientMessage = $"Wecome {user.MobilePhone}!"
                }
            };
        }
        public abstract void Test();
    }

    /// <summary>
    /// 登录服务
    /// </summary>
    [Station("TestCtr")]
    public class LoginStation : ApiControlleraaa
    {
        public override void Test()
        {
            Console.WriteLine("OK");
        }
    }
}
