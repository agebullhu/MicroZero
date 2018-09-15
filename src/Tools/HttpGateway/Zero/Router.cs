using System.Collections.Generic;
using System.IO;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
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
            var form = new Dictionary<string, string>();
            if (Request.QueryString.HasValue)
            {
                foreach (var key in Request.Query.Keys)
                    form.TryAdd(key, Request.Query[key]);
            }
            if (Request.HasFormContentType)
            {
                foreach (var key in Request.Form.Keys)
                    form.TryAdd(key, Request.Form[key]);
            }
            if (form.Count > 0)
                Data.Form = JsonConvert.SerializeObject(form);
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
            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                var caller = new ApiClient
                {
                    Station = host.Station,
                    Commmand = Data.ApiName,
                    Argument = Data.Context ?? Data.Form,
                    ExtendArgument = Data.Context == null ? null : Data.Form
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