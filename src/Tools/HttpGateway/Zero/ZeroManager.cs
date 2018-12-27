using System;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace ZeroNet.Http.Gateway
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
                _result = ApiResult.NoReady;
                return;
            }
            _data = data;
            _words = data.ApiName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (_words.Length == 0)
            {
                data.Status = ZeroOperatorStatus.FormalError;
                data.ResultMessage = ApiResult.ArgumentErrorJson;
                return;
            }

            Call();
            _data.ResultMessage = JsonConvert.SerializeObject(_result);
        }

        private void Call()
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                _result = ApiResult.NoReady;
                return;
            }
            if (_words.Length < 2)
            {
                _data.Status = ZeroOperatorStatus.FormalError;
                _data.ResultMessage = ApiResult.ArgumentErrorJson;
                return;
            }            
            var value = SystemManager.Instance.CallCommand(_words);
            if (!value.InteractiveSuccess)
            {
                _result = ApiResult.NetworkError;
                return;
            }
            switch (value.State)
            {
                case ZeroOperatorStateType.NotSupport:
                    _result = ApiResult.NotSupport;
                    return;
                case ZeroOperatorStateType.Ok:
                    _result = ApiValueResult.Succees(value.GetValue(ZeroFrameType.Context) ?? value.State.Text());
                    return;
                default:
                    _result = ApiResult.Error(ErrorCode.LogicalError, value.Message,value.ToString());
                    return;
            }
        }
    }
}