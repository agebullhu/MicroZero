using System;
using System.Collections.Generic;
using System.IO;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal partial class Router
    {
        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private string CallZero()
        {
            if (!(Data.RouteHost is ZeroHost host))
            {
                LogRecorder.MonitorTrace("Host Type Failed");
                return Data.ResultMessage;
            }
            try
            {
                var arguments = new Dictionary<string, string>();
                if (Request.QueryString.HasValue)
                {
                    foreach (var key in Request.Query.Keys)
                        arguments.TryAdd(key, Request.Query[key]);
                }
                if (Request.HasFormContentType)
                {
                    foreach (var key in Request.Form.Keys)
                        arguments.TryAdd(key, Request.Form[key]);
                }
                if (arguments.Count > 0)
                    Data.HttpForm = JsonConvert.SerializeObject(arguments);
                if (Request.ContentLength != null)
                {
                    using (var texter = new StreamReader(Request.Body))
                    {
                        Data.HttpContext = texter.ReadToEnd();
                        if (string.IsNullOrEmpty(Data.HttpContext))
                            Data.HttpContext = null;
                        texter.Close();
                    }
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "读取远程参数");
                return Data.ResultMessage = ApiResult.ArgumentErrorJson;
            }
            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                return CallApi(host);
            }
        }

        private string CallApi(ZeroHost zeroHost)
        {
            
            var caller = new ApiClient
            {
                Station = zeroHost.Station,
                Commmand = Data.ApiName,
                Argument = Data.HttpContext ?? Data.HttpForm,
                ExtendArgument = Data.HttpForm,
                ContextJson = Data.GlobalContextJson
            };
            caller.CallCommand();
            Data.ZeroState = caller.State;
            Data.UserState = caller.State.ToOperatorStatus(true);
            caller.CheckStateResult();
            return Data.ResultMessage = caller.Result;
        }
        private string CallApi2(ZeroHost zeroHost)
        {
            var config = ZeroApplication.Config[zeroHost.Station];
            if (config == null)
            {
                Data.UserState = UserOperatorStateType.NotFind;
                Data.ZeroState = ZeroOperatorStateType.NotFind;
                {
                    return Data.ResultMessage = ApiResult.NoFindJson;
                }
            }

            switch (config.StationType)
            {
                case ZeroStationType.Api:
                case ZeroStationType.RouteApi:
                    var caller = new ApiClient
                    {
                        Station = zeroHost.Station,
                        Commmand = Data.ApiName,
                        Argument = Data.HttpContext ?? Data.HttpForm,
                        ExtendArgument = Data.HttpForm,
                        ContextJson = Data.GlobalContextJson
                    };
                    caller.CallCommand();
                    Data.ResultMessage = caller.Result;
                    Data.ZeroState = caller.State;
                    Data.UserState = UserOperatorStateType.NotFind;
                    return Data.ResultMessage = ApiResult.NoFindJson;
                case ZeroStationType.Notify:
                case ZeroStationType.Queue:
                    if (ZeroPublisher.Publish(zeroHost.Station, Data.ApiName, Data.ApiName, Data.HttpContext))
                    {
                        Data.UserState = UserOperatorStateType.Success;
                        Data.ZeroState = ZeroOperatorStateType.Ok;
                        return Data.ResultMessage = ApiResult.SucceesJson;
                    }
                    else
                    {
                        Data.UserState = UserOperatorStateType.NetWorkError;
                        Data.ZeroState = ZeroOperatorStateType.NetError;
                        return Data.ResultMessage = ApiResult.NetworkErrorJson;
                    }
                default:
                    Data.UserState = UserOperatorStateType.NotFind;
                    Data.ZeroState = ZeroOperatorStateType.NotFind;
                    return Data.ResultMessage = ApiResult.NoFindJson;
            }
            
        }
    }
}