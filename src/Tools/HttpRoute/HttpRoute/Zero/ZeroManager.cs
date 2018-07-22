using System;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{

    /// <summary>
    ///     管理类
    /// </summary>
    internal class ZeroManager
    {
        private RouteData _data;
        private string[] _words;
        private ApiResult _result;
        public void Command(RouteData data)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                _result = ApiResult.Error(ErrorCode.NoReady);
                return;
            }
            _data = data;
            _words = data.ApiName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (_words.Length == 0)
            {
                data.Status = RouteStatus.FormalError;
                data.ResultMessage = ApiResult.ArgumentErrorJson;
                return;
            }

            switch (_words[0].ToLower())
            {
                case "install":
                    Install();
                    break;
                default:
                    Call();
                    break;
            }
            _data.ResultMessage = JsonConvert.SerializeObject(_result);
        }

        private void Install()
        {
            if (_words.Length < 3)
            {
                _data.Status = RouteStatus.FormalError;
                _result = ApiResult.Error(ErrorCode.LogicalError);
                return;
            }
            if (ZeroApplication.Config.TryGetConfig(_words[2], out var config))
            {
                _result = ApiResult<StationConfig>.Succees(config);
                _result.Status = new ApiStatusResult
                {
                    ErrorCode = ErrorCode.Success,
                    ClientMessage = "站点已存在"
                };
                return;
            }


            string type = _words[1];
            try
            {
                var result = SystemManager.Instance.CallCommand(_words);
                if (!result.InteractiveSuccess)
                {
                    _result = ApiResult.Error(ErrorCode.NetworkError,"服务器无法访问");
                    return;
                }
                switch (result.State)
                {
                    case ZeroOperatorStateType.ArgumentInvalid:
                        _result = ApiResult.Error(ErrorCode.LogicalError, $"命令格式错误:{result.State.Text()}");
                        return;
                    case ZeroOperatorStateType.NotSupport:
                        _result = ApiResult.Error(ErrorCode.LogicalError, $"类型{type}不支持");
                        return;
                    case ZeroOperatorStateType.Failed:
                        _result = ApiResult.Error(ErrorCode.LogicalError, "已存在");
                        _result.Status = new ApiStatusResult
                        {
                            ErrorCode = ErrorCode.Success,
                            ClientMessage = "安装成功"
                        };
                        return;
                    case ZeroOperatorStateType.Ok:
                        _result = ApiResult.Succees();
                        _result.Status = new ApiStatusResult
                        {
                            ErrorCode = ErrorCode.Success,
                            ClientMessage = "安装成功"
                        };
                        return;
                    default:
                        _result = ApiResult.Error(ErrorCode.LogicalError, result.State.Text());
                        return;
                }
            }
            catch (Exception e)
            {
                _result = ApiResult.Error(ErrorCode.NetworkError, e.Message);
            }

            //try
            //{
            //    config = JsonConvert.DeserializeObject<StationConfig>(datas[0]);
            //}
            //catch
            //{
            //    _result = ApiResult.Error(ErrorCode.UnknowError, "返回值不正确");
            //    return;
            //}
            //lock (ZeroApplication.Config.Configs)
            //{
            //    ZeroApplication.Config.Configs.Add(stationName, config);
            //}
            //_result = ApiResult<StationConfig>.Succees(config);
            //_result.Status = new ApiStatusResult
            //{
            //    ErrorCode = ErrorCode.Success,
            //    ClientMessage = "安装成功"
            //};
        }

        private void Call()
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                _result = ApiResult.Error(ErrorCode.NoReady);
                return;
            }
            if (_words.Length < 2)
            {
                _data.Status = RouteStatus.FormalError;
                _result = ApiResult.Error(ErrorCode.LogicalError,"参数错误");
                return;
            }
            

            var value = SystemManager.Instance.CallCommand(_words);
            if (!value.InteractiveSuccess)
            {
                _result = ApiResult.Error(ErrorCode.NetworkError);
                return;
            }
            switch (value.State)
            {
                case ZeroOperatorStateType.NotSupport:
                    _result = ApiResult.Error(ErrorCode.LogicalError, "不支持的操作");
                    return;
                case ZeroOperatorStateType.Ok:
                    _result = ApiValueResult.Succees(value.GetValue(ZeroFrameType.Context) ?? value.State.Text());
                    _result.Status = new ApiStatusResult
                    {
                        ErrorCode = ErrorCode.Success,
                        ClientMessage = "操作成功"
                    };
                    return;
                default:
                    _result = ApiResult.Error(ErrorCode.LogicalError, value.State.Text());
                    return;
            }
        }
    }
}