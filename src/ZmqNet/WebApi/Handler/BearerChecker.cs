#if !NETSTANDARD2_0
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.DataModel.Redis;
using Agebull.Common.Logging;
using GoodLin.Common.Ioc;
using Newtonsoft.Json;
using Yizuan.Service.Api.OAuth;

namespace Yizuan.Service.Api.WebApi
{
    /// <summary>
    ///     身份检查器
    /// </summary>
    public class BearerHandler : IHttpSystemHandler
    {
        /// <summary>
        ///     开始时的处理
        /// </summary>
        /// <returns>如果返回内容不为空，直接返回,后续的处理不再继续</returns>
        Task<HttpResponseMessage> IHttpSystemHandler.OnBegin(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var code = Check(request);
            //校验不通过，直接返回，不做任何处理
            if (code == 0)
                return null;
            LogRecorder.MonitorTrace("Authorization：校验出错");
            return Task.Factory.StartNew(() =>
            {
                var result = ApiResult.Error(code);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(result))
                };
            }, cancellationToken);
        }

        /// <summary>
        ///     结束时的处理
        /// </summary>
        void IHttpSystemHandler.OnEnd(HttpRequestMessage request, CancellationToken cancellationToken,
            HttpResponseMessage response)
        {
        }

        /// <summary>
        ///     执行检查
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空或不合格
        ///     2：令牌是伪造的
        /// </returns>
        private int Check(HttpRequestMessage request)
        {
            var header = request.Headers.ToString();
            if (string.IsNullOrWhiteSpace(header) || header.Contains("iToolsVM"))
                return ErrorCode.DenyAccess;
            var token = ExtractToken(request);
            if (string.IsNullOrWhiteSpace(token))
                return ErrorCode.DenyAccess;

            switch (token[0])
            {
                default:
                case '*':
                    ApiContext.SetRequestContext(new InternalCallContext
                    {
                        RequestId = Guid.NewGuid(),
                        ServiceKey = System.Configuration.ConfigurationManager.AppSettings["ServiceKey"],
                        Bear = token,
                        UserId = -2
                    });
                    return CheckDeviceId(request, token);
                case '{':
                    return CheckServiceKey(request, token);
                case '$':
                    return RevertCallContext(request, token);
                case '#':
                    ApiContext.SetRequestContext(new InternalCallContext
                    {
                        RequestId = Guid.NewGuid(),
                        ServiceKey = System.Configuration.ConfigurationManager.AppSettings["ServiceKey"],
                        Bear = token,
                        UserId = -2
                    });
                    return CheckAccessToken(token);
            }
        }

        /// <summary>
        ///     检查设备标识
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空
        ///     2：令牌是伪造的
        /// </returns>
        private int CheckDeviceId(HttpRequestMessage request, string token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Contains('.') || token.Length <= 33)
                return ErrorCode.DenyAccess;
            for (var index = 1; index < token.Length; index++)
            {
                var ch = token[index];
                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_')
                    continue;
                return ErrorCode.DenyAccess;
            }
            var checker = IocHelper.Create<IBearValidater>();
            ApiResult<LoginUserInfo> result;
            try
            {
                result = checker.ValidateDeviceId(token);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return 0; //ErrorCode.Auth_Device_Unknow;
            }
            if (!result.Result)
                return result.Status.ErrorCode;
            CreateApiContext(result.ResultData, token);
            LogRecorder.MonitorTrace("Authorization：匿名用户");
            return 0;
        }

        /// <summary>
        ///     构造上下文
        /// </summary>
        private void CreateApiContext(LoginUserInfo customer, string token)
        {
            ApiContext.SetCustomer(customer);
            ApiContext.SetRequestContext(new InternalCallContext
            {
                Bear = token,
                Os = customer.Os,
                Browser = customer.Browser,
                RequestId = Guid.NewGuid(),
                ServiceKey = System.Configuration.ConfigurationManager.AppSettings["ServiceKey"],
                UserId = customer.UserId
            });
            ApiContext.Current.Cache();
            LogRecorder.MonitorTrace(JsonConvert.SerializeObject(customer));
        }

        /// <summary>
        ///     还原调用上下文
        /// </summary>
        private int RevertCallContext(HttpRequestMessage request, string token)
        {
            //if (request.Headers.UserAgent.ToString() != "Yizuan.Service.WebApi.WebApiCaller")
            //    return ErrorCode.DenyAccess;
            //for (var index = 1; index < token.Length; index++)
            //{
            //    var ch = token[index];
            //    if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '-')
            //        continue;
            //    return ErrorCode.DenyAccess;
            //}
            ApiContext context;
            using (var proxy = new RedisProxy())
            {
                var key = ApiContext.GetCacheKey(token);
                context = proxy.Get<ApiContext>(key);
                proxy.RemoveKey(key);
            }
            if (context?.Request == null || context.LoginUser == null)
            {
                //return ErrorCode.DenyAccess;
                ApiContext.TryCheckByAnymouse();
                return 0;
            }
            var checker = IocHelper.Create<IBearValidater>();
            var result = checker.ValidateServiceKey(context.Request.ServiceKey);
            if (!result.Result)
                return result.Status.ErrorCode;
            ApiContext.SetContext(context);
            return 0;
        }

        /// <summary>
        ///     检查旧标识
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空
        ///     2：令牌是伪造的
        /// </returns>
        private int CheckServiceKey(HttpRequestMessage request, string token)
        {
            InternalCallContext context;
            try
            {
                context = JsonConvert.DeserializeObject<InternalCallContext>(token);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return ErrorCode.Auth_ServiceKey_Unknow;
            }
            if (context == null)
                return ErrorCode.Auth_ServiceKey_Unknow;
            var checker = IocHelper.Create<IBearValidater>();
            var result = checker.ValidateServiceKey(context.ServiceKey);
            if (!result.Result)
                return result.Status.ErrorCode;
            var user = checker.GetLoginUser(context.Bear);
            if (!user.Result)
                return user.Status.ErrorCode;
            ApiContext.SetCustomer(user.ResultData);
            ApiContext.SetRequestContext(context);
            ApiContext.Current.Cache();
            LogRecorder.MonitorTrace($"Authorization：{user.ResultData.NickName}");
            return 0;
        }

        /// <summary>
        ///     检查AccessToken
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空
        ///     2：令牌是伪造的
        /// </returns>
        private int CheckAccessToken(string token)
        {
            if (token.Length != 33)
                return ErrorCode.DenyAccess;
            for (var index = 1; index < token.Length; index++)
            {
                var ch = token[index];
                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
                    continue;
                return ErrorCode.DenyAccess;
            }
            var checker = IocHelper.Create<IBearValidater>();
            ApiResult<LoginUserInfo> result;
            try
            {
                result = checker.VerifyAccessToken(token);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return ErrorCode.UnknowError;
            }
            if (!result.Result)
                return result.Status.ErrorCode;

            CreateApiContext(result.ResultData, token);

            LogRecorder.MonitorTrace("Authorization：" + result.ResultData.Account);
            return 0;
        }

        /// <summary>
        ///     取请求头的身份验证令牌
        /// </summary>
        /// <returns></returns>
        private static string ExtractToken(HttpRequestMessage request)
        {
            const string bearer = "Bearer";
            var authz = request.Headers.Authorization;
            if (authz != null)
                return string.Equals(authz.Scheme, bearer, StringComparison.OrdinalIgnoreCase) ? authz.Parameter : null;
            if (!request.Headers.Contains("Authorization"))
                return null;
            var au = request.Headers.GetValues("Authorization").FirstOrDefault();
            if (au == null)
                return null;
            var aus = au.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (aus.Length < 2 || aus[0] != bearer)
                return null;
            return aus[1].Trim();
        }
    }
}
#endif