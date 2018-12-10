/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc
    
    文件名：CustomMessageHandler_Events.cs
    文件功能描述：自定义MessageHandler
    
    
    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

//PDBMARK_FILE MP
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.Entities;
using Senparc.NeuChar.Entities;

namespace Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler
{
    /// <summary>
    /// 自定义MessageHandler
    /// </summary>
    public partial class CustomMessageHandler
    {
        public string ResultXml { get; set; }
        public override IResponseMessageBase OnTextOrEventRequest(RequestMessageText requestMessage)
        {
            return null;//返回null，则继续执行OnTextRequest或OnEventRequest
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ClickRequest(RequestMessageEvent_Click requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/click", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 进入事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_EnterRequest(RequestMessageEvent_Enter requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/enter", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 位置事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_LocationRequest(RequestMessageEvent_Location requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/local", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 通过二维码扫描关注扫描事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ScanRequest(RequestMessageEvent_Scan requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/scan", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 打开网页事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ViewRequest(RequestMessageEvent_View requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/view", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 群发完成事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_MassSendJobFinishRequest(RequestMessageEvent_MassSendJobFinish requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/massSend", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 订阅（关注）事件
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/sub", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 退订
        /// 实际上用户无法收到非订阅账号的消息，所以这里可以随便写。
        /// unsubscribe事件的意义在于及时删除网站应用中已经记录的OpenID绑定，消除冗余数据。并且关注用户流失的情况。
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_UnsubscribeRequest(RequestMessageEvent_Unsubscribe requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/unsub", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 事件之扫码推事件(scancode_push)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ScancodePushRequest(RequestMessageEvent_Scancode_Push requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/scanCode/push", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 事件之扫码推事件且弹出“消息接收中”提示框(scancode_waitmsg)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ScancodeWaitmsgRequest(RequestMessageEvent_Scancode_Waitmsg requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/scanCode/wait", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 事件之弹出拍照或者相册发图（pic_photo_or_album）
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_PicPhotoOrAlbumRequest(RequestMessageEvent_Pic_Photo_Or_Album requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/picPhotoOrAlbum", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 事件之弹出系统拍照发图(pic_sysphoto)
        /// 实际测试时发现微信并没有推送RequestMessageEvent_Pic_Sysphoto消息，只能接收到用户在微信中发送的图片消息。
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_PicSysphotoRequest(RequestMessageEvent_Pic_Sysphoto requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/picSysphoto", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 事件之弹出微信相册发图器(pic_weixin)
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_PicWeixinRequest(RequestMessageEvent_Pic_Weixin requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/pic/weixin", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 事件之弹出地理位置选择器（location_select）
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_LocationSelectRequest(RequestMessageEvent_Location_Select requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/loc/select", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 事件之发送模板消息返回结果
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_TemplateSendJobFinishRequest(RequestMessageEvent_TemplateSendJobFinish requestMessage)
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

            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/tmp/sended", requestMessage);
            return defaultResponseMessage;
        }

        #region 微信认证事件推送

        public override IResponseMessageBase OnEvent_QualificationVerifySuccessRequest(RequestMessageEvent_QualificationVerifySuccess requestMessage)
        {
            //以下方法可以强制定义返回的字符串值
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/event/qualificationVerifySuccess", requestMessage);
            //return defaultResponseMessage;
            return new SuccessResponseMessage();//返回"success"字符串
        }

        #endregion
    }
}