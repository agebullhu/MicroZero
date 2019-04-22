using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ZeroMQ;

namespace Agebull.MicroZero.ZeroApis
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
                    item.Result = ApiResultIoc.InnerErrorJson;
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
                LogRecorder.MonitorTrace(JsonConvert.SerializeObject(item, Formatting.Indented));
                ZeroOperatorStateType state = RestoryContext(item);
                if (state == ZeroOperatorStateType.Ok)
                {
                    using (MonitorScope.CreateScope("Prepare"))
                    {
                        Prepare(item);
                    }
                    using (MonitorScope.CreateScope("Do"))
                    {
                        state = ExecCommand(item, true);
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
                if (state == ZeroOperatorStateType.Ok)
                {
                    using (MonitorScope.CreateScope("End"))
                    {
                        End(item);
                    }
                }
            }
        }
        private void ApiCallNoMonitor(ZSocket socket, ApiCallItem item)
        {
            ZeroOperatorStateType state = RestoryContext(item);
            if (state == ZeroOperatorStateType.Ok)
            {
                Prepare(item);
                state = ExecCommand(item, false);

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
                if (!string.IsNullOrWhiteSpace(item.Context))
                {
                    GlobalContext.SetContext(JsonConvert.DeserializeObject<GlobalContext>(item.Context));
                }
                GlobalContext.Current.DependencyObjects.Annex(item);
                if (!string.IsNullOrWhiteSpace(item.Content))
                {
                    GlobalContext.Current.DependencyObjects.Annex(JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Content));
                }
                GlobalContext.Current.Request.RequestId = item.RequestId;
                GlobalContext.Current.Request.CallGlobalId = item.CallId;
                GlobalContext.Current.Request.LocalGlobalId = item.GlobalId;
                return ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace($"Restory context exception:{e.Message}");
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "restory context", item.Context);
                item.Result = ApiResultIoc.ArgumentErrorJson;
                item.Status = UserOperatorStateType.FormalError;
                return ZeroOperatorStateType.LocalException;
            }
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="item"></param>
        /// <param name="monitor"></param>
        /// <returns></returns>
        private ZeroOperatorStateType ExecCommand(ApiCallItem item, bool monitor)
        {
            //1 查找调用方法
            if (!Station.ApiActions.TryGetValue(item.ApiName.Trim(), out var action))
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: Action({item.ApiName}) no find");
                item.Result = ApiResultIoc.NoFindJson;
                item.Status = UserOperatorStateType.NotFind;
                return ZeroOperatorStateType.NotFind;
            }

            //2 确定调用方法及对应权限
            if (action.NeedLogin && (GlobalContext.Customer == null || GlobalContext.Customer.UserId <= 0))
            {
                if (monitor)
                    LogRecorder.MonitorTrace("Error: Need login user");
                item.Result = ApiResultIoc.DenyAccessJson;
                item.Status = UserOperatorStateType.DenyAccess;
                return ZeroOperatorStateType.DenyAccess;
            }

            //3 参数校验
            try
            {
                if (!action.RestoreArgument(item.Argument ?? "{}"))
                {
                    if (monitor)
                        LogRecorder.MonitorTrace("Error: argument can't restory.");
                    item.Result = ApiResultIoc.ArgumentErrorJson;
                    item.Status = UserOperatorStateType.FormalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: argument restory {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "restory argument", item.Argument);
                item.Result = ApiResultIoc.LocalExceptionJson;
                item.Status = UserOperatorStateType.FormalError;
                return ZeroOperatorStateType.LocalException;
            }

            try
            {

                if (!action.Validate(out var message))
                {
                    if (monitor)
                        LogRecorder.MonitorTrace($"Error: argument validate {message}.");
                    item.Result = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.ArgumentError, message));
                    item.Status = UserOperatorStateType.LogicalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: argument validate {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "invalidate argument", item.Argument);
                item.Result = ApiResultIoc.LocalExceptionJson;
                item.Status = UserOperatorStateType.LocalException;
                return ZeroOperatorStateType.LocalException;
            }

            //4 方法执行
            try
            {
                GlobalContext.Current.DependencyObjects.Annex(action);
                var res = action.Execute();
                if (action.ResultType == typeof(string))
                {
                    item.Result = res as string;
                    item.Status = UserOperatorStateType.Success;
                    return ZeroOperatorStateType.Ok;
                }
                switch (res)
                {
                    case IApiResult result:
                        if (result.Status == null)
                            result.Status = new ApiStatusResult { InnerMessage = item.GlobalId };
                        else
                            result.Status.InnerMessage = item.GlobalId;
                        item.Result = JsonHelper.SerializeObject(result);
                        item.Status = result.Success ? UserOperatorStateType.Success : UserOperatorStateType.LogicalError;
                        return result.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed;
                    case string str:
                        item.Result = str;
                        item.Status = UserOperatorStateType.Success;
                        return ZeroOperatorStateType.Ok;
                    default:
                        item.Result = ApiResultIoc.SucceesJson;
                        item.Status = UserOperatorStateType.Success;
                        return ZeroOperatorStateType.Ok;
                }

            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: execute {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, item.ApiName, "execute", JsonHelper.SerializeObject(item));
                item.Result = ApiResultIoc.LocalExceptionJson;
                item.Status = UserOperatorStateType.LocalException;
                return ZeroOperatorStateType.LocalException;
            }
        }
    }
}