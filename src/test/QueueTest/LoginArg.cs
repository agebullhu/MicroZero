using Gboxt.Common.DataModel;

namespace ApiTest
{ 
    /// <summary>
    /// 登录参数
    /// </summary>
    public class TestArg : IApiArgument
    {
        /// <summary>
        /// 手机号
        /// </summary>
        /// <value>11位手机号,不能为空</value>
        /// <example>15618965007</example>
        [DataRule(CanNull = false, Max = 11, Min = 11, Regex = "1[3-9]\\d{9,9}")]
        public string MobilePhone { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        /// <value>6-16位特殊字符\字母\数字组成,特殊字符\字母\数字都需要一或多个,不能为空</value>
        /// <example>pwd#123</example>
        [DataRule(CanNull = false, Max = 6, Min = 16, Regex = "[\\da-zA-Z~!@#$%^&*]{6,16}")]
        public string UserPassword { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        /// <value>6位字母或数字,不能为空</value>
        /// <example>123ABC</example>
        [DataRule(CanNull = false, Max = 6, Min = 6, Regex = "[a-zA-Z\\d]{6,6}")]
        public string VerificationCode { get; set; }
        
        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }

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
