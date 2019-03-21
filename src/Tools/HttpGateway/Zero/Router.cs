using Agebull.Common.Logging;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApi;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     Http配置类
    /// </summary>
    public class HttpOption
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否Https
        /// </summary>
        public bool IsHttps { get; set; }

        /// <summary>
        /// 证书文件路径
        /// </summary>
        public string CerFile { get; set; }

        /// <summary>
        /// 证书密码
        /// </summary>
        public string CerPwd { get; set; }
    }

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

            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                return CallApi(host);
            }
        }

        private string CallApi(ZeroHost zeroHost)
        {
            var form = JsonConvert.SerializeObject(Data.Arguments);
            var caller = new ApiClient
            {
                Station = zeroHost.Station,
                Commmand = Data.ApiName,
                Argument = Data.HttpContext ?? form,
                ExtendArgument = form,
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
                    return Data.ResultMessage = ApiResultIoc.NoFindJson;
                }
            }

            var form = JsonConvert.SerializeObject(Data.Arguments);
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
            
        }
    }
}