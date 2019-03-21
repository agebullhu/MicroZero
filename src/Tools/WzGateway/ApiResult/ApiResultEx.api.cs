using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

using Agebull.Common;
using Agebull.Common.DataModel;
using Gboxt.Common.DataModel;
using Agebull.Common.WebApi;



namespace Xuhui.Internetpro.WzHealthCardService
{
    partial class ApiResultEx : IApiResultData, IApiArgument
    {
        #region 数据校验
        /// <summary>数据校验</summary>
        /// <param name="message">返回的消息</param>
        /// <returns>成功则返回真</returns>
        public bool Validate(out string message)
        {
            var result = Validate();
            message = result.succeed ? null : result.Items.Where(p => !p.succeed).Select(p => $"{p.Caption}:{ p.Message}").LinkToString(';');
            return true;//result.succeed;
        }

        /// <summary>
        /// 扩展校验
        /// </summary>
        /// <param name="result">结果存放处</param>
        partial void ValidateEx(ValidateResult result);

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <returns>数据校验对象</returns>
        public ValidateResult Validate()
        {
            var result = new ValidateResult();
            if (string.IsNullOrWhiteSpace(TradeCode))
                result.AddNoEmpty("交易码", nameof(TradeCode));
            else
            {
                if (TradeCode.Length > 200)
                    result.Add("交易码", nameof(TradeCode), $"不能多于200个字");
            }
            if (string.IsNullOrWhiteSpace(RequestId))
                result.AddNoEmpty("请求唯一标识符", nameof(RequestId));
            else
            {
                if (RequestId.Length > 200)
                    result.Add("请求唯一标识符", nameof(RequestId), $"不能多于200个字");
            }
            if (string.IsNullOrWhiteSpace(Extend))
                result.AddNoEmpty("透传参数", nameof(Extend));
            else
            {
                if (Extend.Length > 200)
                    result.Add("透传参数", nameof(Extend), $"不能多于200个字");
            }
            if (string.IsNullOrWhiteSpace(Msg))
                result.AddNoEmpty("状态消息", nameof(Msg));
            else
            {
                if (Msg.Length > 200)
                    result.Add("状态消息", nameof(Msg), $"不能多于200个字");
            }
            ValidateEx(result);
            return result;
        }
        #endregion
    }
}
