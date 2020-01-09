using System;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
using Agebull.MicroZero.ZeroApis;
using Agebull.EntityModel.Common;

namespace MicroZero.Http.Gateway
{

    /// <summary>
    ///     管理类
    /// </summary>
    internal class ZeroManager
    {
        private RouteData _data;
        private string[] _words;
        private IApiResult _result;
        public void Command(RouteData data)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                _result = ApiResultIoc.Ioc.NoReady;
                return;
            }
            _data = data;
            _words = data.ApiName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (_words.Length == 0)
            {
                data.UserState = UserOperatorStateType.FormalError;
                data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                data.ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return;
            }

            Call();
            _data.ResultMessage = JsonHelper.SerializeObject(_result);
        }

        private void Call()
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                _result = ApiResultIoc.Ioc.NoReady;
                return;
            }
            if (_words.Length < 2)
            {
                _data.UserState = UserOperatorStateType.FormalError;
                _data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                _data.ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return;
            }
            var value = ZeroCenterProxy.Master.CallCommand(_words);
            if (!value.InteractiveSuccess)
            {
                _result = ApiResultIoc.Ioc.NetworkError;
                return;
            }
            switch (value.State)
            {
                case ZeroOperatorStateType.NotSupport:
                    _result = ApiResultIoc.Ioc.NotSupport;
                    return;
                case ZeroOperatorStateType.Ok:
                    //BUG
                    _result = ApiValueResult.Succees(value.GetString(ZeroFrameType.Context) ?? value.State.Text());
                    return;
                default:
                    _result = ApiResultIoc.Ioc.Error(ErrorCode.LogicalError, value.ErrorMessage, value.ToString());
                    return;
            }
        }
    }
}