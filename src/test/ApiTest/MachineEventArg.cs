using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using System.Text.RegularExpressions;

namespace ApiTest
{
    /// <summary>
    /// 登录参数
    /// </summary>
    public class MachineEventArg : IApiArgument
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// 机器标识
        /// </summary>
        public string MachineId { get; set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        public string JsonStr { get; set; }
        
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
