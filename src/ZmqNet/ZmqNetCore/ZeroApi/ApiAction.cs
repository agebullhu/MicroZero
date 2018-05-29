using System;
using System.Reflection;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// Api方法的信息
    /// </summary>
    public class ApiActionInfo
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string FunctionName { get; set; }
        /// <summary>
        /// Api路由名称
        /// </summary>
        public string RouteName { get; set; }
        /// <summary>
        /// 访问设置
        /// </summary>
        public ApiAccessOption AccessOption { get; set; }

        /// <summary>
        /// 是否有调用参数
        /// </summary>
        public bool HaseArgument { get; set; }

        /// <summary>
        /// 访问设置
        /// </summary>
        public Func<IApiResult> Action { get; set; }

        /// <summary>
        /// 访问设置
        /// </summary>
        public Func<IApiArgument, IApiResult> ArgumentAction { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type ArgumenType { get; set; }

    }
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
        /// 访问控制
        /// </summary>
        public ApiAccessOption Access { get; set; }

        /// <summary>
        /// 需要登录
        /// </summary>
        public bool NeedLogin => Access.HasSomeFlags(ApiAccessOption.Customer, ApiAccessOption.Business, ApiAccessOption.Employe);
        /// <summary>
        /// 是否公开接口
        /// </summary>
        public bool IsPublic => Access.HasFlag(ApiAccessOption.Public);
        /// <summary>
        /// 参数类型
        /// </summary>
        public Type ArgumenType { get; set; }
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
    public delegate IApiResult ApiDelegate1(IApiArgument argument);

    /// <summary>
    /// API标准委托
    /// </summary>
    /// <returns></returns>
    public delegate IApiResult ApiDelegate();

    /// <summary>
    /// Api动作
    /// </summary>
    public sealed class AnyApiAction<TControler> : ApiAction
        where TControler : class, new()
    {
        /// <summary>
        /// 参数
        /// </summary>
        private IApiArgument _argument;
        /// <summary>
        /// 还原参数
        /// </summary>
        public override bool RestoreArgument(string argument)
        {
            if (ArgumenType == null)
                return true;
            _argument = JsonConvert.DeserializeObject(argument, ArgumenType) as IApiArgument;
            return true;
        }

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Validate(out string message)
        {
            if (_argument != null)
                return _argument.Validate(out message);
            if (Access.HasFlag(ApiAccessOption.ArgumentCanNil))
            {
                message = null;
                return true;
            }
            message = "参数不能为空";
            return false;
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
                var action = Method.CreateDelegate(typeof(ApiDelegate), new TControler());
                return action.DynamicInvoke() as IApiResult;
            }
            else
            {
                var action = Method.CreateDelegate(typeof(ApiDelegate1), new TControler());
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
        /// <summary>
        /// 参数
        /// </summary>
        private TArgument _argument;
        /// <summary>
        /// 还原参数
        /// </summary>
        public override bool RestoreArgument(string argument)
        {
            if (ArgumenType != null)
                _argument = JsonConvert.DeserializeObject(argument, ArgumenType) as TArgument;
            else
                _argument = JsonConvert.DeserializeObject<TArgument>(argument);
            return _argument != null;
        }

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Validate(out string message)
        {
            if (_argument != null)
                return _argument.Validate(out message);
            if (Access.HasFlag(ApiAccessOption.ArgumentCanNil))
            {
                message = null;
                return true;
            }
            message = "参数不能为空";
            return false;
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