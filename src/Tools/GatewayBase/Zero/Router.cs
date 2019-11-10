using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;

namespace MicroZero.Http.Gateway
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
        private Task<string> CallZeroTask()
        {
            return Task.Factory.StartNew(CallZero);
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private string CallZero()
        {
            if (!(Data.RouteHost is ZeroHost host))
            {
                LogRecorderX.MonitorTrace("Host Type Failed");
                return Data.ResultMessage;
            }

            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                var form = JsonHelper.SerializeObject(Data.Arguments);
                var caller = new ApiClient
                {
                    Station = host.Station,
                    Commmand = Data.ApiName,
                    Argument = Data.HttpContent ?? form,
                    ExtendArgument = form,
                    Files = Data.Files,
                    ContextJson = Data.GlobalContextJson
                };

                caller.CallCommand();

                Data.ZeroState = caller.State;
                Data.UserState = caller.State.ToOperatorStatus(true);
                caller.CheckStateResult();
                Data.IsFile = caller.ResultType == ZeroFrameType.ResultFileEnd;
                Data.ResultBinary = caller.Binary;
                return caller.Result;
            }
        }
        /*
        private string CallApi2(ZeroHost zeroHost)
        {
            var config = ZeroApplication.Config[zeroHost.Station];
            if (config == null)
            {
                Data.UserState = UserOperatorStateType.NotFind;
                Data.ZeroState = ZeroOperatorStateType.NotFind;
                {
                    return Data.ResultMessage = ApiResultIoc.NoFindJson;
                }
            }

            var form = JsonHelper.SerializeObject(Data.Arguments);
            switch (config.StationType)
            {
                case ZeroStationType.Api:
                case ZeroStationType.RouteApi:
                    var caller = new ApiClient
                    {
                        Station = zeroHost.Station,
                        Commmand = Data.ApiName,
                        Argument = Data.HttpContext ?? form,
                        ExtendArgument = form,
                        ContextJson = Data.GlobalContextJson
                    };
                    caller.CallCommand();
                    Data.ResultMessage = caller.Result;
                    Data.ZeroState = caller.State;
                    Data.UserState = UserOperatorStateType.NotFind;
                    return Data.ResultMessage = ApiResultIoc.NoFindJson;
                case ZeroStationType.Notify:
                case ZeroStationType.Queue:
                    if (ZeroPublisher.DoPublish(zeroHost.Station, Data.ApiName, Data.ApiName, Data.HttpContext))
                    {
                        Data.UserState = UserOperatorStateType.Success;
                        Data.ZeroState = ZeroOperatorStateType.Ok;
                        return Data.ResultMessage = ApiResultIoc.SucceesJson;
                    }
                    else
                    {
                        Data.UserState = UserOperatorStateType.NetWorkError;
                        Data.ZeroState = ZeroOperatorStateType.NetError;
                        return Data.ResultMessage = ApiResultIoc.NetworkErrorJson;
                    }
                default:
                    Data.UserState = UserOperatorStateType.NotFind;
                    Data.ZeroState = ZeroOperatorStateType.NotFind;
                    return Data.ResultMessage = ApiResultIoc.NoFindJson;
            }
            
        }*/
    }
}