using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    /// <summary>
    /// 登录服务
    /// </summary>
    [Station("Login")]
    public class LoginStation : ZeroApiController
    {
        //public LoginStation()
        //{
        //    Name = "Login";
        //    StationName = "Login";
        //}

        ///// <summary>
        ///// 初始化
        ///// </summary>
        //protected override void Initialize()
        //{
        //    RegistAction<LoginArg, ApiResult>("api/login", Login, ApiAccessOption.Anymouse);
        //}
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns></returns>
        [Route("api/login")]
        public ApiResult Login(LoginArg user)
        {
            return new ApiResult
            {
                Success = true,
                Status = new ApiStatsResult
                {
                    ClientMessage = $"Wecome {user.MobilePhone}!"
                }
            };
        }
    }
}
