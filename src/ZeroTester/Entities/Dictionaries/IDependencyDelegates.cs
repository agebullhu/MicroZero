using System;

namespace Agebull.EntityModel
{
    public interface IDependencyDelegates
    {
        #region 基本

        /// <summary>
        ///     附加方法
        /// </summary>
        void AnnexDelegate<TAction>(TAction action) where TAction : class;

        /// <summary>
        ///     是否已附加对象
        /// </summary>
        /// <typeparam name="TAction">对象类型(理论上限定为Action或Func这两种类型的委托)</typeparam>
        /// <returns>如果存在则返回方法,否则返回空</returns>
        bool HaseDelegate<TAction>() where TAction : class;

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        /// <typeparam name="TAction">对象类型</typeparam>
        /// <returns></returns>
        TAction GetDelegate<TAction>() where TAction : class;

        #endregion

        #region 同步执行

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </returns>
        TResult Execute<TResult>();

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </returns>
        TResult Execute<TArg1, TResult>(TArg1 arg1);

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </returns>
        TResult Execute<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </returns>
        TResult Execute<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </returns>
        TResult Execute<TArg1, TArg2, TArg3, TArg4, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </remarks>
        void Run();

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </remarks>
        void Run<TArg1>(TArg1 arg1);

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </remarks>
        void Run<TArg1, TArg2>(TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在并成功执行,返回对应的值,否则抛出异常
        /// </remarks>
        void Run<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法则执行,否则抛出异常
        /// </remarks>
        void Run<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        TResult TryExecute<TResult>();

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        TResult TryExecute<TArg1, TResult>(TArg1 arg1);

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        TResult TryExecute<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        TResult TryExecute<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        TResult TryExecute<TArg1, TArg2, TArg3, TArg4, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在则执行,否则跳过并不抛出异常
        /// </remarks>
        void TryRun();

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在则执行,否则跳过并不抛出异常
        /// </remarks>
        void TryRun<TArg1>(TArg1 arg1);

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在则执行,否则跳过并不抛出异常
        /// </remarks>
        void TryRun<TArg1, TArg2>(TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在则执行,否则跳过并不抛出异常
        /// </remarks>
        void TryRun<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <remarks>
        ///     如果方法存在则执行,否则跳过并不抛出异常
        /// </remarks>
        void TryRun<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
        #endregion

        #region 异步执行
        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncExecute<TResult>(Action<TResult> asyncAction);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncExecute<TArg1, TResult>(Action<TResult> asyncAction, TArg1 arg1);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncExecute<TArg1, TArg2, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncExecute<TArg1, TArg2, TArg3, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncExecute<TArg1, TArg2, TArg3, TArg4, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncRun(Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncRun<TArg1>(TArg1 arg1, Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncRun<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncRun<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncRun<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncTryExecute<TResult>(Action<TResult> asyncAction);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncTryExecute<TArg1, TResult>(Action<TResult> asyncAction, TArg1 arg1);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncTryExecute<TArg1, TArg2, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncTryExecute<TArg1, TArg2, TArg3, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void AsyncTryExecute<TArg1, TArg2, TArg3, TArg4, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </remarks>
        void AsyncTryRun(Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </remarks>
        void AsyncTryRun<TArg1>(TArg1 arg1, Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </remarks>
        void AsyncTryRun<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </remarks>
        void AsyncTryRun<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, Action asyncAction = null);

        /// <summary>
        ///     异步执行对应类型的方法(Action)
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        ///  </remarks>
        void AsyncTryRun<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, Action asyncAction = null);
        #endregion

        #region 异步代理执行
        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void Execute<TResult>(ITackProxy<TResult> proxy);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void Execute<TArg1, TResult>(ITackProxy<TResult> proxy, TArg1 arg1);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void Execute<TArg1, TArg2, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void Execute<TArg1, TArg2, TArg3, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void Execute<TArg1, TArg2, TArg3, TArg4, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void Run(ITackProxy proxy);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void Run<TArg1>(ITackProxy proxy, TArg1 arg1);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void Run<TArg1, TArg2>(ITackProxy proxy, TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void Run<TArg1, TArg2, TArg3>(ITackProxy proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法的调用asyncAction; 否则抛出异常
        /// </remarks>
        void Run<TArg1, TArg2, TArg3, TArg4>(ITackProxy proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void TryExecute<TResult>(ITackProxy<TResult> proxy);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void TryExecute<TArg1, TResult>(ITackProxy<TResult> proxy, TArg1 arg1);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void TryExecute<TArg1, TArg2, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void TryExecute<TArg1, TArg2, TArg3, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction; 否则抛出异常
        /// </remarks>
        void TryExecute<TArg1, TArg2, TArg3, TArg4, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </remarks>
        void TryRun(ITackProxy proxy);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </remarks>
        void TryRun<TArg1>(ITackProxy proxy, TArg1 arg1);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </remarks>
        void TryRun<TArg1, TArg2>(ITackProxy proxy, TArg1 arg1, TArg2 arg2);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </remarks>
        void TryRun<TArg1, TArg2, TArg3>(ITackProxy proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <remarks>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        ///  </remarks>
        void TryRun<TArg1, TArg2, TArg3, TArg4>(ITackProxy proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
        #endregion
    }
}