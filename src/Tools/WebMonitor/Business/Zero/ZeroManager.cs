using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using WebMonitor.Models;

namespace WebMonitor.Models
{

    /// <summary>
    ///     管理类
    /// </summary>
    internal class ZeroManager
    {
        public static ApiResult Command(params string[] commands)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                return ApiResult.Error(ErrorCode.LocalError, "系统未就绪");
            }
            if (commands.Length == 0 || commands.Any(p => p == null))
            {
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            }
            var value = SystemManager.Instance.CallCommand(commands.ToArray());
            if (!value.InteractiveSuccess)
            {
                return ApiResult.Error(ErrorCode.NetworkError);
            }
            switch (value.State)
            {
                case ZeroOperatorStateType.NotSupport:
                    return ApiResult.Error(ErrorCode.LogicalError, "不支持的操作");
                case ZeroOperatorStateType.Ok:
                    var _result = ApiValueResult.Succees(value.GetValue(ZeroFrameType.Context) ?? value.State.Text());
                    _result.Status = new ApiStatsResult
                    {
                        ErrorCode = ErrorCode.Success,
                        ClientMessage = "操作成功"
                    };
                    return _result;
                default:
                    return ApiResult.Error(ErrorCode.LogicalError, value.State.Text());
            }
        }

        public static ApiResult Update(StationConfig option)
        {
            if (!ZeroApplication.Config.TryGetConfig(option.Name, out var config))
            {
                return ApiResult.Error(ErrorCode.NetworkError, "站点不存在");
            }

            try
            {
                var result = SystemManager.Instance.CallCommand("update", JsonConvert.SerializeObject(option));
                if (!result.InteractiveSuccess)
                {
                    return ApiResult.Error(ErrorCode.NetworkError, "服务器无法访问");
                }
                switch (result.State)
                {
                    case ZeroOperatorStateType.Ok:
                        var apiResult = ApiResult.Succees();
                        apiResult.Status = new ApiStatsResult
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

        public static ApiResult Install(StationConfig option)
        {
            if (ZeroApplication.Config.TryGetConfig(option.Name, out var config))
            {
                var result= ApiResult<StationConfig>.Succees(config);
                result.Status = new ApiStatsResult
                {
                    ErrorCode = ErrorCode.LogicalError,
                    ClientMessage = "站点已存在"
                };
                return result;
            }

            try
            {
                var result = SystemManager.Instance.CallCommand("install", JsonConvert.SerializeObject(option));
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
                        apiResult.Status = new ApiStatsResult
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