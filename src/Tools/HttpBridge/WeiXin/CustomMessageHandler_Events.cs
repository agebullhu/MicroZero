using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.BaiduMap;
using Senparc.NeuChar;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.Entities;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.AdvancedAPIs;
using ZeroNet.Http.Gateway;
using Senparc.NeuChar.Helpers;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.Tencent;

namespace Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler
{
    /// <summary>
    /// 自定义MessageHandler
    /// </summary>
    public partial class CustomMessageHandler
    {

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool? Success { get; private set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public string ResultXml { get; private set; }
        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private void CallZero<T>(string command, T argument)
        {
            Success = false;
            int cnt = 0;
            while (++cnt < 3)
            {
                CallZeroInner(command, argument);
                if (Success == true)
                    return;
                Thread.Sleep(50);
            }
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private void CallZeroInner<T>(string command, T argument)
        {
            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                var caller = new ApiClient
                {
                    Station = "WechatCallBack",
                    Commmand = command,
                    Argument = JsonConvert.SerializeObject(argument)
                };
                caller.CallCommand();
                ResultXml = caller.State == ZeroOperatorStateType.Ok ? caller.Result : "";
                Success = caller.State == ZeroOperatorStateType.Ok && !String.IsNullOrWhiteSpace(ResultXml);
                LogRecorder.MonitorTrace($"{caller.State } : {caller.Result}");
                LogRecorder.MonitorTrace($"Success  : {Success}");
                LogRecorder.MonitorTrace($"ResultXml: {ResultXml}");
            }
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            CallZero("v1/msg/text", requestMessage);
            if (Success == true)
            {
                var res = new ResponseMessageText();
                res.FillEntityWithXml(XDocument.Parse(ResultXml));
                var result = CustomApi.SendText(WechatProcesser.Option.WeixinAppId, requestMessage.FromUserName, res.Content);
                LogRecorder.MonitorTrace(result.ToJson());
            }
            return new ResponseMessageText();
        }

        /// <summary>
        /// 订阅（关注）事件
        /// </summary>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            CallZero("v1/event/sub", requestMessage);
            if (Success == true)
            {
                var res = new ResponseMessageNews();
                res.FillEntityWithXml(XDocument.Parse(ResultXml));
                var result = CustomApi.SendNews(WechatProcesser.Option.WeixinAppId, requestMessage.FromUserName, res.Articles);
                LogRecorder.MonitorTrace(result.ToJson());
            }
            return new ResponseMessageText();
        }

        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            CallZero("v1/msg/local", requestMessage);
            var markersList = new List<BaiduMarkers>
            {
                new BaiduMarkers()
                {
                    Longitude = requestMessage.Location_X,
                    Latitude = requestMessage.Location_Y,
                    Color = "red",
                    Label = "S",
                    Size = BaiduMarkerSize.m
                }
            };

            var mapUrl = BaiduMapHelper.GetBaiduStaticMap(requestMessage.Location_X, requestMessage.Location_Y, 1, 6,
                markersList);
            CustomApi.SendNews(WechatProcesser.Option.WeixinAppId, requestMessage.FromUserName, new List<Article>
            {
                new Article()
                {
                    Description =
                        $"【来自百度地图】您刚才发送了地理位置信息。Location_X：{requestMessage.Location_X}，Location_Y：{requestMessage.Location_Y}，Scale：{requestMessage.Scale}，标签：{requestMessage.Label}",
                    PicUrl = mapUrl,
                    Title = "定位地点周边地图",
                    Url = mapUrl
                }
            });
            return new ResponseMessageText();
        }

        public IResponseMessageBase OnShortVideoRequest(RequestMessageShortVideo requestMessage)
        {

            CallZero("v1/msg/shortvedio", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {

            CallZero("v1/msg/image", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 处理语音请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {

            CallZero("v1/msg/voice", requestMessage);
            return new ResponseMessageText();
        }
        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {

            CallZero("v1/msg/vedio", requestMessage);
            return new ResponseMessageText();
        }


        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {

            CallZero("v1/msg/link", requestMessage);
            return new ResponseMessageText();
        }

        public IResponseMessageBase OnFileRequest(RequestMessageFile requestMessage)
        {

            CallZero("v1/msg/file", requestMessage);
            return new ResponseMessageText();
        }

        public IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {

            CallZero("v1/msg/def", requestMessage);
            return new ResponseMessageText();
        }


        public IResponseMessageBase OnUnknownTypeRequest(RequestMessageUnknownType requestMessage)
        {

            CallZero("v1/msg/unknown", requestMessage);
            return new ResponseMessageText();
        }
        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_ClickRequest(RequestMessageEvent_Click requestMessage)
        {

            CallZero("v1/event/click", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 进入事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_EnterRequest(RequestMessageEvent_Enter requestMessage)
        {

            CallZero("v1/event/enter", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 位置事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_LocationRequest(RequestMessageEvent_Location requestMessage)
        {

            CallZero("v1/event/local", requestMessage);

            return new ResponseMessageText();
        }

        /// <summary>
        /// 通过二维码扫描关注扫描事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_ScanRequest(RequestMessageEvent_Scan requestMessage)
        {

            CallZero("v1/event/scan", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 打开网页事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_ViewRequest(RequestMessageEvent_View requestMessage)
        {

            CallZero("v1/event/view", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 群发完成事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_MassSendJobFinishRequest(RequestMessageEvent_MassSendJobFinish requestMessage)
        {

            CallZero("v1/event/massSend", requestMessage);
            return new ResponseMessageText();
        }


        /// <summary>
        /// 退订
        /// 实际上用户无法收到非订阅账号的消息，所以这里可以随便写。
        /// unsubscribe事件的意义在于及时删除网站应用中已经记录的OpenID绑定，消除冗余数据。并且关注用户流失的情况。
        /// </summary>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_UnsubscribeRequest(RequestMessageEvent_Unsubscribe requestMessage)
        {

            CallZero("v1/event/unsub", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 事件之扫码推事件(scancode_push)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_ScancodePushRequest(RequestMessageEvent_Scancode_Push requestMessage)
        {

            CallZero("v1/event/scanCode/push", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 事件之扫码推事件且弹出“消息接收中”提示框(scancode_waitmsg)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_ScancodeWaitmsgRequest(RequestMessageEvent_Scancode_Waitmsg requestMessage)
        {

            CallZero("v1/event/scanCode/wait", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 事件之弹出拍照或者相册发图（pic_photo_or_album）
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_PicPhotoOrAlbumRequest(RequestMessageEvent_Pic_Photo_Or_Album requestMessage)
        {

            CallZero("v1/event/picPhotoOrAlbum", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 事件之弹出系统拍照发图(pic_sysphoto)
        /// 实际测试时发现微信并没有推送RequestMessageEvent_Pic_Sysphoto消息，只能接收到用户在微信中发送的图片消息。
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_PicSysphotoRequest(RequestMessageEvent_Pic_Sysphoto requestMessage)
        {

            CallZero("v1/event/picSysphoto", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 事件之弹出微信相册发图器(pic_weixin)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_PicWeixinRequest(RequestMessageEvent_Pic_Weixin requestMessage)
        {

            CallZero("v1/event/pic/weixin", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 事件之弹出地理位置选择器（location_select）
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_LocationSelectRequest(RequestMessageEvent_Location_Select requestMessage)
        {

            CallZero("v1/event/loc/select", requestMessage);
            return new ResponseMessageText();
        }

        /// <summary>
        /// 事件之发送模板消息返回结果
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase OnEvent_TemplateSendJobFinishRequest(RequestMessageEvent_TemplateSendJobFinish requestMessage)
        {
            switch (requestMessage.Status)
            {
                case "success":
                    //发送成功
                    break;
                case "failed:user block":
                    //送达由于用户拒收（用户设置拒绝接收公众号消息）而失败
                    break;
                case "failed: system failed":
                    //送达由于其他原因失败
                    break;
                default:
                    throw new WeixinException("未知模板消息状态：" + requestMessage.Status);
            }


            CallZero("v1/event/tmp/sended", requestMessage);
            return new ResponseMessageText();
        }

        #region 微信认证事件推送

        public IResponseMessageBase OnEvent_QualificationVerifySuccessRequest(RequestMessageEvent_QualificationVerifySuccess requestMessage)
        {
            //以下方法可以强制定义返回的字符串值

            CallZero("v1/event/qualificationVerifySuccess", requestMessage);
            //return new ResponseMessageText();
            return new SuccessResponseMessage();//返回"success"字符串
        }

        #endregion
    }
}