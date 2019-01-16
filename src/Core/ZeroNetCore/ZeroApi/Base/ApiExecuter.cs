using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api调用
    /// </summary>
    public class ApiExecuter
    {
        internal ApiStationBase Station;
        /// <summary>
        /// 调用 
        /// </summary>
        public void Execute(ref ZSocket socket, ApiCallItem item)
        {
            using (IocScope.CreateScope())
            {
                GlobalContext.Current.DependencyObjects.Annex(item);
                Interlocked.Increment(ref Station.CallCount);
                try
                {
                    if (LogRecorder.LogMonitor)
                        ApiCallByMonitor(socket, item);
                    else
                        ApiCallNoMonitor(socket, item);
                }
                catch (Exception ex)
                {
                    ZeroTrace.WriteException(Station.StationName, ex, "ApiCall", item.ApiName);
                    item.Result = ApiResult.InnerErrorJson;
                    Station.OnExecuestEnd(socket, item, ZeroOperatorStateType.Error);
                }
                finally
                {
                    Interlocked.Decrement(ref Station.WaitCount);
                }
            }
        }

        private void Prepare(ApiCallItem item)
        {
            item.Handlers = Station.CreateHandlers();
            if (item.Handlers == null)
                return;
            foreach (var p in item.Handlers)
            {
                try
                {
                    p.Prepare(Station, item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station.StationName, e, "PreActions", item.ApiName);
                }
            }
        }

        private void End(ApiCallItem item)
        {
            if (item.Handlers == null)
                return;
            foreach (var p in item.Handlers)
            {
                try
                {
                    p.End(Station, item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station.StationName, e, "EndActions", item.ApiName);
                }
            }
        }
        private void ApiCallByMonitor(ZSocket socket, ApiCallItem item)
        {
            using (MonitorScope.CreateScope($"{Station.StationName}/{item.ApiName}"))
            {
                LogRecorder.MonitorTrace($"Caller:{Encoding.ASCII.GetString(item.Caller)}");
                LogRecorder.MonitorTrace($"GlobalId:{item.GlobalId}");
                LogRecorder.MonitorTrace(JsonConvert.SerializeObject(item,Formatting.Indented));
                ZeroOperatorStateType state = RestoryContext(item);
                if (state == ZeroOperatorStateType.Ok)
                {
                    using (MonitorScope.CreateScope("Prepare"))
                    {
                        Prepare(item);
                    }
                    using (MonitorScope.CreateScope("Do"))
                    {
                        state = ExecCommand(item);
                    }

                    if (state != ZeroOperatorStateType.Ok)
                        Interlocked.Increment(ref Station.ErrorCount);
                    else
                        Interlocked.Increment(ref Station.SuccessCount);
                }
                else
                {
                    LogRecorder.MonitorTrace("Restory context failed");
                    Interlocked.Increment(ref Station.ErrorCount);
                }

                LogRecorder.MonitorTrace(item.Result);
                if (!Station.OnExecuestEnd(socket, item, state))
                {
                    ZeroTrace.WriteError(item.ApiName, "SendResult");
                    Interlocked.Increment(ref Station.SendError);
                }
                End(item);
            }
        }
        private void ApiCallNoMonitor(ZSocket socket, ApiCallItem item)
        {
            ZeroOperatorStateType state = RestoryContext(item);
            if (state == ZeroOperatorStateType.Ok)
            {
                Prepare(item);
                state = ExecCommand(item);

                if (state != ZeroOperatorStateType.Ok)
                    Interlocked.Increment(ref Station.ErrorCount);
                else
                    Interlocked.Increment(ref Station.SuccessCount);
            }
            else
            {
                Interlocked.Increment(ref Station.ErrorCount);
            }
            if (!Station.OnExecuestEnd(socket, item, state))
            {
                Interlocked.Increment(ref Station.SendError);
            }
            End(item);
        }
        /// <summary>
        /// 还原调用上下文
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private ZeroOperatorStateType RestoryContext(ApiCallItem item)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(item.ContextJson))
                {
                    GlobalContext.SetContext(JsonConvert.DeserializeObject<ApiContext>(item.ContextJson));
                }
                GlobalContext.Current.DependencyObjects.Annex(item);
                if (!string.IsNullOrWhiteSpace(item.Content))
                {
                    GlobalContext.Current.DependencyObjects.Annex(JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Content));
                }
                GlobalContext.Current.Request.SetValue(item.CallId, item.Requester, item.RequestId);
                GlobalContext.Current.Request.LocalGlobalId = item.GlobalId;
                return ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace($"Restory context exception:{e.Message}");
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "restory context", item.ContextJson);
                item.Result = ApiResult.ArgumentErrorJson;
                item.Status = UserOperatorStateType.FormalError;
                return ZeroOperatorStateType.LocalException;
            }
        }
        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private ZeroOperatorStateType ExecCommand(ApiCallItem item)
        {
            //1 查找调用方法
            if (!Station.ApiActions.TryGetValue(item.ApiName.Trim(), out var action))
            {
                LogRecorder.MonitorTrace($"Error: Action({item.ApiName}) no find");
                item.Result = ApiResult.NoFindJson;
                item.Status = UserOperatorStateType.NotFind;
                return ZeroOperatorStateType.NotFind;
            }

            //2 确定调用方法及对应权限
            if (action.NeedLogin && (GlobalContext.Customer == null || GlobalContext.Customer.UserId <= 0))
            {
                LogRecorder.MonitorTrace("Error: Need login user");
                item.Result = ApiResult.DenyAccessJson;
                item.Status = UserOperatorStateType.DenyAccess;
                return ZeroOperatorStateType.DenyAccess;
            }

            //3 参数校验
            try
            {
                if (!action.RestoreArgument(item.Argument ?? "{}"))
                {
                    LogRecorder.MonitorTrace("Error: argument can't restory.");
                    item.Result = ApiResult.ArgumentErrorJson;
                    item.Status = UserOperatorStateType.FormalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace($"Error: argument restory {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "restory argument", item.Argument);
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = UserOperatorStateType.FormalError;
                return ZeroOperatorStateType.LocalException;
            }

            try
            {

                if (!action.Validate(out var message))
                {
                    LogRecorder.MonitorTrace($"Error: argument validate {message}.");
                    item.Result = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.LogicalError, message));
                    item.Status = UserOperatorStateType.LogicalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace($"Error: argument validate {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "invalidate argument", item.Argument);
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = UserOperatorStateType.LocalException;
                return ZeroOperatorStateType.LocalException;
            }

            //4 方法执行
            try
            {
                var res = action.Execute();
                switch (res)
                {
                    case IApiResult result:
                        if (result.Status == null)
                            result.Status = new ApiStatusResult { InnerMessage = item.GlobalId };
                        else
                            result.Status.InnerMessage = item.GlobalId;
                        item.Result = JsonConvert.SerializeObject(result);
                        item.Status = result.Success ? UserOperatorStateType.Success : UserOperatorStateType.LogicalError;
                        return result.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed;
                    case string str:
                        item.Result = str;
                        item.Status = UserOperatorStateType.Success;
                        return ZeroOperatorStateType.Ok;
                    default:
                        item.Result = ApiResult.SucceesJson;
                        item.Status = UserOperatorStateType.Success;
                        return ZeroOperatorStateType.Ok;
                }

            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                LogRecorder.MonitorTrace($"Error: execute {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "execute", JsonConvert.SerializeObject(item));
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = UserOperatorStateType.LocalException;
                return ZeroOperatorStateType.LocalException;
            }
        }
    }
}