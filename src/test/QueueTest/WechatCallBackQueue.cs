using System;
using Agebull.Common.Context;
using Agebull.Common.Logging;

using Agebull.MicroZero.ZeroApis;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Senparc.NeuChar;
using Newtonsoft.Json;

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
        /// 微信支付回调
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v1/wx/call")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnWxCallback()/*WxPayResult arg*/
        {
            return ApiResult.Ok;
        }

        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v2/wx/call")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnWxCallbackV2(WxPayResult arg)/**/
        {
            return ApiResult.Ok;
        }

        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v2/wx/refundcallback")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnRefundCallbackV2()/*WxPayResult arg*/
        {
            return ApiResult.Ok;
        }
        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v1/order/timeout")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnOrderTimeOutV1()/*WxPayResult arg*/
        {
            return ApiResult.Ok;
        }
        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v2/order/timeout")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnOrderTimeOutV2()/*WxPayResult arg*/
        {
            return ApiResult.Ok;
        }
        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v1/pay/timeout")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnPayTimeOutV1()/*WxPayResult arg*/
        {
            return ApiResult.Ok;
        }
        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [Route("v2/pay/timeout")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult OnPayTimeOutV2(WxPayResult arg)/**/
        {
            return ApiResult.Ok;
        }

    }

    /// <summary>
    /// 微信支付回调信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class WxPayResult
    {

        [JsonProperty("wx_appid")] private string _appid;


        [JsonProperty("wx_bank_type")] private string _bankType;

        [JsonProperty("wx_cash_fee")] private decimal _cashFee;


        [JsonProperty("wx_fee_type")] private string _feeType;

        [JsonProperty("wx_is_subscribe")] private string _isSubscribe;

        [JsonProperty("wx_mch_id")] private string _mchId;


        [JsonProperty("wx_nonce_str")] private string _nonceStr;

        [JsonProperty("wx_openid")] private string _openid;

        [JsonProperty("wx_out_trade_no")] private string _outTradeNo;

        [JsonProperty("wx_result_code")] private string _resultCode;

        [JsonProperty("wx_return_code")] private string _returnCode;


        [JsonProperty("wx_sign")] private string _sign;

        [JsonProperty("wx_sub_appid")] private string _subAppid;

        [JsonProperty("wx_sub_is_subscribe")] private string _subIsSubscribe;

        [JsonProperty("wx_sub_mch_id")] private string _subMchId;

        [JsonProperty("wx_sub_openid")] private string _subOpenid;

        [JsonProperty("wx_time_end")] private string _timeEnd;

        [JsonProperty("wx_total_fee")] private decimal _totalFee;


        [JsonProperty("wx_trade_type")] private string _tradeType;


        [JsonProperty("wx_transaction_id")] private string _transactionId;

        /// <summary>
        ///     微信开放平台审核通过的应用APPID
        /// </summary>
        public string Appid
            => _appid;

        /// <summary>
        ///     微信支付分配的商户号
        /// </summary>
        public string MchId
            => _mchId;

        /// <summary>
        ///     微信开放平台审核通过的应用APPID
        /// </summary>
        public string SubAppid
            => _subAppid;

        /// <summary>
        ///     微信支付分配的商户号
        /// </summary>
        public string SubMchId
            => _subMchId;

        /// <summary>
        ///     用户在商户appid下的唯一标识
        /// </summary>
        public string Openid
            => _openid;

        /// <summary>
        ///     用户在商户sub_appid下的唯一标识
        /// </summary>
        public string SubOpenid
            => _subOpenid;

        /// <summary>
        ///     随机字符串，不长于32位
        /// </summary>
        public string NonceStr
            => _nonceStr;

        /// <summary>
        ///     签名，详见签名算法
        /// </summary>
        public string Sign
            => _sign;

        /// <summary>
        ///     SUCCESS/FAIL
        /// </summary>
        public string ResultCode
            => _resultCode;

        /// <summary>
        ///     返回状态
        /// </summary>
        public string ReturnCode
            => _returnCode;

        /// <summary>
        ///     用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效
        /// </summary>
        public string IsSubscribe
            => _isSubscribe;

        /// <summary>
        ///     用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效
        /// </summary>
        public string SubIsSubscribe
            => _subIsSubscribe;

        /// <summary>
        ///     订单类型
        /// </summary>
        public string TradeType
            => _tradeType;

        /// <summary>
        ///     银行类型，采用字符串类型的银行标识，银行类型见银行列表
        /// </summary>
        public string BankType => _bankType;

        /// <summary>
        ///     货币类型，符合ISO4217标准的三位字母代码，默认人民币：CNY，其他值列表详见货币类型
        /// </summary>
        public string FeeType => _feeType;

        /// <summary>
        ///     微信支付订单号
        /// </summary>
        public string TransactionId => _transactionId;

        /// <summary>
        ///     商户系统的订单号，与请求一致。
        /// </summary>
        public string OutTradeNo => _outTradeNo;

        /// <summary>
        ///     支付完成时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010。其他详见时间规则
        /// </summary>
        public string TimeEnd => _timeEnd;

        /// <summary>
        ///     订单总金额，单位为分
        /// </summary>
        public decimal TotalFee => _totalFee / 100;

        /// <summary>
        ///     现金支付金额订单现金支付金额，详见支付金额
        /// </summary>
        public decimal CashFee => _cashFee / 100;

    }
}
