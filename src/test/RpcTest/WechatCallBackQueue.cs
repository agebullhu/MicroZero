
using System.Threading.Tasks;
using Agebull.MicroZero.ZeroApis;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;

namespace ApiTest
{
    /// <summary>
    /// 支付回调
    /// </summary>
    //[Station("PayCallback")]
    public class PayCallbackController : QueueStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        public PayCallbackController()
        {
            Name = "PayCallback";
            StationName = "PayCallback";
            Subscribe = "";
        }

        /// <summary>
        ///     配置校验
        /// </summary>
        protected override ZeroStationOption GetApiOption()
        {
            var option = ZeroApplication.GetApiOption(StationName);
            option.SpeedLimitModel = SpeedLimitType.Single;
            return option;
        }

        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <returns></returns>
        [Route("v1/wx/call")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnWxCallback()
        {
            return ApiResult.Ok;
        }
        /// <summary>
        /// 微信支付回调V2版本
        /// </summary>
        /// <returns></returns>
        [Route("v2/wx/call")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnWxCallbackV2()
        {
            return ApiResult.Ok;
        }

        /// <summary>
        /// 微信支付超时处理
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v1/pay/timeout")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnWxPayTimeout(Argument arg)
        {
            return ApiResult.Ok;
        }
        /// <summary>
        /// 微信支付超时处理V2版本
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v2/pay/timeout")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnWxPayTimeoutV2(Argument arg)
        {
            return ApiResult.Ok;
        }
        /// <summary>
        /// 订单超时处理
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v1/order/timeout")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnOrderTimeout(Argument<long> arg)
        {
            return ApiResult.Ok;
        }
        /// <summary>
        /// 微信支付退款回调
        /// </summary>
        /// <returns></returns>
        [Route("v2/wx/refundcallback")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnWxRefundCallbackV2()
        {
            return ApiResult.Ok;
        }

    }
}
