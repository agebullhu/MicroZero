using System.Collections.Generic;
using System.IO;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
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
            var host = Data.RouteHost as ZeroHost;
            if (host==null)
            {
                LogRecorder.MonitorTrace("Host Type Failed");
                return Data.ResultMessage;
            }

            string context;
            //参数解析
            if (Request.HasFormContentType)
            {
                var values = new Dictionary<string, string>();
                foreach (var form in Request.Form.Keys)
                    values.TryAdd(form, Request.Form[form]);
                context = JsonConvert.SerializeObject(values);
            }
            else if (Request.ContentLength > 0)
            {
                using (var texter = new StreamReader(Request.Body))
                {
                    context = texter.ReadToEnd();
                }
            }
            else
            {
                var values = new Dictionary<string, string>();
                foreach (var query in Request.Query.Keys)
                    values.TryAdd(query, Request.Query[query]);
                context = JsonConvert.SerializeObject(values);
            }

            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                var caller = new ApiClient
                {
                    Station = host.Station,
                    Commmand = Data.ApiName,
                    Argument = context
                };
                caller.CallCommand();
                if (caller.State < ZeroOperatorStateType.Failed)
                    Data.Status = RouteStatus.None;
                else if (caller.State < ZeroOperatorStateType.Bug)
                    Data.Status = RouteStatus.LogicalError;
                else if (caller.State < ZeroOperatorStateType.Error)
                    Data.Status = RouteStatus.FormalError;
                else if (caller.State <= ZeroOperatorStateType.NotSupport)
                    Data.Status = RouteStatus.NotFind;
                else if (caller.State == ZeroOperatorStateType.DenyAccess)
                    Data.Status = RouteStatus.DenyAccess;
                else if (caller.State == ZeroOperatorStateType.Unavailable)
                    Data.Status = RouteStatus.Unavailable;
                else if (caller.State == ZeroOperatorStateType.LocalException)
                    Data.Status = RouteStatus.LocalException;
                else if (caller.State >= ZeroOperatorStateType.LocalNoReady ||
                         caller.State == ZeroOperatorStateType.TimeOut)
                    Data.Status = RouteStatus.LocalError;
                else Data.Status = RouteStatus.RemoteError;
                Data.ResultMessage = caller.Result;
                LogRecorder.MonitorTrace(caller.State.Text());
                LogRecorder.MonitorTrace(Data.ResultMessage);
            }

            return Data.ResultMessage;
        }
    }
}