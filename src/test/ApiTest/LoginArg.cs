using Gboxt.Common.DataModel;

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
        public Entertype State { get; set; }

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

    /// <summary>
    /// 进入方式
    /// </summary>
    /// <remark>
    /// 1:刷脸,2:人证,3:邀请码,4:二维码,5:IC卡,6:驾驶证,7:护照 8:港奥,9:手动填写
    /// </remark>
    public enum Entertype
    {
        /// <summary>
        /// 刷脸
        /// </summary>
        刷脸 = 0x1,
        /// <summary>
        /// 人证
        /// </summary>
        人证 = 0x2,
        /// <summary>
        /// 邀请码
        /// </summary>
        邀请码 = 0x3,
        /// <summary>
        /// 二维码
        /// </summary>
        二维码 = 0x4,
        /// <summary>
        /// IC卡
        /// </summary>
        IC卡 = 0x5,
        /// <summary>
        /// 驾驶证
        /// </summary>
        驾驶证 = 0x6,
        /// <summary>
        /// 护照
        /// </summary>
        护照 = 0x7,
        /// <summary>
        /// 港奥
        /// </summary>
        港奥 = 0x8,
        /// <summary>
        /// 手动填写
        /// </summary>
        手动填写 = 0x9,
    }
}
