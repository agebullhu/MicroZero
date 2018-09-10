using Agebull.Common.Rpc;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using System.ComponentModel;

namespace ApiTest
{
    /// <summary>
    /// 登录服务
    /// </summary>
    [Station("Login")]
    public class LoginStation : ApiController
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [Route("api/login"), Category("登录")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult Login()
        {
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
    }
}
