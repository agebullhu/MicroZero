using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Yizuan.Service.Api.OAuth
{
    /// <summary>
    /// 用户身份校验
    /// </summary>
    public interface IBearValidater
    {
        /// <summary>
        /// 检查调用的ServiceKey（来自内部调用）
        /// </summary>
        /// <param name="token">令牌</param>
        /// <returns></returns>
        ApiResult ValidateServiceKey(string token);

        /// <summary>
        /// 检查AT(来自登录用户)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        ApiResult<LoginUserInfo> VerifyAccessToken(string token);

        /// <summary>
        /// 检查设备标识（来自未登录用户）
        /// </summary>
        /// <param name="token">令牌</param>
        /// <returns></returns>
        ApiResult<LoginUserInfo> ValidateDeviceId(string token);

        /// <summary>
        /// 检查设备标识（来自未登录用户）
        /// </summary>
        /// <param name="at">登录用户的AT</param>
        /// <returns></returns>
        ApiResult<LoginUserInfo> GetLoginUser(string at);
    }
}