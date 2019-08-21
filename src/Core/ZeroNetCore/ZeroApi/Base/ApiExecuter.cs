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
    ///     Api调用
    /// </summary>
    public class ApiExecuter
    {
        internal ApiStationBase Station;

        internal ZSocket Socket;


        internal ApiCallItem Item;

        /// <summary>
        /// 调用 
        /// </summary>
        public async Task ExecuteAsync()
        {
            using (IocScope.CreateScope())
            {
                try
                {
                    if (LogRecorderX.LogMonitor)
                        await AsyncCallByMonitor();
                    else
                        await AsyncCallNoMonitor();
                }
                catch (Exception ex)
                {
                    ZeroTrace.WriteException(Station.StationName, ex, "ApiCall", Item.ApiName);
                    Item.Result = ApiResultIoc.InnerErrorJson;
                    Station.OnExecuestEnd(Socket, Item, ZeroOperatorStateType.Error);
                }
            }
        }


        #region 异步


        private async Task AsyncCallByMonitor()
        {
            using (MonitorScope.CreateScope($"{Station.StationName}/{Item.ApiName}"))
            {
                LogRecorderX.MonitorTrace($"Caller:{Encoding.ASCII.GetString(Item.Caller)}");
                LogRecorderX.MonitorTrace($"GlobalId:{Item.GlobalId}");
                LogRecorderX.MonitorTrace(JsonConvert.SerializeObject(Item, Formatting.Indented));

                ZeroOperatorStateType state = RestoryContext();

                if (state == ZeroOperatorStateType.Ok)
                {
                    using (MonitorScope.CreateScope("Prepare"))
                    {
                        Prepare();
                    }
                    await Task.Yield();
                    using (MonitorScope.CreateScope("Do"))
                    {
                        state = CommandPrepare(true, out var action);
                        await Task.Yield();
                        if (state == ZeroOperatorStateType.Ok)
                        {
                            var res = CommandExec(true, action);
                            await Task.Yield();
                            state = CheckCommandResult(res);
                        }
                    }

                    if (state != ZeroOperatorStateType.Ok)
                        Interlocked.Increment(ref Station.ErrorCount);
                    else
                        Interlocked.Increment(ref Station.SuccessCount);
                }
                else
                {
                    LogRecorderX.MonitorTrace("Restory context failed");
                    Interlocked.Increment(ref Station.ErrorCount);
                }
                LogRecorderX.MonitorTrace(Item.Result);
                if (!Station.OnExecuestEnd(Socket, Item, state))
                {
                    ZeroTrace.WriteError(Item.ApiName, "SendResult");
                    Interlocked.Increment(ref Station.SendError);
                }
                await Task.Yield();
                using (MonitorScope.CreateScope("End"))
                {
                    End();
                }
            }
        }

        private async Task AsyncCallNoMonitor()
        {
            ZeroOperatorStateType state = RestoryContext();
            if (state == ZeroOperatorStateType.Ok)
            {
                Prepare();
                await Task.Yield();
                state = CommandPrepare(false, out var action);
                await Task.Yield();
                if (state == ZeroOperatorStateType.Ok)
                {
                    var res = CommandExec(false, action);
                    await Task.Yield();
                    state = CheckCommandResult(res);
                }

                if (state != ZeroOperatorStateType.Ok)
                    Interlocked.Increment(ref Station.ErrorCount);
                else
                    Interlocked.Increment(ref Station.SuccessCount);
            }
            else
            {
                Interlocked.Increment(ref Station.ErrorCount);
            }
            if (!Station.OnExecuestEnd(Socket, Item, state))
            {
                Interlocked.Increment(ref Station.SendError);
            }
            await Task.Yield();
            End();
        }


        #endregion
        
        #region 同步

        /// <summary>
        /// 调用 
        /// </summary>
        public void Execute()
        {
            using (IocScope.CreateScope())
            {
                GlobalContext.Current.DependencyObjects.Annex(Item);
                try
                {
                    if (LogRecorderX.LogMonitor)
                        ApiCallByMonitor();
                    else
                        ApiCallNoMonitor();
                }
                catch (Exception ex)
                {
                    ZeroTrace.WriteException(Station.StationName, ex, "ApiCall", Item.ApiName);
                    Item.Result = ApiResultIoc.InnerErrorJson;
                    Station.OnExecuestEnd(Socket, Item, ZeroOperatorStateType.Error);
                }
            }
        }

        private void ApiCallByMonitor()
        {
            using (MonitorScope.CreateScope($"{Station.StationName}/{Item.ApiName}"))
            {
                LogRecorderX.MonitorTrace($"Caller:{Encoding.ASCII.GetString(Item.Caller)}");
                LogRecorderX.MonitorTrace($"GlobalId:{Item.GlobalId}");
                LogRecorderX.MonitorTrace(JsonConvert.SerializeObject(Item, Formatting.Indented));

                ZeroOperatorStateType state = RestoryContext();

                if (state == ZeroOperatorStateType.Ok)
                {
                    using (MonitorScope.CreateScope("Prepare"))
                    {
                        Prepare();
                    }
                    using (MonitorScope.CreateScope("Do"))
                    {
                        state = CommandPrepare(true, out var action);
                        if (state == ZeroOperatorStateType.Ok)
                        {
                            var res = CommandExec(true, action);
                            state = CheckCommandResult(res);
                        }
                    }

                    if (state != ZeroOperatorStateType.Ok)
                        Interlocked.Increment(ref Station.ErrorCount);
                    else
                        Interlocked.Increment(ref Station.SuccessCount);
                }
                else
                {
                    LogRecorderX.MonitorTrace("Restory context failed");
                    Interlocked.Increment(ref Station.ErrorCount);
                }
                LogRecorderX.MonitorTrace(Item.Result);
                if (!Station.OnExecuestEnd(Socket, Item, state))
                {
                    ZeroTrace.WriteError(Item.ApiName, "SendResult");
                    Interlocked.Increment(ref Station.SendError);
                }
                using (MonitorScope.CreateScope("End"))
                {
                    End();
                }
            }
        }

        private void ApiCallNoMonitor()
        {
            ZeroOperatorStateType state = RestoryContext();
            if (state == ZeroOperatorStateType.Ok)
            {
                Prepare();
                state = CommandPrepare(false, out var action);
                if (state == ZeroOperatorStateType.Ok)
                {
                    var res = CommandExec(false, action);
                    state = CheckCommandResult(res);
                }

                if (state != ZeroOperatorStateType.Ok)
                    Interlocked.Increment(ref Station.ErrorCount);
                else
                    Interlocked.Increment(ref Station.SuccessCount);
            }
            else
            {
                Interlocked.Increment(ref Station.ErrorCount);
            }
            if (!Station.OnExecuestEnd(Socket, Item, state))
            {
                Interlocked.Increment(ref Station.SendError);
            }
            End();
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

            //4 方法执行
            try
            {
                GlobalContext.Current.DependencyObjects.Annex(action);
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
                default:
                    Item.Result = ApiResultIoc.SucceesJson;
                    Item.Status = UserOperatorStateType.Success;
                    return ZeroOperatorStateType.Ok;
            }
        }

        #endregion
    }
}