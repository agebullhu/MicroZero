using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.OAuth;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZeroNet.Http.Gateway;
using ZeroNet.Http.Route;

namespace Xuhui.Internetpro.WzHealthCardService.Gateway
{
    /// <summary>
    ///     安全检查员
    /// </summary>
    internal class SecurityChecker
    {
        #region 变量


        //private static readonly Dictionary<string, bool> Keys = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     数据
        /// </summary>
        public RouteData Data { get; set; }

        /// <summary>
        ///     数据
        /// </summary>
        public ApiArgument Argument { get; set; }

        private static readonly string UnknowDeviceJson = JsonConvert.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.Auth_Device_Unknow));

        private static readonly string UnknowAccessTokenJson = JsonConvert.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.Auth_AccessToken_Unknow));
        private static readonly string AccessTokenTimeOutJson = JsonConvert.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.Auth_AccessToken_TimeOut));
        private static readonly IApiResult<LoginUserInfo> DenyAccessResult = ApiResultIoc.Ioc.Error<LoginUserInfo>(ErrorCode.DenyAccess, null, null, "gateway", null, null);

        #endregion

        #region 预检

        /// <summary>
        ///     预检
        /// </summary>
        /// <returns></returns>
        public bool PreCheck()
        {
            if (!Argument.Header.Validate(out var msg))
            {
                Data.UserState = UserOperatorStateType.LogicalError;
                Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Data.ResultMessage = JsonConvert.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.LogicalError, msg));
                return false;
            }
            if (!CheckSign())
            {
                Data.UserState = UserOperatorStateType.LogicalError;
                Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Data.ResultMessage = JsonConvert.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.LogicalError, "签名错误"));
                return false;
            }
            if (!KillDenyHttpHeaders())
            {
                Data.UserState = UserOperatorStateType.DenyAccess;
                Data.ZeroState = ZeroOperatorStateType.DenyAccess;
                Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                return false;
            }
            return true;
        }

        /// <summary>
        ///     验签
        /// </summary>
        /// <returns></returns>
        private bool CheckSign()
        {
            var str = $"{Argument.Header.OrganizationId}{Argument.Header.AppId}{Argument.Header.Token}{Argument.Header.DataSources}{Argument.Header.TradeCode}{Argument.Header.RequestId}{Argument.Header.RequestTime}";
            MD5 md5 = new MD5CryptoServiceProvider();
            var inputBytes = Encoding.ASCII.GetBytes(str);
            var result = md5.ComputeHash(inputBytes);
            var j = result.Length;
            var sign = new StringBuilder();
            char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            for (var i = 0; i < j; i++)
            {
                sign.Append(hexDigits[result[i] >> 4 & 0xF]);
                sign.Append(hexDigits[result[i] & 0xF]);
            }
            return sign.ToString() == Argument.Header.Sign;
        }

        /// <summary>
        ///     针对HttpHeader特征阻止不安全访问
        /// </summary>
        /// <returns></returns>
        private bool KillDenyHttpHeaders()
        {
            try
            {
                foreach (var head in RouteOption.Option.Security.DenyHttpHeaders)
                {
                    if (!Data.Headers.ContainsKey(head.Head))
                        continue;
                    switch (head.DenyType)
                    {
                        case DenyType.Hase:
                            if (Data.Headers.ContainsKey(head.Head))
                                return false;
                            break;
                        case DenyType.NonHase:
                            if (!Data.Headers.ContainsKey(head.Head))
                                return false;
                            break;
                        case DenyType.Count:
                            if (!Data.Headers.ContainsKey(head.Head))
                                break;
                            if (Data.Headers[head.Head].Count == int.Parse(head.Value))
                                return false;
                            break;
                        case DenyType.Equals:
                            if (!Data.Headers.ContainsKey(head.Head))
                                break;
                            if (string.Equals(Data.Headers[head.Head].ToString(), head.Value,
                                StringComparison.OrdinalIgnoreCase))
                                return false;
                            break;
                        case DenyType.Like:
                            if (!Data.Headers.ContainsKey(head.Head))
                                break;
                            if (Data.Headers[head.Head].ToString().Contains(head.Value))
                                return false;
                            break;
                        case DenyType.Regex:
                            if (!Data.Headers.ContainsKey(head.Head))
                                break;
                            var regx = new Regex(head.Value,
                                RegexOptions.IgnoreCase | RegexOptions.ECMAScript | RegexOptions.Multiline);
                            if (regx.IsMatch(Data.Headers[head.Head].ToString()))
                                return false;
                            break;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return true;
            }
        }

        #endregion

        #region 令牌检查

        /// <summary>
        ///     检查特定操作系统与App的适用性
        /// </summary>
        /// <returns></returns>
        private bool CheckApisInner()
        {
            //var header = Request.Headers.Values.LinkToString(" ");
            //if (string.IsNullOrWhiteSpace(header) || header.Contains("iToolsVM"))
            //    return false;
            if (!RouteOption.Option.SystemConfig.CheckApiItem || Data.ApiItem == null)
                return true;
            //OS匹配
            if (!string.IsNullOrWhiteSpace(Data.ApiItem.Os) && Data.Token.IndexOf(Data.ApiItem.Os, StringComparison.OrdinalIgnoreCase) < 0)
                return false;
            //APP匹配
            if (!string.IsNullOrWhiteSpace(Data.ApiItem.App) && Data.Token.IndexOf(Data.ApiItem.App, StringComparison.OrdinalIgnoreCase) < 0)
                return false;
            return true;
        }

        /// <summary>
        ///     执行检查
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空或不合格
        ///     2：令牌是伪造的
        /// </returns>
        public bool CheckToken()
        {
            try
            {
                if (!RouteOption.Option.Security.CheckBearer)
                {
                    GlobalContext.SetUser(LoginUserInfo.CreateAnymouse(Data.Token, "*", "*"));
                    return true;
                }
                if (!CheckApisInner())
                {
                    Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Data.Token))
                {
                    if (!RouteOption.Option.SystemConfig.CheckApiItem || Data.ApiItem == null || Data.ApiItem.NoBearer)
                    {
                        GlobalContext.SetUser(LoginUserInfo.CreateAnymouse(Data.Token, "*", "*"));
                        return true;
                    }
                    Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                    return false;
                }
                switch (Data.Token[0])
                {
                    case '*':
                    case '#':
                        break;
                    default:
                        Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                        return false;
                }
                if (RouteOption.Option.Security.DenyTokens.ContainsKey(Data.Token))
                {
                    Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                    return false;
                }
                for (var index = 1; index < Data.Token.Length; index++)
                {
                    var ch = Data.Token[index];
                    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z' || ch == '_')
                        continue;
                    LogRecorder.MonitorTrace("Token Layout Error");
                    Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                    return false;
                }

                IApiResult result;
                switch (Data.Token[0])
                {
                    default:
                        Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                        return false;
                    case '*':
                        if (Data.ApiItem != null && Data.ApiItem.NeedLogin)
                        {
                            Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                            return false;
                        }
                        result = CheckToken("DeviceId", RouteOption.Option.Security.DeviceIdCheckApi, out var vl);
                        if (result == null || result.Status.ErrorCode == ErrorCode.Auth_UnknowToken)
                        {
                            Data.ResultMessage = UnknowDeviceJson;
                            return false;
                        }
                        if (result.Success)
                        {
                            return true;
                        }
                        Data.ResultMessage = vl;
                        return false;
                    case '#':
                        result = CheckToken("AccessToken", RouteOption.Option.Security.AccessTokenCheckApi, out vl);
                        if (result == null || result.Status.ErrorCode == ErrorCode.Auth_UnknowToken)
                        {
                            Data.ResultMessage = UnknowAccessTokenJson;
                            return false;
                        }
                        if (result.Status.ErrorCode == ErrorCode.Auth_AccessToken_TimeOut)
                        {
                            Data.ResultMessage = AccessTokenTimeOutJson;
                            return false;
                        }
                        if (result.Success)
                        {
                            return true;
                        }
                        Data.ResultMessage = vl;
                        return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return true;
            }
        }
        private IApiResult<LoginUserInfo> CheckToken(string name, string api, out string json)
        {
            // 远程调用
            using (MonitorScope.CreateScope($"Check{name}:{Data.Token}"))
            {
                var caller = new ApiClient
                {
                    Simple = true,
                    Station = RouteOption.Option.Security.AuthStation,
                    Commmand = api,
                    Argument = $"{{\"Token\":\"{Data.Token}\"}}"
                };
                caller.CallCommand();

                json = caller.Result;

                if (caller.Result == null)
                    return null;
                //GlobalContext.Current.Request.Token = Data.Token;
                var result = ApiResultIoc.Ioc.DeserializeObject<LoginUserInfo>(caller.Result);
                if (result == null)
                    return DenyAccessResult;
                if (!result.Success)
                    return result;
                //形成透传上下文
                GlobalContext.SetOrganizational(new OrganizationalInfo
                {
                    OrgId = result.ResultData.OrganizationId,
                    Name = result.ResultData.Organization,
                    OrgKey = result.ResultData.Organization,
                    RouteName = result.ResultData.Organization
                });
                var context = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(GlobalContext.Current));
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                context["user"] = obj["data"];
                Data.GlobalContextJson = context.ToString();
                return result;
            }
        }
        #endregion

        #region 返回值检查

        /// <summary>
        ///     检查返回值是否合理
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool CheckResult(RouteData data)
        {
            if (data.UserState != UserOperatorStateType.Success || data.HostName == null)
                return false;
            if (string.IsNullOrWhiteSpace(data.ResultMessage))
            {
                IocHelper.Create<IRuntimeWaring>().Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                data.UserState = UserOperatorStateType.FormalError;
                data.ZeroState = ZeroOperatorStateType.FrameInvalid;
                return false;
            }

            var json = data.ResultMessage.Trim();
            switch (json[0])
            {
                case '{':
                case '[':
                    break;
                default:
                    IocHelper.Create<IRuntimeWaring>()?.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                    return true;
            }

            IApiResult result;
            try
            {
                result = ApiResultIoc.Ioc.DeserializeObject(data.ResultMessage);
                if (result == null)
                {
                    IocHelper.Create<IRuntimeWaring>()?.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                    data.UserState = UserOperatorStateType.FormalError;
                    data.ZeroState = ZeroOperatorStateType.Error;
                    return false;
                }
            }
            catch
            {
                IocHelper.Create<IRuntimeWaring>()?.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                data.UserState = UserOperatorStateType.FormalError;
                data.ZeroState = ZeroOperatorStateType.Error;
                return false;
            }

            if (result.Status == null || result.Success)
                return true;
            switch (result.Status.ErrorCode)
            {
                case ErrorCode.ReTry:
                case ErrorCode.Ignore:
                    data.UserState = UserOperatorStateType.NotReady;
                    data.ZeroState = ZeroOperatorStateType.LocalNoReady;
                    return false;
                case ErrorCode.LogicalError:
                    data.UserState = UserOperatorStateType.LogicalError;
                    data.ZeroState = ZeroOperatorStateType.LocalException;
                    return false;
                case ErrorCode.DenyAccess:
                case ErrorCode.Auth_RefreshToken_Unknow:
                case ErrorCode.Auth_ServiceKey_Unknow:
                case ErrorCode.Auth_AccessToken_Unknow:
                case ErrorCode.Auth_User_Unknow:
                case ErrorCode.Auth_Device_Unknow:
                case ErrorCode.Auth_AccessToken_TimeOut:
                    data.UserState = UserOperatorStateType.DenyAccess;
                    data.ZeroState = ZeroOperatorStateType.DenyAccess;
                    return false;
            }
            data.UserState = UserOperatorStateType.FormalError;
            data.ZeroState = ZeroOperatorStateType.FrameInvalid;
            IocHelper.Create<IRuntimeWaring>()?.Waring(data.HostName, data.ApiName, result.Status?.ClientMessage ?? "处理错误但无消息");
            return false;
        }


        #endregion
    }
}