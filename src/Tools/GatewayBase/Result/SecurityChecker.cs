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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using Agebull.MicroZero.Helpers;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     返回值检查
    /// </summary>
    internal class ResultChecker
    {
        #region 返回值检查


        /// <summary>
        ///     检查返回值是否合理
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool DoCheck(RouteData data)
        {
            if (data.UserState != UserOperatorStateType.Success || data.ApiHost == null)
                return false;
            if (string.IsNullOrWhiteSpace(data.ResultMessage))
            {
                //IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, "返回值非法(空内容)");
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
                    //IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, "返回值非法(空内容)");
                    return true;
            }

            IApiResult result;
            try
            {
                result = ApiResultIoc.Ioc.DeserializeObject(data.ResultMessage);
                if (result == null)
                {
                    //IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, "返回值非法(空内容)");
                    data.UserState = UserOperatorStateType.FormalError;
                    data.ZeroState = ZeroOperatorStateType.Error;
                    return false;
                }
            }
            catch
            {
                //IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, "返回值非法(空内容)");
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
            //IocHelper.Create<IRuntimeWaring>()?.Waring(data.ApiHost, data.ApiName, result.Status?.ClientMessage ?? "处理错误但无消息");
            return false;
        }


        #endregion
    }
}