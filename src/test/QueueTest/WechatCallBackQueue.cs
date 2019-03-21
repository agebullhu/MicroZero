using System;
using Agebull.Common.Context;
using Agebull.Common.Logging;

using Agebull.MicroZero.ZeroApi;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;
using Senparc.NeuChar;
using MicroZero.Http.Gateway.Weixin;

namespace ApiTest
{
    /// <summary>
    /// 登录服务
    /// </summary>
    [Station("WechatCallBack")]
    public class WechatCallBackQueue : QueueStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        public WechatCallBackQueue()
        {
            Name = "WechatCallBack";
            StationName = "WechatCallBack";
            Subscribe = "";
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [Route("CallBack")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult CallBack()
        {
            var item = GlobalContext.Current.DependencyObjects.Dependency<ApiCallItem>();
            if (!Enum.TryParse<RequestMsgType>(item.Title, out var type))
                return ApiResult.ArgumentError;
            try
            {
                var wx = new WeixinMessageHandler();
                wx.Handler(type, item.Argument);
                return ApiResult.Succees();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return ApiResult.LocalException;
            }
        }
    }

}
