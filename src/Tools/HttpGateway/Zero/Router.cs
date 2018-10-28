using System;
using System.Collections.Generic;
using System.IO;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
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
                    Data.Form = JsonConvert.SerializeObject(arguments);
                if (Request.ContentLength != null)
                {
                    using (var texter = new StreamReader(Request.Body))
                    {
                        Data.Context = texter.ReadToEnd();
                        if (string.IsNullOrEmpty(Data.Context))
                            Data.Context = null;
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
                var caller = new ApiClient
                {
                    Station = host.Station,
                    Commmand = Data.ApiName,
                    Argument = Data.Context ?? Data.Form,
                    ExtendArgument = Data.Form
                };
                caller.CallCommand();
                Data.ResultMessage = caller.Result;
                Data.Status = caller.State.ToOperatorStatus(true);

                LogRecorder.MonitorTrace($"State : {caller.State}");
            }
            return Data.ResultMessage;
        }
    }
}