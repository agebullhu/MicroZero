using System;
using System.Linq;
using System.Threading.Tasks;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
using Agebull.EntityModel.Common;
using Agebull.MicroZero.ZeroApis;

namespace WebMonitor.Models
{

    /// <summary>
    ///     管理类
    /// </summary>
    public class ZeroManager
    {
        public static async Task<ApiResult> Command(params string[] commands)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                return ApiResult.Error(ErrorCode.LocalError, "系统未就绪");
            }
            if (commands.Length == 0 || commands.Any(p => p == null))
            {
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            }
            var value = await SystemManager.Instance.CallCommand(commands.ToArray());
            if (!value.InteractiveSuccess)
            {
                return ApiResult.Error(ErrorCode.NetworkError);
            }
            switch (value.State)
            {
                case ZeroOperatorStateType.NotSupport:
                    return ApiResult.Error(ErrorCode.LogicalError, "不支持的操作");
                case ZeroOperatorStateType.Ok:
                    //BUG
                    var result = ApiValueResult.Succees(value.GetString(ZeroFrameType.Context) ?? value.State.Text());
                    result.Status = new ApiStatusResult
                    {
                        ErrorCode = ErrorCode.Success,
                        ClientMessage = "操作成功"
                    };
                    return result;
                default:
                    return ApiResult.Error(ErrorCode.LogicalError, value.State.Text());
            }
        }

        public static async Task<ApiResult> Update(StationConfig option)
        {
            if (!ZeroApplication.Config.TryGetConfig(option.Name, out _))
            {
                return ApiResult.Error(ErrorCode.NetworkError, "站点不存在");
            }

            try
            {
                var result =await SystemManager.Instance.CallCommand("update", JsonHelper.SerializeObject(option));
                if (!result.InteractiveSuccess)
                {
                    return ApiResult.Error(ErrorCode.NetworkError, "服务器无法访问");
                }
                switch (result.State)
                {
                    case ZeroOperatorStateType.Ok:
                        var apiResult = ApiResult.Succees();
                        apiResult.Status = new ApiStatusResult
                        {
                            ErrorCode = ErrorCode.Success,
                            ClientMessage = "安装成功"
                        };
                        return apiResult;
                    default:
                        return ApiResult.Error(ErrorCode.LogicalError, result.State.Text());
                }
            }
            catch (Exception e)
            {
                return ApiResult.Error(ErrorCode.NetworkError, e.Message);
            }
        }

        public static async Task<ApiResult> Install(StationConfig option)
        {
            if (ZeroApplication.Config.TryGetConfig(option.Name, out var config))
            {
                var result= ApiResult<StationConfig>.Succees(config);
                result.Status = new ApiStatusResult
                {
                    ErrorCode = ErrorCode.LogicalError,
                    ClientMessage = "站点已存在"
                };
                return result;
            }

            try
            {
                var result =await SystemManager.Instance.CallCommand("install", JsonHelper.SerializeObject(option));
                if (!result.InteractiveSuccess)
                {
                    return ApiResult.Error(ErrorCode.NetworkError, "服务器无法访问");
                }
                switch (result.State)
                {
                    case ZeroOperatorStateType.ArgumentInvalid:
                        return ApiResult.Error(ErrorCode.LogicalError, $"命令格式错误:{result.State.Text()}");
                    case ZeroOperatorStateType.NotSupport:
                        return ApiResult.Error(ErrorCode.LogicalError, "类型不支持");
                    case ZeroOperatorStateType.Failed:
                        return ApiResult.Error(ErrorCode.LogicalError, "已存在");
                    case ZeroOperatorStateType.Ok:
                        var apiResult= ApiResult.Succees();
                        apiResult.Status = new ApiStatusResult
                        {
                            ErrorCode = ErrorCode.Success,
                            ClientMessage = "安装成功"
                        };
                        return apiResult;
                    default:
                        return ApiResult.Error(ErrorCode.LogicalError, result.State.Text());
                }
            }
            catch (Exception e)
            {
                return ApiResult.Error(ErrorCode.NetworkError, e.Message);
            }
        }

    }
}