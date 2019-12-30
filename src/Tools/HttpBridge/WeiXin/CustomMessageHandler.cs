using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.NeuChar.Entities;
using System.Xml.Linq;
using Newtonsoft.Json;
using Senparc.NeuChar;
using Senparc.NeuChar.Context;
using Senparc.Weixin.Tencent;
using Senparc.NeuChar.Exceptions;

namespace Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler
{
    /// <summary>
    /// 自定义MessageHandler
    /// 把MessageHandler作为基类，重写对应请求的处理方法
    /// </summary>
    public partial class CustomMessageHandler
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="postModel"></param>
        /// <returns></returns>
        public static IRequestMessageBase GetRequestEntity(string xml, PostModel postModel)
        {
            XDocument xDocument = XDocument.Parse(xml);

            XDocument decryptDoc = xDocument;
            var encrypt = xDocument.Root?.Element("Encrypt")?.Value;
            bool UsingEcryptMessage = false;
            if (!string.IsNullOrWhiteSpace(postModel?.Token) && !string.IsNullOrEmpty(encrypt))
            {
                //使用了加密
                UsingEcryptMessage = true;

                WXBizMsgCrypt msgCrype = new WXBizMsgCrypt(postModel.Token, postModel.EncodingAESKey, postModel.AppId);
                string msgXml = null;
                var result = msgCrype.DecryptMsg(postModel.Msg_Signature, postModel.Timestamp, postModel.Nonce, xml, ref msgXml);

                //判断result类型
                if (result != 0)
                {
                    //验证没有通过，取消执行
                    return null;
                }

                decryptDoc = XDocument.Parse(msgXml);//完成解密
            }

            var requestMessage = RequestMessageFactory.GetRequestEntity(decryptDoc, xml);
            if (UsingEcryptMessage)
            {
                requestMessage.Encrypt = encrypt;
            }
            return requestMessage;
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="v"></param>
        public IResponseMessageBase Handler(IRequestMessageBase v)
        {
            switch (v.MsgType)
            {
                case RequestMsgType.Text:
                    return OnTextRequest(v as RequestMessageText);
                case RequestMsgType.Location:
                    return OnLocationRequest(v as RequestMessageLocation);

                case RequestMsgType.Image:
                    return OnImageRequest(v as RequestMessageImage);

                case RequestMsgType.Voice:
                    return OnVoiceRequest(v as RequestMessageVoice);

                case RequestMsgType.Video:
                    return OnVideoRequest(v as RequestMessageVideo);

                case RequestMsgType.Link:
                    return OnLinkRequest(v as RequestMessageLink);

                case RequestMsgType.ShortVideo:
                    return OnShortVideoRequest(v as RequestMessageShortVideo);

                case RequestMsgType.File:
                    return OnFileRequest(v as RequestMessageFile);

                case RequestMsgType.Unknown:
                    return OnUnknownTypeRequest(v as RequestMessageUnknownType);

                case RequestMsgType.Event:
                    switch ((v as RequestMessageEvent)?.EventName)
                    {
                        case "ANNUAL_RENEW":
                            return OnEventRequest(v as RequestMessageEvent_AnnualRenew);

                        case "CARD_NOT_PASS_CHECK":
                            return OnEventRequest(v as RequestMessageEvent_Card_Not_Pass_Check);

                        case "CARD_PASS_CHECK":
                            return OnEventRequest(v as RequestMessageEvent_Card_Pass_Check);

                        case "CARD_PAY_ORDER":
                            return OnEventRequest(v as RequestMessageEvent_Card_Pay_Order);

                        case "CARD_SKU_REMIND":
                            return OnEventRequest(v as RequestMessageEvent_Card_Sku_Remind);

                        case "CLICK":
                            return OnEventRequest(v as RequestMessageEvent_Click);

                        case "ENTER":
                            return OnEventRequest(v as RequestMessageEvent_Enter);

                        case "GIFTCARD_PAY_DONE":
                            return OnEventRequest(v as RequestMessageEvent_GiftCard_Pay_Done);

                        case "GIFTCARD_SEND_TO_FRIEND":
                            return OnEventRequest(v as RequestMessageEvent_GiftCard_Send_To_Friend);

                        case "GIFTCARD_USER_ACCEPT":
                            return OnEventRequest(v as RequestMessageEvent_GiftCard_User_Accept);

                        case "KF_CLOSE_SESSION":
                            return OnEventRequest(v as RequestMessageEvent_Kf_Close_Session);

                        case "KF_CREATE_SESSION":
                            return OnEventRequest(v as RequestMessageEvent_Kf_Create_Session);

                        case "KF_SWITCH_SESSION":
                            return OnEventRequest(v as RequestMessageEvent_Kf_Switch_Session);

                        case "LOCATION":
                            return OnEventRequest(v as RequestMessageEvent_Location);

                        case "LOCATION_SELECT":
                            return OnEventRequest(v as RequestMessageEvent_Location_Select);

                        case "MASSSENDJOBFINISH":
                            return OnEventRequest(v as RequestMessageEvent_MassSendJobFinish);

                        case "MERCHANT_ORDER":
                            return OnEventRequest(v as RequestMessageEvent_Merchant_Order);

                        case "NAMING_VERIFY_FAIL":
                            return OnEventRequest(v as RequestMessageEvent_NamingVerifyFail);

                        case "NAMING_VERIFY_SUCCESS":
                            return OnEventRequest(v as RequestMessageEvent_NamingVerifySuccess);

                        case "PIC_PHOTO_OR_ALBUM":
                            return OnEventRequest(v as RequestMessageEvent_Pic_Photo_Or_Album);

                        case "PIC_SYSPHOTO":
                            return OnEventRequest(v as RequestMessageEvent_Pic_Sysphoto);

                        case "PIC_WEIXIN":
                            return OnEventRequest(v as RequestMessageEvent_Pic_Weixin);

                        case "POI_CHECK_NOTIFY":
                            return OnEventRequest(v as RequestMessageEvent_Poi_Check_Notify);

                        case "QUALIFICATION_VERIFY_FAIL":
                            return OnEventRequest(v as RequestMessageEvent_QualificationVerifyFail);

                        case "QUALIFICATION_VERIFY_SUCCESS":
                            return OnEventRequest(v as RequestMessageEvent_QualificationVerifySuccess);

                        case "SCAN":
                            return OnEventRequest(v as RequestMessageEvent_Scan);

                        case "SCANCODE_PUSH":
                            return OnEventRequest(v as RequestMessageEvent_Scancode_Push);

                        case "SCANCODE_WAITMSG":
                            return OnEventRequest(v as RequestMessageEvent_Scancode_Waitmsg);

                        case "SHAKEAROUNDUSERSHAKE":
                            return OnEventRequest(v as RequestMessageEvent_ShakearoundUserShake);

                        case "SUBMIT_MEMBERCARD_USER_INFO":
                            return OnEventRequest(v as RequestMessageEvent_Submit_Membercard_User_Info);

                        case "SUBSCRIBE":
                            return OnEventRequest(v as RequestMessageEvent_Subscribe);

                        case "TEMPLATESENDJOBFINISH":
                            return OnEventRequest(
                                v as RequestMessageEvent_TemplateSendJobFinish);

                        case "UNSUBSCRIBE":
                            return OnEventRequest(v as RequestMessageEvent_Unsubscribe);

                        case "UPDATE_MEMBER_CARD":
                            return OnEventRequest(v as RequestMessageEvent_Update_Member_Card);

                        case "USER_CONSUME_CARD":
                            return OnEventRequest(v as RequestMessageEvent_User_Consume_Card);

                        case "USER_DEL_CARD":
                            return OnEventRequest(v as RequestMessageEvent_User_Del_Card);

                        case "USER_ENTER_SESSION_FROM_CARD":
                            return OnEventRequest(v as RequestMessageEvent_User_Enter_Session_From_Card);

                        case "USER_GET_CARD":
                            return OnEventRequest(
                                v as RequestMessageEvent_User_Get_Card);

                        case "USER_GIFTING_CARD":
                            return OnEventRequest(
                                v as RequestMessageEvent_User_Gifting_Card);

                        case "USER_PAY_FROM_PAY_CELL":
                            return OnEventRequest(
                                v as RequestMessageEvent_User_Pay_From_Pay_Cell);

                        case "USER_VIEW_CARD":
                            return OnEventRequest(
                                v as RequestMessageEvent_User_View_Card);

                        case "VERIFY_EXPIRED":
                            return OnEventRequest(
                                v as RequestMessageEvent_VerifyExpired);

                        case "VIEW":
                            return OnEventRequest(v as RequestMessageEvent_View);

                        case "WEAPP_AUDIT_FAIL":
                            return OnEventRequest(
                                v as RequestMessageEvent_WeAppAuditFail);

                        case "WEAPP_AUDIT_SUCCESS":
                            return OnEventRequest(
                                v as RequestMessageEvent_WeAppAuditSuccess);

                        case "WIFICONNECTED":
                            return OnEventRequest(
                                v as RequestMessageEvent_WifiConnected);

                        default:
                            return OnEventRequest(v as RequestMessageEventBase);
                    }
                default:
                    throw new UnknownRequestMsgTypeException("未知的MsgType请求类型", null);
            }
        }


        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="json"></param>
        public IResponseMessageBase Handler(RequestMsgType type, string json)
        {
            switch (type)
            {
                case RequestMsgType.Text:
                    return OnTextRequest(JsonConvert.DeserializeObject<RequestMessageText>(json));
                case RequestMsgType.Location:
                    return OnLocationRequest(JsonConvert.DeserializeObject<RequestMessageLocation>(json));
                    
                case RequestMsgType.Image:
                    return OnImageRequest(JsonConvert.DeserializeObject<RequestMessageImage>(json));
                    
                case RequestMsgType.Voice:
                    return OnVoiceRequest(JsonConvert.DeserializeObject<RequestMessageVoice>(json));
                    
                case RequestMsgType.Video:
                    return OnVideoRequest(JsonConvert.DeserializeObject<RequestMessageVideo>(json));
                    
                case RequestMsgType.Link:
                    return OnLinkRequest(JsonConvert.DeserializeObject<RequestMessageLink>(json));
                    
                case RequestMsgType.ShortVideo:
                    return OnShortVideoRequest(JsonConvert.DeserializeObject<RequestMessageShortVideo>(json));
                    
                case RequestMsgType.File:
                    return OnFileRequest(JsonConvert.DeserializeObject<RequestMessageFile>(json));
                    
                case RequestMsgType.Unknown:
                    return OnUnknownTypeRequest(JsonConvert.DeserializeObject<RequestMessageUnknownType>(json));

                case RequestMsgType.Event:
                    switch (JsonConvert.DeserializeObject<RequestMessageEvent>(json).EventName)
                    {
                        case "ANNUAL_RENEW":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_AnnualRenew>(json));

                        case "CARD_NOT_PASS_CHECK":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Card_Not_Pass_Check>(json));

                        case "CARD_PASS_CHECK":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Card_Pass_Check>(json));

                        case "CARD_PAY_ORDER":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Card_Pay_Order>(json));

                        case "CARD_SKU_REMIND":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Card_Sku_Remind>(json));

                        case "CLICK":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_Click>(json));

                        case "ENTER":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_Enter>(json));

                        case "GIFTCARD_PAY_DONE":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_GiftCard_Pay_Done>(json));

                        case "GIFTCARD_SEND_TO_FRIEND":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_GiftCard_Send_To_Friend>(json));

                        case "GIFTCARD_USER_ACCEPT":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_GiftCard_User_Accept>(json));

                        case "KF_CLOSE_SESSION":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Kf_Close_Session>(json));

                        case "KF_CREATE_SESSION":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Kf_Create_Session>(json));

                        case "KF_SWITCH_SESSION":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Kf_Switch_Session>(json));

                        case "LOCATION":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_Location>(json));

                        case "LOCATION_SELECT":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Location_Select>(json));

                        case "MASSSENDJOBFINISH":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_MassSendJobFinish>(json));

                        case "MERCHANT_ORDER":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Merchant_Order>(json));

                        case "NAMING_VERIFY_FAIL":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_NamingVerifyFail>(json));

                        case "NAMING_VERIFY_SUCCESS":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_NamingVerifySuccess>(json));

                        case "PIC_PHOTO_OR_ALBUM":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Pic_Photo_Or_Album>(json));

                        case "PIC_SYSPHOTO":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Pic_Sysphoto>(json));

                        case "PIC_WEIXIN":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_Pic_Weixin>(json));

                        case "POI_CHECK_NOTIFY":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Poi_Check_Notify>(json));

                        case "QUALIFICATION_VERIFY_FAIL":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_QualificationVerifyFail>(json));

                        case "QUALIFICATION_VERIFY_SUCCESS":
                            return OnEventRequest(JsonConvert
                                .DeserializeObject<RequestMessageEvent_QualificationVerifySuccess>(json));

                        case "SCAN":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_Scan>(json));

                        case "SCANCODE_PUSH":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Scancode_Push>(json));

                        case "SCANCODE_WAITMSG":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Scancode_Waitmsg>(json));

                        case "SHAKEAROUNDUSERSHAKE":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_ShakearoundUserShake>(json));

                        case "SUBMIT_MEMBERCARD_USER_INFO":
                            return OnEventRequest(JsonConvert
                                .DeserializeObject<RequestMessageEvent_Submit_Membercard_User_Info>(json));

                        case "SUBSCRIBE":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_Subscribe>(json));

                        case "TEMPLATESENDJOBFINISH":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_TemplateSendJobFinish>(json));

                        case "UNSUBSCRIBE":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_Unsubscribe>(json));

                        case "UPDATE_MEMBER_CARD":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_Update_Member_Card>(json));

                        case "USER_CONSUME_CARD":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_User_Consume_Card>(json));

                        case "USER_DEL_CARD":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_User_Del_Card>(json));

                        case "USER_ENTER_SESSION_FROM_CARD":
                            return OnEventRequest(JsonConvert
                                .DeserializeObject<RequestMessageEvent_User_Enter_Session_From_Card>(json));

                        case "USER_GET_CARD":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_User_Get_Card>(json));

                        case "USER_GIFTING_CARD":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_User_Gifting_Card>(json));

                        case "USER_PAY_FROM_PAY_CELL":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_User_Pay_From_Pay_Cell>(json));

                        case "USER_VIEW_CARD":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_User_View_Card>(json));

                        case "VERIFY_EXPIRED":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_VerifyExpired>(json));

                        case "VIEW":
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEvent_View>(json));

                        case "WEAPP_AUDIT_FAIL":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_WeAppAuditFail>(json));

                        case "WEAPP_AUDIT_SUCCESS":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_WeAppAuditSuccess>(json));

                        case "WIFICONNECTED":
                            return OnEventRequest(
                                JsonConvert.DeserializeObject<RequestMessageEvent_WifiConnected>(json));

                        default:
                            return OnEventRequest(JsonConvert.DeserializeObject<RequestMessageEventBase>(json));
                    }
                default:
                    throw new UnknownRequestMsgTypeException("未知的MsgType请求类型", null);
            }
        }

        /// <summary>
        /// Event事件类型请求
        /// </summary>
        public IResponseMessageBase OnEventRequest(RequestMessageEventBase requestMessage)
        {
            switch (requestMessage.Event)
            {
                case Event.ENTER:
                    return OnEvent_EnterRequest(requestMessage as RequestMessageEvent_Enter);
                    
                case Event.LOCATION://自动发送的用户当前位置
                    return OnEvent_LocationRequest(requestMessage as RequestMessageEvent_Location);
                    
                case Event.subscribe://订阅
                    return OnEvent_SubscribeRequest(requestMessage as RequestMessageEvent_Subscribe);
                    
                case Event.unsubscribe://退订
                    return OnEvent_UnsubscribeRequest(requestMessage as RequestMessageEvent_Unsubscribe);
                    
                case Event.CLICK://菜单点击
                    return OnEvent_ClickRequest(requestMessage as RequestMessageEvent_Click);
                    
                case Event.scan://二维码
                    return OnEvent_ScanRequest(requestMessage as RequestMessageEvent_Scan);
                    
                case Event.VIEW://URL跳转（view视图）
                    return OnEvent_ViewRequest(requestMessage as RequestMessageEvent_View);
                    
                case Event.MASSSENDJOBFINISH://群发消息成功
                    return OnEvent_MassSendJobFinishRequest(requestMessage as RequestMessageEvent_MassSendJobFinish);
                    
                case Event.TEMPLATESENDJOBFINISH://推送模板消息成功
                    return OnEvent_TemplateSendJobFinishRequest(requestMessage as RequestMessageEvent_TemplateSendJobFinish);
                    
                case Event.pic_photo_or_album://弹出拍照或者相册发图
                    return OnEvent_PicPhotoOrAlbumRequest(requestMessage as RequestMessageEvent_Pic_Photo_Or_Album);
                    
                case Event.scancode_push://扫码推事件
                    return OnEvent_ScancodePushRequest(requestMessage as RequestMessageEvent_Scancode_Push);
                    
                case Event.scancode_waitmsg://扫码推事件且弹出“消息接收中”提示框
                    return OnEvent_ScancodeWaitmsgRequest(requestMessage as RequestMessageEvent_Scancode_Waitmsg);
                    
                case Event.location_select://弹出地理位置选择器
                    return OnEvent_LocationSelectRequest(requestMessage as RequestMessageEvent_Location_Select);
                    
                case Event.pic_weixin://弹出微信相册发图器
                    return OnEvent_PicWeixinRequest(requestMessage as RequestMessageEvent_Pic_Weixin);
                    
                case Event.pic_sysphoto://弹出系统拍照发图
                    return OnEvent_PicSysphotoRequest(requestMessage as RequestMessageEvent_Pic_Sysphoto);
                    
                case Event.card_pass_check://卡券通过审核
                    return OnEvent_Card_Pass_CheckRequest(requestMessage as RequestMessageEvent_Card_Pass_Check);
                    
                case Event.card_not_pass_check://卡券未通过审核
                    return OnEvent_Card_Not_Pass_CheckRequest(requestMessage as RequestMessageEvent_Card_Not_Pass_Check);
                    
                case Event.user_get_card://领取卡券
                    return OnEvent_User_Get_CardRequest(requestMessage as RequestMessageEvent_User_Get_Card);
                    
                case Event.user_del_card://删除卡券
                    return OnEvent_User_Del_CardRequest(requestMessage as RequestMessageEvent_User_Del_Card);
                    
                case Event.kf_create_session://多客服接入会话
                    return OnEvent_Kf_Create_SessionRequest(requestMessage as RequestMessageEvent_Kf_Create_Session);
                    
                case Event.kf_close_session://多客服关闭会话
                    return OnEvent_Kf_Close_SessionRequest(requestMessage as RequestMessageEvent_Kf_Close_Session);
                    
                case Event.kf_switch_session://多客服转接会话
                    return OnEvent_Kf_Switch_SessionRequest(requestMessage as RequestMessageEvent_Kf_Switch_Session);
                    
                case Event.poi_check_notify://审核结果事件推送
                    return OnEvent_Poi_Check_NotifyRequest(requestMessage as RequestMessageEvent_Poi_Check_Notify);
                    
                case Event.WifiConnected://Wi-Fi连网成功
                    return OnEvent_WifiConnectedRequest(requestMessage as RequestMessageEvent_WifiConnected);
                    
                case Event.user_consume_card://卡券核销
                    return OnEvent_User_Consume_CardRequest(requestMessage as RequestMessageEvent_User_Consume_Card);
                    
                case Event.user_enter_session_from_card://从卡券进入公众号会话
                    return OnEvent_User_Enter_Session_From_CardRequest(requestMessage as RequestMessageEvent_User_Enter_Session_From_Card);
                    
                case Event.user_view_card://进入会员卡
                    return OnEvent_User_View_CardRequest(requestMessage as RequestMessageEvent_User_View_Card);
                    
                case Event.merchant_order://微小店订单付款通知
                    return OnEvent_Merchant_OrderRequest(requestMessage as RequestMessageEvent_Merchant_Order);
                    
                case Event.submit_membercard_user_info://接收会员信息事件通知
                    return OnEvent_Submit_Membercard_User_InfoRequest(requestMessage as RequestMessageEvent_Submit_Membercard_User_Info);
                    
                case Event.ShakearoundUserShake://摇一摇事件通知
                    return OnEvent_ShakearoundUserShakeRequest(requestMessage as RequestMessageEvent_ShakearoundUserShake);
                    
                case Event.user_gifting_card://卡券转赠事件推送
                    return OnEvent_User_Gifting_CardRequest(requestMessage as RequestMessageEvent_User_Gifting_Card);
                    
                case Event.user_pay_from_pay_cell://微信买单完成
                    return OnEvent_User_Pay_From_Pay_CellRequest(requestMessage as RequestMessageEvent_User_Pay_From_Pay_Cell);
                    
                case Event.update_member_card://会员卡内容更新事件：会员卡积分余额发生变动时
                    return OnEvent_Update_Member_CardRequest(requestMessage as RequestMessageEvent_Update_Member_Card);
                    
                case Event.card_sku_remind://卡券库存报警事件：当某个card_id的初始库存数大于200且当前库存小于等于100时
                    return OnEvent_Card_Sku_RemindRequest(requestMessage as RequestMessageEvent_Card_Sku_Remind);
                    
                case Event.card_pay_order://券点流水详情事件：当商户朋友的券券点发生变动时
                    return OnEvent_Card_Pay_OrderRequest(requestMessage as RequestMessageEvent_Card_Pay_Order);
                    

                #region 卡券回调

                case Event.giftcard_pay_done:
                    return OnEvent_GiftCard_Pay_DoneRequest(requestMessage as RequestMessageEvent_GiftCard_Pay_Done);
                    

                case Event.giftcard_send_to_friend:
                    return OnEvent_GiftCard_Send_To_FriendRequest(requestMessage as RequestMessageEvent_GiftCard_Send_To_Friend);
                    

                case Event.giftcard_user_accept:
                    return OnEvent_GiftCard_User_AcceptRequest(requestMessage as RequestMessageEvent_GiftCard_User_Accept);
                    

                #endregion

                #region 微信认证事件推送

                case Event.qualification_verify_success://资质认证成功（此时立即获得接口权限）
                    return OnEvent_QualificationVerifySuccessRequest(requestMessage as RequestMessageEvent_QualificationVerifySuccess);
                    
                case Event.qualification_verify_fail://资质认证失败
                    return OnEvent_QualificationVerifyFailRequest(requestMessage as RequestMessageEvent_QualificationVerifyFail);
                    
                case Event.naming_verify_success://名称认证成功（即命名成功）
                    return OnEvent_NamingVerifySuccessRequest(requestMessage as RequestMessageEvent_NamingVerifySuccess);
                    
                case Event.naming_verify_fail://名称认证失败（这时虽然客户端不打勾，但仍有接口权限）
                    return OnEvent_NamingVerifyFailRequest(requestMessage as RequestMessageEvent_NamingVerifyFail);
                    
                case Event.annual_renew://年审通知
                    return OnEvent_AnnualRenewRequest(requestMessage as RequestMessageEvent_AnnualRenew);
                    
                case Event.verify_expired://认证过期失效通知
                    return OnEvent_VerifyExpiredRequest(requestMessage as RequestMessageEvent_VerifyExpired);
                    
                #endregion

                #region 小程序审核事件推送

                case Event.weapp_audit_success://
                    return OnEvent_WeAppAuditSuccessRequest(requestMessage as RequestMessageEvent_WeAppAuditSuccess);
                    
                case Event.weapp_audit_fail://
                    return OnEvent_WeAppAuditFailRequest(requestMessage as RequestMessageEvent_WeAppAuditFail);
                    
                #endregion
                default:
                    throw new UnknownRequestMsgTypeException("未知的Event下属请求信息", null);
            }
        }

        #region Event下属分类，接收事件方法


        /// <summary>
        /// 卡券通过审核
        /// </summary>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_Card_Pass_CheckRequest(RequestMessageEvent_Card_Pass_Check requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 卡券未通过审核
        /// </summary>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_Card_Not_Pass_CheckRequest(RequestMessageEvent_Card_Not_Pass_Check requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 领取卡券
        /// </summary>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_User_Get_CardRequest(RequestMessageEvent_User_Get_Card requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 删除卡券
        /// </summary>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_User_Del_CardRequest(RequestMessageEvent_User_Del_Card requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 多客服接入会话
        /// </summary>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_Kf_Create_SessionRequest(RequestMessageEvent_Kf_Create_Session requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 多客服关闭会话
        /// </summary>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_Kf_Close_SessionRequest(RequestMessageEvent_Kf_Close_Session requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 多客服转接会话
        /// </summary>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_Kf_Switch_SessionRequest(RequestMessageEvent_Kf_Switch_Session requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之审核结果事件推送
        /// </summary>
        public virtual IResponseMessageBase OnEvent_Poi_Check_NotifyRequest(RequestMessageEvent_Poi_Check_Notify requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之Wi-Fi连网成功
        /// </summary>
        public virtual IResponseMessageBase OnEvent_WifiConnectedRequest(RequestMessageEvent_WifiConnected requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之卡券核销
        /// </summary>
        public virtual IResponseMessageBase OnEvent_User_Consume_CardRequest(RequestMessageEvent_User_Consume_Card requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之从卡券进入公众号会话
        /// </summary>
        public virtual IResponseMessageBase OnEvent_User_Enter_Session_From_CardRequest(RequestMessageEvent_User_Enter_Session_From_Card requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之进入会员卡
        /// </summary>
        public virtual IResponseMessageBase OnEvent_User_View_CardRequest(RequestMessageEvent_User_View_Card requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之微小店订单付款通知
        /// </summary>
        public virtual IResponseMessageBase OnEvent_Merchant_OrderRequest(RequestMessageEvent_Merchant_Order requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之接收会员信息事件通知
        /// </summary>
        public virtual IResponseMessageBase OnEvent_Submit_Membercard_User_InfoRequest(RequestMessageEvent_Submit_Membercard_User_Info requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之摇一摇事件通知
        /// </summary>
        public virtual IResponseMessageBase OnEvent_ShakearoundUserShakeRequest(RequestMessageEvent_ShakearoundUserShake requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
            //return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 卡券转赠事件推送
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_User_Gifting_CardRequest(RequestMessageEvent_User_Gifting_Card requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }
        /// <summary>
        /// 微信买单完成
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_User_Pay_From_Pay_CellRequest(RequestMessageEvent_User_Pay_From_Pay_Cell requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }
        /// <summary>
        /// 会员卡内容更新事件：会员卡积分余额发生变动时
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_Update_Member_CardRequest(RequestMessageEvent_Update_Member_Card requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }
        /// <summary>
        /// 卡券库存报警事件：当某个card_id的初始库存数大于200且当前库存小于等于100时
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_Card_Sku_RemindRequest(RequestMessageEvent_Card_Sku_Remind requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }
        /// <summary>
        /// 券点流水详情事件：当商户朋友的券券点发生变动时
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_Card_Pay_OrderRequest(RequestMessageEvent_Card_Pay_Order requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        #region 微信认证事件推送


        /// <summary>
        /// 资质认证失败
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_QualificationVerifyFailRequest(RequestMessageEvent_QualificationVerifyFail requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }


        /// <summary>
        /// 名称认证成功（即命名成功）
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_NamingVerifySuccessRequest(RequestMessageEvent_NamingVerifySuccess requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 名称认证失败（这时虽然客户端不打勾，但仍有接口权限）
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_NamingVerifyFailRequest(RequestMessageEvent_NamingVerifyFail requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 年审通知
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_AnnualRenewRequest(RequestMessageEvent_AnnualRenew requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 认证过期失效通知
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_VerifyExpiredRequest(RequestMessageEvent_VerifyExpired requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        #endregion

        #region 小程序审核事件推送

        /// <summary>
        /// 小程序审核失败通知
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_WeAppAuditFailRequest(RequestMessageEvent_WeAppAuditFail requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }
        /// <summary>
        /// 小程序审核成功通知
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual IResponseMessageBase OnEvent_WeAppAuditSuccessRequest(RequestMessageEvent_WeAppAuditSuccess requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        #endregion

        #region 卡券回调

        /// <summary>
        /// 用户购买礼品卡付款成功
        /// </summary>
        public virtual IResponseMessageBase OnEvent_GiftCard_Pay_DoneRequest(RequestMessageEvent_GiftCard_Pay_Done requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 用户购买后赠送
        /// </summary>
        public virtual IResponseMessageBase OnEvent_GiftCard_Send_To_FriendRequest(RequestMessageEvent_GiftCard_Send_To_Friend requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 用户领取礼品卡成功
        /// </summary>
        public virtual IResponseMessageBase OnEvent_GiftCard_User_AcceptRequest(RequestMessageEvent_GiftCard_User_Accept requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }


        #endregion

        #endregion
    }
}

