using System;
using System.Collections.Generic;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
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
            _data = data;
            _words = data.ApiName.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (_words.Length == 0)
            {
                data.Status = RouteStatus.FormalError;
                data.ResultMessage = RouteRuntime.ArgumentErrorJson;
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
                _result = ApiResult.Error(ErrorCode.ArgumentError);
                return;
            }

            var stationName = _words[2];
            StationConfig config;
            lock (ZeroApplication.Configs)
            {
                if (ZeroApplication.Configs.TryGetValue(stationName, out config))
                {
                    _result = ApiResult<StationConfig>.Succees(config);
                    _result.Status = new ApiStatsResult
                    {
                        ErrorCode = ErrorCode.Success,
                        ClientMessage = "站点已存在"
                    };
                    return;
                }
            }

            if (ZeroApplication.State != StationState.Run)
            {
                _result = ApiResult.Error(ErrorCode.NoReady);
                return;
            }

            string type = _words[1];
            List<string> datas;
            try
            {
                datas = ZeroApplication.ZeroManageAddress.MulitRequestNet("install", type, stationName);
                if (datas.Count == 0)
                {
                    _result = ApiResult.Error(ErrorCode.NetworkError);
                    return;
                }
                switch (datas[0])
                {
                    case null:
                        _result = ApiResult.Error(ErrorCode.NetworkError);
                        return;
                    case ZeroNetStatus.ZeroCommandNoSupport:
                        _result = ApiResult.Error(ErrorCode.UnknowError, $"类型{type}不支持");
                        return;
                    case ZeroNetStatus.ZeroCommandFailed:
                        _result = ApiResult.Error(ErrorCode.UnknowError, datas[0]);
                        return;
                    case ZeroNetStatus.ZeroCommandOk:
                        _result = ApiResult.Succees();
                        _result.Status = new ApiStatsResult
                        {
                            ErrorCode = ErrorCode.Success,
                            ClientMessage = "安装成功"
                        };
                        return;
                    default:
                        _result = ApiResult.Error(ErrorCode.UnknowError, datas[0]);
                        return;
                }
            }
            catch (Exception e)
            {
                _result = ApiResult.Error(ErrorCode.NetworkError, e.Message);
                return;
            }

            try
            {
                config = JsonConvert.DeserializeObject<StationConfig>(datas[0]);
            }
            catch
            {
                _result = ApiResult.Error(ErrorCode.UnknowError, "返回值不正确");
                return;
            }
            lock (ZeroApplication.Configs)
            {
                ZeroApplication.Configs.Add(stationName, config);
            }
            _result = ApiResult<StationConfig>.Succees(config);
            _result.Status = new ApiStatsResult
            {
                ErrorCode = ErrorCode.Success,
                ClientMessage = "安装成功"
            };
        }

        private void Call()
        {
            if (_words.Length < 2)
            {
                _data.Status = RouteStatus.FormalError;
                _result = ApiResult.Error(ErrorCode.ArgumentError);
                return;
            }
            
            if (ZeroApplication.State != StationState.Run)
            {
                _result = ApiResult.Error(ErrorCode.NoReady);
                return;
            }

            try
            {
                var datas = ZeroApplication.ZeroManageAddress.MulitRequestNet(_words);
                if (datas.Count == 0)
                {
                    _result = ApiResult.Error(ErrorCode.NetworkError);
                    return;
                }
                switch (datas[0])
                {
                    case null:
                        _result = ApiResult.Error(ErrorCode.NetworkError);
                        return;
                    case ZeroNetStatus.ZeroCommandNoSupport:
                        _result = ApiResult.Error(ErrorCode.UnknowError, "不支持的操作");
                        return;
                    case ZeroNetStatus.ZeroCommandFailed:
                        _result = ApiResult.Error(ErrorCode.UnknowError, datas[0]);
                        return;
                    case ZeroNetStatus.ZeroCommandOk:
                        _result = ApiValueResult.Succees(datas[0]);
                        _result.Status = new ApiStatsResult
                        {
                            ErrorCode = ErrorCode.Success,
                            ClientMessage = "操作成功"
                        };
                        return;
                    default:
                        _result = ApiResult.Error(ErrorCode.UnknowError, datas[0]);
                        return;
                }
            }
            catch (Exception e)
            {
                _result = ApiResult.Error(ErrorCode.NetworkError, e.Message);
                return;
            }
        }
    }
}