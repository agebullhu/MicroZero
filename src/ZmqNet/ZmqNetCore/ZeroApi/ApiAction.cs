using System;
using System.Reflection;
using Agebull.Common.Logging;
using Newtonsoft.Json;
using Agebull.ZeroNet.ZeroApi;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// Api站点
    /// </summary>
    public abstract class ApiAction
    {
        /// <summary>
        /// Api名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 需要登录
        /// </summary>
        public bool NeedLogin { get; set; }
        /// <summary>
        /// 是否公开接口
        /// </summary>
        public bool IsPublic { get; set; }
        /// <summary>
        /// 还原参数
        /// </summary>
        public abstract bool RestoreArgument(string argument);

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool Validate(out string message);

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public abstract IApiResult Execute();
    }
    /// <summary>
    /// Api动作
    /// </summary>
    public sealed class ApiAction<TResult> : ApiAction
        where TResult : IApiResult
    {
        /// <summary>
        /// 还原参数
        /// </summary>
        public override bool RestoreArgument(string argument)
        {
            return true;
        }

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Validate(out string message)
        {
            message = null;
            return true;
        }

        /// <summary>
        /// 执行行为
        /// </summary>
        public Func<TResult> Action { get; set; }
        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public override IApiResult Execute()
        {
            return Action();
        }
    }
    /// <summary>
    /// API标准委托
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public delegate IApiResult ApiDelegate(IApiArgument argument);

    /// <summary>
    /// API标准委托
    /// </summary>
    /// <returns></returns>
    public delegate IApiResult ApiDelegate2();

    /// <summary>
    /// Api动作
    /// </summary>
    public sealed class AnyApiAction<TControler> : ApiAction
        where TControler : class, new()
    {
        /// <summary>
        /// 参数类型
        /// </summary>
        public Type ArgumenType { get; set; }

        private IApiArgument _argument;
        /// <summary>
        /// 还原参数
        /// </summary>
        public override bool RestoreArgument(string argument)
        {
            if (ArgumenType == null)
                return true;
            try
            {
                _argument = JsonConvert.DeserializeObject(argument, ArgumenType) as IApiArgument;
                return _argument != null;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return false;
            }
        }

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Validate(out string message)
        {
            if (_argument == null)
            {
                message = "参数不能为空";
                return false;
            }
            return _argument.Validate(out message);
        }
        /// <summary>
        /// 执行行为
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public override IApiResult Execute()
        {
            if (ArgumenType == null)
            {
                var action = Method.CreateDelegate(typeof(ApiDelegate2),new TControler());
                return action.DynamicInvoke() as IApiResult;
            }
            else
            {
                var action = Method.CreateDelegate(typeof(ApiDelegate),new TControler());
                return action.DynamicInvoke(_argument) as IApiResult;
            }
        }
    }

    /// <summary>
    /// Api动作
    /// </summary>
    public sealed class ApiAction<TArgument, TResult> : ApiAction
        where TArgument : class, IApiArgument
        where TResult : IApiResult
    {
        private TArgument _argument;
        /// <summary>
        /// 还原参数
        /// </summary>
        public override bool RestoreArgument(string argument)
        {
            try
            {
                _argument = JsonConvert.DeserializeObject<TArgument>(argument);
                return _argument != null;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return false;
            }
        }

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Validate(out string message)
        {
            return _argument.Validate(out message);
        }

        /// <summary>
        /// 执行行为
        /// </summary>
        public Func<TArgument, TResult> Action { get; set; }
        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public override IApiResult Execute()
        {
            return Action(_argument);
        }
    }
}