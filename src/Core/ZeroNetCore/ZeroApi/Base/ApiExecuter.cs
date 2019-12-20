using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     Api调用器
    /// </summary>
    public class ApiExecuter
    {

        /// <summary>
        /// 当前站点
        /// </summary>
        internal ApiStationBase.ApiTaskItem TaskItem;
        /// <summary>
        /// 当前站点
        /// </summary>
        internal ApiStationBase Station;
        /// <summary>
        /// 当前连接
        /// </summary>
        internal ZSocket Socket { get; set; }
        /// <summary>
        /// 调用的内容
        /// </summary>
        internal ApiCallItem Item;

        /// <summary>
        /// 取消停牌
        /// </summary>
        internal CancellationTokenSource CancellationToken = new CancellationTokenSource();

        /// <summary>
        /// 范围资源
        /// </summary>
        internal IDisposable ScopeResource { get; set; }

        #region 同步

        /// <summary>
        /// 调用 
        /// </summary>
        public async Task Execute()
        {
            await Task.Yield();
            TaskItem.Thread = Thread.CurrentThread;
            try
            {
                using (ScopeResource = IocScope.CreateScope())
                {
                    if (CancellationToken.IsCancellationRequested)
                        return;
                    Prepare();
                    GlobalContext.Current.DependencyObjects.Annex(Item);
                    ZeroOperatorStateType state;
                    try
                    {
                        state = LogRecorderX.LogMonitor ? ApiCallByMonitor() : ApiCallNoMonitor();
                    }
                    catch (Exception ex)
                    {
                        ZeroTrace.WriteException(Station.StationName, ex, "ApiCall", Item.ApiName, Item.Argument);
                        Item.Result = ApiResultIoc.InnerErrorJson;
                        state = ZeroOperatorStateType.LocalException;
                    }

                    var socket = Socket;
                    Socket = null;
                    if (!await Station.OnExecuestEnd(socket, Item, state))
                    {
                        ZeroTrace.WriteError(Item.ApiName, "SendResult");
                    }

                    End();
                    ScopeResource = null;
                }
            }
            catch (ThreadInterruptedException)
            {
                ZeroTrace.SystemLog("Timeout", Item.ApiName, Item.Argument,Item.Content,Item.Context);
            }
            finally
            {
                Station.Tasks.TryRemove(TaskItem.TaskId, out _);
                CancellationToken.Dispose();
                CancellationToken = null;
            }
        }

        private ZeroOperatorStateType ApiCallByMonitor()
        {
            using (MonitorScope.CreateScope($"{Station.StationName}/{Item.ApiName}"))
            {
                LogRecorderX.MonitorTrace($"Caller:{Encoding.ASCII.GetString(Item.Caller)}");
                LogRecorderX.MonitorTrace($"GlobalId:{Item.GlobalId}");
                LogRecorderX.MonitorTrace(JsonConvert.SerializeObject(Item, Formatting.Indented));

                ZeroOperatorStateType state = RestoryContext();

                if (state != ZeroOperatorStateType.Ok)
                {
                    LogRecorderX.MonitorTrace("Restory context failed");
                    Interlocked.Increment(ref Station.ErrorCount);
                    return state;
                }

                using (MonitorScope.CreateScope("Do"))
                {
                    state = CommandPrepare(true, out var action);
                    if (state == ZeroOperatorStateType.Ok)
                    {
                        object res;
                        if (CancellationToken.IsCancellationRequested)
                            res = ZeroOperatorStateType.Unavailable;
                        else
                        {
                            GlobalContext.Current.DependencyObjects.Annex(action);
                            GlobalContext.Current.DependencyObjects.Annex(this);
                            res = CommandExec(true, action);
                        }

                        state = CheckCommandResult(res);
                    }
                }

                if (state != ZeroOperatorStateType.Ok)
                    Interlocked.Increment(ref Station.ErrorCount);
                else
                    Interlocked.Increment(ref Station.SuccessCount);

                LogRecorderX.MonitorTrace(Item.Result);
                return state;
            }
        }

        private ZeroOperatorStateType ApiCallNoMonitor()
        {
            ZeroOperatorStateType state = RestoryContext();
            if (state != ZeroOperatorStateType.Ok)
            {
                Interlocked.Increment(ref Station.ErrorCount);
                return state;
            }

            Prepare();
            state = CommandPrepare(false, out var action);
            if (state == ZeroOperatorStateType.Ok)
            {
                object res;
                if (CancellationToken.IsCancellationRequested)
                    res = ZeroOperatorStateType.Unavailable;
                else
                {
                    GlobalContext.Current.DependencyObjects.Annex(action);
                    GlobalContext.Current.DependencyObjects.Annex(this);
                    res = CommandExec(false, action);
                }

                state = CheckCommandResult(res);
            }

            if (state != ZeroOperatorStateType.Ok)
                Interlocked.Increment(ref Station.ErrorCount);
            else
                Interlocked.Increment(ref Station.SuccessCount);
            return state;
        }


        #endregion

        #region 注入


        private void Prepare()
        {
            Item.Handlers = Station.CreateHandlers();
            if (Item.Handlers == null)
                return;
            foreach (var p in Item.Handlers)
            {
                if (CancellationToken.IsCancellationRequested)
                    return;
                try
                {
                    p.Prepare(Station, Item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station.StationName, e, "PreActions", Item.ApiName);
                }
            }
        }

        private void End()
        {
            if (Item.Handlers == null)
                return;
            foreach (var p in Item.Handlers)
            {
                try
                {
                    p.End(Station, Item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station.StationName, e, "EndActions", Item.ApiName);
                }
            }
        }

        #endregion

        #region 执行命令

        /// <summary>
        /// 还原调用上下文
        /// </summary>
        /// <returns></returns>
        private ZeroOperatorStateType RestoryContext()
        {
            try
            {
                //GlobalContext.Current.DependencyObjects.Annex(CancellationToken);
                if (!string.IsNullOrWhiteSpace(Item.Context))
                {
                    GlobalContext.SetContext(JsonConvert.DeserializeObject<GlobalContext>(Item.Context));//BUG:数据不能全覆盖
                }
                GlobalContext.Current.DependencyObjects.Annex(Item);
                if (!string.IsNullOrWhiteSpace(Item.Argument))
                {
                    try
                    {
                        GlobalContext.Current.DependencyObjects.Annex(JsonConvert.DeserializeObject<Dictionary<string, string>>(Item.Argument));
                    }
                    catch
                    {
                    }
                }
                else if (!string.IsNullOrWhiteSpace(Item.Extend))
                {
                    try
                    {
                        GlobalContext.Current.DependencyObjects.Annex(JsonHelper.DeserializeObject<Dictionary<string, string>>(Item.Extend));
                    }
                    catch
                    {
                    }
                }
                GlobalContext.Current.Request.RequestId = Item.RequestId;
                GlobalContext.Current.Request.CallGlobalId = Item.CallId;
                GlobalContext.Current.Request.LocalGlobalId = Item.GlobalId;
                return ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                LogRecorderX.MonitorTrace($"Restory context exception:{e.Message}");
                ZeroTrace.WriteException(Station.StationName, e, Item.ApiName, "restory context", Item.Context);
                Item.Result = ApiResultIoc.ArgumentErrorJson;
                Item.Status = UserOperatorStateType.FormalError;
                return ZeroOperatorStateType.LocalException;
            }
        }


        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private ZeroOperatorStateType CommandPrepare(bool monitor, out ApiAction action)
        {
            //1 查找调用方法
            if (!Station.ApiActions.TryGetValue(Item.ApiName.Trim(), out action))
            {
                if (monitor)
                    LogRecorderX.MonitorTrace($"Error: Action({Item.ApiName}) no find");
                Item.Result = ApiResultIoc.NoFindJson;
                Item.Status = UserOperatorStateType.NotFind;
                return ZeroOperatorStateType.NotFind;
            }

            //2 确定调用方法及对应权限
            if (action.NeedLogin && (GlobalContext.Customer == null || GlobalContext.Customer.UserId <= 0))
            {
                if (monitor)
                    LogRecorderX.MonitorTrace("Error: Need login user");
                Item.Result = ApiResultIoc.DenyAccessJson;
                Item.Status = UserOperatorStateType.DenyAccess;
                return ZeroOperatorStateType.DenyAccess;
            }

            //3 参数校验
            if (action.Access.HasFlag(ApiAccessOption.ArgumentIsDefault))
                return ZeroOperatorStateType.Ok;
            try
            {
                if (!action.RestoreArgument(Item.Argument ?? "{}"))
                {
                    if (monitor)
                        LogRecorderX.MonitorTrace("Error: argument can't restory.");
                    Item.Result = ApiResultIoc.ArgumentErrorJson;
                    Item.Status = UserOperatorStateType.FormalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorderX.MonitorTrace($"Error: argument restory {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, Item.ApiName, "restory argument", Item.Argument);
                Item.Result = ApiResultIoc.LocalExceptionJson;
                Item.Status = UserOperatorStateType.FormalError;
                return ZeroOperatorStateType.LocalException;
            }

            try
            {
                if (!action.Validate(out var message))
                {
                    if (monitor)
                        LogRecorderX.MonitorTrace($"Error: argument validate {message}.");
                    Item.Result = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.ArgumentError, message));
                    Item.Status = UserOperatorStateType.LogicalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorderX.MonitorTrace($"Error: argument validate {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, Item.ApiName, "invalidate argument", Item.Argument);
                Item.Result = ApiResultIoc.LocalExceptionJson;
                Item.Status = UserOperatorStateType.LocalException;
                return ZeroOperatorStateType.LocalException;
            }
            return ZeroOperatorStateType.Ok;
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private object CommandExec(bool monitor, ApiAction action)
        {
            try
            {
                return action.Execute();
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
                if (monitor)
                    LogRecorderX.MonitorTrace($"Error: execute {e.Message}.");
                ZeroTrace.WriteException(Station.StationName, e, Item.ApiName, "execute", JsonHelper.SerializeObject(Item));
                Item.Result = ApiResultIoc.LocalExceptionJson;
                Item.Status = UserOperatorStateType.LocalException;
                return ZeroOperatorStateType.LocalException;
            }
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private ZeroOperatorStateType CheckCommandResult(object res)
        {
            switch (res)
            {
                case ZeroOperatorStateType state:
                    return state;
                case string str:
                    Item.Result = str;
                    Item.Status = UserOperatorStateType.Success;
                    return ZeroOperatorStateType.Ok;
                case ApiFileResult result:
                    if (result.Status == null)
                        result.Status = new ApiStatusResult { InnerMessage = Item.GlobalId };
                    else
                        result.Status.InnerMessage = Item.GlobalId;
                    Item.EndTag = ZeroFrameType.ResultFileEnd;
                    Item.Binary = result.Data;
                    result.Data = null;
                    Item.Status = result.Success ? UserOperatorStateType.Success : UserOperatorStateType.LogicalError;
                    Item.Result = JsonHelper.SerializeObject(result);
                    return result.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed;
                case IApiResult result:
                    if (result.Status == null)
                        result.Status = new ApiStatusResult { InnerMessage = Item.GlobalId };
                    else
                        result.Status.InnerMessage = Item.GlobalId;
                    Item.Result = JsonHelper.SerializeObject(result);
                    Item.Status = result.Success ? UserOperatorStateType.Success : UserOperatorStateType.LogicalError;
                    return result.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed;
            }


            if (res?.GetType().ToString().IndexOf("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", StringComparison.Ordinal) == 0)
            {
                dynamic resd = res;
                return CheckCommandResult(resd.Result);
            }
            Item.Result = ApiResultIoc.SucceesJson;
            Item.Status = UserOperatorStateType.Success;
            return ZeroOperatorStateType.Ok;
        }

        #endregion
    }
}