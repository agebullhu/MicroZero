using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.OAuth;

using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;
using Agebull.EntityModel.Common;
using Agebull.MicroZero.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroZero.Http.Gateway
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

        private static readonly string UnknowDeviceJson = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.Auth_Device_Unknow));

        private static readonly string UnknowAccessTokenJson = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.Auth_AccessToken_Unknow));
        private static readonly string AccessTokenTimeOutJson = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.Auth_AccessToken_TimeOut));
        private static readonly IApiResult<LoginUserInfo> DenyAccessResult = ApiResultIoc.Ioc.Error<LoginUserInfo>(ErrorCode.DenyAccess, null, null, "gateway", null, null);

        #endregion

        #region 预检

        /// <summary>
        ///     预检
        /// </summary>
        /// <returns></returns>
        public bool PreCheck()
        {
            if (CheckSign() && KillDenyHttpHeaders())
                return true;
            Data.UserState = UserOperatorStateType.DenyAccess;
            Data.ZeroState = ZeroOperatorStateType.DenyAccess;
            Data.ResultMessage = ApiResultIoc.DenyAccessJson;
            return false;

        }

        /// <summary>
        ///     验签
        /// </summary>
        /// <returns></returns>
        internal bool CheckSign()
        {
            return true;
        }

        /// <summary>
        ///     针对HttpHeader特征阻止不安全访问
        /// </summary>
        /// <returns></returns>
        internal bool KillDenyHttpHeaders()
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
                LogRecorderX.Exception(e);
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
            ////OS匹配
            //if (!string.IsNullOrWhiteSpace(Data.ApiItem.Os) && Data.Token.IndexOf(Data.ApiItem.Os, StringComparison.OrdinalIgnoreCase) < 0)
            //    return false;
            ////APP匹配
            //if (!string.IsNullOrWhiteSpace(Data.ApiItem.App) && Data.Token.IndexOf(Data.ApiItem.App, StringComparison.OrdinalIgnoreCase) < 0)
            //    return false;
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

                if (Data.Arguments.ContainsKey("token"))
                    Data.Arguments.Remove("token");
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
                    default:
                        Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                        return false;
                    case '*':
                    case '#':
                        break;
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
                    LogRecorderX.MonitorTrace("Token Layout Error");
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
                LogRecorderX.Exception(e);
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
                    //OrgKey = result.ResultData.Organization,
                    //RouteName = result.ResultData.Organization
                });
                var context = (JObject)JsonConvert.DeserializeObject(JsonHelper.SerializeObject(GlobalContext.Current));
                var obj = (JObject)JsonConvert.DeserializeObject(json);
                context["user"] = obj["data"];
                Data.GlobalContextJson = context.ToString();
                return result;
            }
        }
        #endregion
        #region V2令牌校验

        /// <summary>
        ///     执行检查
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空或不合格
        ///     2：令牌是伪造的
        /// </returns>
        public bool CheckToken2()
        {
            try
            {
                if (!RouteOption.Option.Security.CheckBearer)
                {
                    GlobalContext.SetUser(LoginUserInfo.CreateAnymouse(Data.Token, "*", "*"));
                    return true;
                }

                if (Data.Arguments.ContainsKey("token"))
                    Data.Arguments.Remove("token");
                if (!CheckApisInner())
                {
                    Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                    return false;
                }

                var token1 = '\0';
                if (!string.IsNullOrWhiteSpace(Data.Token))
                {
                    token1 = Data.Token[0];
                    switch (token1)
                    {
                        default:
                            Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                            return false;
                        case '*':
                        case '#':
                            break;
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
                        LogRecorderX.MonitorTrace("Token Layout Error");
                        Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                        return false;
                    }
                }


                IApiResult result;
                switch (token1)
                {
                    default:
                        Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                        return false;
                    case '\0':
                    case '*':
                        if (Data.ApiItem != null && Data.ApiItem.NeedLogin)
                        {
                            Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                            return false;
                        }
                        result = CheckToken2("DeviceId", out var vl);
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
                        result = CheckToken2("AccessToken", out vl);
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
                LogRecorderX.Exception(e);
                return true;
            }
        }

        /// <summary>
        /// AT校验请求参数
        /// </summary>
        [DataContract, JsonObject(MemberSerialization.OptIn)]
        public class VerifyTokenArgument 
        {
            /// <summary>
            /// 身份令牌
            /// </summary>
            [JsonProperty]
            public string AuthToken { get; set; }

            /// <summary>
            /// 场景令牌
            /// </summary>
            [JsonProperty]
            public string SceneToken { get; set; }

            /// <summary>
            /// 场景令牌
            /// </summary>
            [JsonProperty]
            public string Page { get; set; }

            /// <summary>
            /// 当前API
            /// </summary>
            [JsonProperty]
            public string API { get; set; }
        }
        private IApiResult<LoginUserInfo> CheckToken2(string name,out string json)
        {
            // 远程调用
            using (MonitorScope.CreateScope($"Check{name}:{Data.Token}"))
            {
                var arg = new VerifyTokenArgument
                {
                    AuthToken=Data.Token,
                    SceneToken = Data["__scene"],
                    Page = Data["__page"],
                    API =  $"{Data.ApiHost}/{Data.ApiName}"
                };
                var caller = new ApiClient
                {
                    Simple = true,
                    Station = RouteOption.Option.Security.AuthStation,
                    Commmand = RouteOption.Option.Security.TokenCheckApi,
                    Argument = JsonConvert.SerializeObject(arg)
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
                    //OrgKey = result.ResultData.Organization,
                    //RouteName = result.ResultData.Organization
                });
                var context = (JObject)JsonConvert.DeserializeObject(JsonHelper.SerializeObject(GlobalContext.Current));
                var obj = (JObject)JsonConvert.DeserializeObject(json);
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
            if (data.UserState != UserOperatorStateType.Success || data.ApiHost == null)
                return false;
            if (string.IsNullOrWhiteSpace(data.ResultMessage))
            {
                IocHelper.Create<IRuntimeWaring>().Waring(data.ApiHost, data.ApiName, "返回值非法(空内容)");
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
                    IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, "返回值非法(空内容)");
                    return true;
            }

            IApiResult result;
            try
            {
                result = ApiResultIoc.Ioc.DeserializeObject(data.ResultMessage);
                if (result == null)
                {
                    IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, "返回值非法(空内容)");
                    data.UserState = UserOperatorStateType.FormalError;
                    data.ZeroState = ZeroOperatorStateType.Error;
                    return false;
                }
            }
            catch
            {
                IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, "返回值非法(空内容)");
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
            IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, result.Status?.ClientMessage ?? "处理错误但无消息");
            return false;
        }


        #endregion
    }
}