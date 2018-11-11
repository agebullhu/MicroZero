using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
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
                        ApiCallByMonitor(ref socket, item);
                    else
                        ApiCallNoMonitor(ref socket, item);
                }
                catch (Exception ex)
                {
                    ZeroTrace.WriteException(Station.StationName, ex, "ApiCall", item.ApiName);
                    item.Result = ApiResult.InnerErrorJson;
                    Station.OnExecuestEnd(ref socket, item, ZeroOperatorStateType.Error);
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
            if (item.RequestId == null)
                item.RequestId = item.GlobalId;

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
        private void ApiCallByMonitor(ref ZSocket socket, ApiCallItem item)
        {
            using (MonitorScope.CreateScope(item.ApiName))
            {
                LogRecorder.MonitorTrace($"Caller:{item.Caller}");
                LogRecorder.MonitorTrace($"GlobalId:{item.GlobalId}");
                LogRecorder.MonitorTrace(JsonConvert.SerializeObject(item));
                ZeroOperatorStateType state = RestoryContext(item);
                if (state == ZeroOperatorStateType.Ok)
                {
                    Prepare(item);
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
                    Interlocked.Increment(ref Station.ErrorCount);

                LogRecorder.MonitorTrace(item.Result);
                if (!Station.OnExecuestEnd(ref socket, item, state))
                {
                    ZeroTrace.WriteError(item.ApiName, "SendResult");
                    Interlocked.Increment(ref Station.SendError);
                }
                End(item);
            }
        }
        private void ApiCallNoMonitor(ref ZSocket socket, ApiCallItem item)
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
            if (!Station.OnExecuestEnd(ref socket, item, state))
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
                if (!string.IsNullOrWhiteSpace(item.Content))
                {
                    GlobalContext.Current.DependencyObjects.Annex(JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Content));
                }

                GlobalContext.Current.Request.SetValue(item.GlobalId, item.Requester, item.RequestId);
                return ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "restory context", item.ContextJson);
                item.Result = ApiResult.ArgumentErrorJson;
                item.Status = ZeroOperatorStatus.FormalError;
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
                item.Result = ApiResult.NoFindJson;
                item.Status = ZeroOperatorStatus.NotFind;
                return ZeroOperatorStateType.NotFind;
            }

            //2 确定调用方法及对应权限
            if (action.NeedLogin && (GlobalContext.Customer == null || GlobalContext.Customer.UserId <= 0))
            {
                item.Result = ApiResult.DenyAccessJson;
                item.Status = ZeroOperatorStatus.DenyAccess;
                return ZeroOperatorStateType.DenyAccess;
            }

            //3 参数校验
            try
            {
                if (!action.RestoreArgument(item.Argument ?? "{}"))
                {
                    item.Result = ApiResult.ArgumentErrorJson;
                    item.Status = ZeroOperatorStatus.FormalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "restory argument", item.Argument);
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = ZeroOperatorStatus.FormalError;
                return ZeroOperatorStateType.LocalException;
            }

            try
            {

                if (!action.Validate(out var message))
                {
                    item.Result = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.LogicalError, message));
                    item.Status = ZeroOperatorStatus.LogicalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "invalidate argument", item.Argument);
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = ZeroOperatorStatus.LocalException;
                return ZeroOperatorStateType.LocalException;
            }

            //4 方法执行
            try
            {
                var result = action.Execute();
                if (result != null)
                {
                    if (result.Status == null)
                        result.Status = new ApiStatusResult { InnerMessage = item.GlobalId };
                    else
                        result.Status.InnerMessage = item.GlobalId;
                }

                item.Result = result == null ? ApiResult.SucceesJson : JsonConvert.SerializeObject(result);
                item.Status = result == null || result.Success ? ZeroOperatorStatus.Success : ZeroOperatorStatus.LogicalError;
                return result == null || result.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "execute", JsonConvert.SerializeObject(item));
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = ZeroOperatorStatus.LocalException;
                return ZeroOperatorStateType.LocalException;
            }
        }
    }
}