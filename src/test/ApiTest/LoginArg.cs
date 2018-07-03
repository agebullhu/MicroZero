using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    /// <summary>
    /// 登录参数
    /// </summary>
    public class LoginArg : IApiArgument
    {
        /// <summary>
        /// 手机号
        /// </summary>
        /// <value>11位手机号,不能为空</value>
        /// <example>15618965007</example>
        public string MobilePhone { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        /// <value>6-16位特殊字符\字母\数字组成,特殊字符\字母\数字都需要一或多个,不能为空</value>
        /// <example>pwd#123</example>
        public string UserPassword { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        /// <value>6位字母或数字,不能为空</value>
        /// <example>123ABC</example>
        public string VerificationCode { get; set; }
        
        /// <summary>
        /// 校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool IApiArgument.Validate(out string message)
        {
            message = null;
            return true;
        }
    }
}
