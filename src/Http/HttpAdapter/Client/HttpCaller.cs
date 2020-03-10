using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     Api站点
    /// </summary>
    internal class HttpCaller
    {
        #region Properties

        /// <summary>
        ///     返回值
        /// </summary>
        internal string Result;

        /// <summary>
        ///     请求站点
        /// </summary>
        internal string Station;

        /// <summary>
        ///     上下文内容（透传方式）
        /// </summary>
        internal string ContextJson;

        /// <summary>
        ///     标题
        /// </summary>
        internal string Title;

        /// <summary>
        ///     调用命令
        /// </summary>
        internal string Commmand;

        /// <summary>
        ///     参数
        /// </summary>
        internal string Argument;

        #endregion

        #region Flow

        private readonly static IHttpClientFactory _clientFactory;
        static HttpCaller()
        {
            IocHelper.ServiceCollection.AddHttpClient();
            _clientFactory = IocHelper.Create<IHttpClientFactory>();
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal bool Call()
        {
            var task = CallApi();
            task.Wait();
            return task.Result;
        }


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal Task<bool> CallAsync()
        {
            return CallApi();
        }


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal bool Plan(ZeroPlanInfo plan)
        {
            var task = CallPlan(plan);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal Task<bool> PlanAsync(ZeroPlanInfo plan)
        {
            return CallPlan(plan);
        }

        #endregion

        #region Socket

        private async Task<bool> CallApi()
        {
            var result = await CallInner();
            Result = result.Item2;

            LogRecorder.MonitorTrace(() => $"result:{Result}");
            return result.Item1;
        }
        private async Task<Tuple<bool, string>> CallInner()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{Station}/{Commmand}");
            request.Content = new StringContent(Argument, Encoding.UTF8, "application/json");
            using (var client = _clientFactory.CreateClient())
            {
                using (var response = await client.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return new Tuple<bool, string>(false, ApiResult.NetworkErrorJson);
                    }
                    var result = await response.Content.ReadAsStringAsync();
                    return new Tuple<bool, string>(true, result);
                }
            }
        }


#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        private async Task<bool> CallPlan(ZeroPlanInfo plan)
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        {
            return false;
            //Result = LastResult.Result;
            //Binary = LastResult.Binary;
            //ResultType = LastResult.ResultType;
            //LogRecorder.MonitorTrace(() => $"result:{Result}");
            //return LastResult.InteractiveSuccess;

        }
        #endregion

    }
}