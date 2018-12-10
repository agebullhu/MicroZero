using System.Collections.Generic;
using System.IO;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Senparc.NeuChar.Entities;
using Newtonsoft.Json;
using ZeroNet.Http.Gateway;

namespace Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler
{
    /// <summary>
    /// 自定义MessageHandler
    /// 把MessageHandler作为基类，重写对应请求的处理方法
    /// </summary>
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private string CallZero<T>(string command, T argument)
        {
            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                var caller = new BridgeCaller
                {
                    Station = "Weixin",
                    Commmand = command,
                    Argument = JsonConvert.SerializeObject(argument)
                };
                caller.CallCommand();
                var resultMessage = caller.State < ZeroOperatorStateType.Failed ? (caller.Result ?? "") : "error";
                LogRecorder.MonitorTrace($"State : {caller.State}");
                return resultMessage;
            }
        }

        /// <summary>
        /// 模板消息集合（Key：checkCode，Value：OpenId）
        /// </summary>
        public static Dictionary<string, string> TemplateMessageCollection = new Dictionary<string, string>();

        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalGlobalMessageContext.ExpireMinutes = 3。
            GlobalMessageContext.ExpireMinutes = 3;


            //在指定条件下，不使用消息去重
            OmitRepeatedMessageFunc = requestMessage => !(requestMessage is RequestMessageText);
        }

        //public CustomMessageHandler(RequestMessageBase requestMessage, PostModel postModel)
        //    : base(requestMessage, postModel)
        //{
        //}

        public override void OnExecuting()
        {
            //测试MessageContext.StorageData
            if (CurrentMessageContext.StorageData == null)
            {
                CurrentMessageContext.StorageData = 0;
            }
            base.OnExecuting();
        }

        public override void OnExecuted()
        {
            base.OnExecuted();
            CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData) + 1;
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/text", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/local", requestMessage);
            return defaultResponseMessage;
        }

        public override IResponseMessageBase OnShortVideoRequest(RequestMessageShortVideo requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/shortvedio", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/image", requestMessage);
            return defaultResponseMessage;
        }

        /// <summary>
        /// 处理语音请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/voice", requestMessage);
            return defaultResponseMessage;
        }
        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/vedio", requestMessage);
            return defaultResponseMessage;
        }


        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/link", requestMessage);
            return defaultResponseMessage;
        }

        public override IResponseMessageBase OnFileRequest(RequestMessageFile requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/file", requestMessage);
            return defaultResponseMessage;
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/def", requestMessage);
            return defaultResponseMessage;
        }


        public override IResponseMessageBase OnUnknownTypeRequest(RequestMessageUnknownType requestMessage)
        {
            var defaultResponseMessage = CreateResponseMessage<ResponseMessageText>();
            ResultXml = CallZero("v1/msg/unknown", requestMessage);
            return defaultResponseMessage;
        }
    }
}
