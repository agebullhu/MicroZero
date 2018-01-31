using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Gboxt.Common.DataModel;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadKey();
            var tester = new RegistTester();
            tester.RegistAction<VerificationApiController>();
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 身份验证服务API
    /// </summary>
    public class VerificationApiController
    {
        /// <summary>
        ///     验证图片验证码:验证图片验证码:
        /// </summary>
        /// <param name="arg">验证码检查参数</param>
        /// <returns>操作结果</returns>
        [Route("v2/vc/img/valiadate")]
        [ApiAccessOptionFilter(ApiAccessOption.Internal | ApiAccessOption.Public | ApiAccessOption.Anymouse)]
        public static IApiResult CheckImgVerificationCode(IApiArgument arg)
        {
            return ApiResult.Error(-1,arg?.ToString());
        }
    }
}
