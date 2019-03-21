using System;
using System.Linq;
using Gboxt.Common.DataModel;



namespace Xuhui.Internetpro.WzHealthCardService
{
    sealed partial class ApiArgumentHeader : IApiResultData , IApiArgument
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
            if(string.IsNullOrWhiteSpace(OrganizationId))
                 result.AddNoEmpty("organizationId", nameof(OrganizationId));
            else 
            {
                if(OrganizationId.Length > 64)
                    result.Add("organizationId", nameof(OrganizationId),"不能多于64个字");
            }
            if(string.IsNullOrWhiteSpace(AppId))
                 result.AddNoEmpty("appId", nameof(AppId));
            else 
            {
                if(AppId.Length > 16)
                    result.Add("appId", nameof(AppId),"不能多于16个字");
            }
            if(string.IsNullOrWhiteSpace(DataSources))
                 result.AddNoEmpty("dataSources", nameof(DataSources));
            else 
            {
                if(DataSources.Length > 2)
                    result.Add("dataSources", nameof(DataSources),"不能多于2个字");
            }
            if(string.IsNullOrWhiteSpace(TradeCode))
                 result.AddNoEmpty("tradeCode", nameof(TradeCode));
            else 
            {
                if(TradeCode.Length > 5)
                    result.Add("tradeCode", nameof(TradeCode),"不能多于5个字");
            }
            if(!string.IsNullOrWhiteSpace(Token))
            {
                if(Token.Length > 32)
                    result.Add("token", nameof(Token),"不能多于32个字");
            }
            if(string.IsNullOrWhiteSpace(RequestId))
                 result.AddNoEmpty("requestId", nameof(RequestId));
            else 
            {
                if(RequestId.Length > 32)
                    result.Add("requestId", nameof(RequestId),"不能多于32个字");
            }
            if(RequestTime == DateTime.MinValue)
                 result.AddNoEmpty("requestTime", nameof(RequestTime));
            if(!string.IsNullOrWhiteSpace(OperatorCode))
            {
                if(OperatorCode.Length > 20)
                    result.Add("operatorCode", nameof(OperatorCode),"不能多于20个字");
            }
            if(!string.IsNullOrWhiteSpace(OperatorName))
            {
                if(OperatorName.Length > 50)
                    result.Add("operatorName", nameof(OperatorName),"不能多于50个字");
            }
            if(!string.IsNullOrWhiteSpace(ClientIp))
            {
                if(ClientIp.Length > 15)
                    result.Add("否",nameof(ClientIp),"不能多于15个字");
            }
            if(!string.IsNullOrWhiteSpace(ClientMacAddress))
            {
                if(ClientMacAddress.Length > 17)
                    result.Add("clientMacAddress", nameof(ClientMacAddress),"不能多于17个字");
            }
            if(string.IsNullOrWhiteSpace(Sign))
                 result.AddNoEmpty("sign", nameof(Sign));
            else 
            {
                if(Sign.Length > 344)
                    result.Add("sign", nameof(Sign),"不能多于344个字");
            }
            if(!string.IsNullOrWhiteSpace(Extend))
            {
                if(Extend.Length > 100)
                    result.Add("extend", nameof(Extend),"不能多于100个字");
            }
            ValidateEx(result);
            return result;
        }
        #endregion
    }
}