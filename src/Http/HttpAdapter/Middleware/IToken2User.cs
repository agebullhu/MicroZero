using Agebull.Common.OAuth;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// 令牌解析
    /// </summary>
    public interface IToken2User
    {
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="token"></param>
        /// <returns>登录用户信息</returns>
        ILoginUserInfo UserInfo(string token)
        {
            return LoginUserInfo.System;
        }
    }
}