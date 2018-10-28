using System;
using System.Windows.Input;
using System.Windows.Threading;
using Agebull.Common.Mvvm;

namespace Agebull.EntityModel
{
    /// <summary>
    /// MVVM模式的基类
    /// </summary>
    public abstract class MvvmBase : NotificationObject
    {
        /// <summary>
        /// 同步上下文
        /// </summary>
        public virtual ISynchronousContext Synchronous
        {
            get;
            set;
        }
        /// <summary>
        /// 线程调度器
        /// </summary>
        public virtual Dispatcher Dispatcher
        {
            get;
            set;
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令执行的方法.</param>
        protected ICommand CreateAsyncCommand<TParameter, TResult>(Func<TParameter, TResult> executeAction)
            where TParameter : class
        {
            return new AsyncCommand<TParameter, TResult>(executeAction);
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令执行的方法.</param>
        /// <param name="canExecuteAction">能否执行的方法.</param>
        protected ICommand CreateAsyncCommand<TParameter, TResult>(Func<TParameter, TResult> executeAction, 
            Func<TParameter, bool> canExecuteAction)
            where TParameter : class
        {
            return new AsyncCommand<TParameter, TResult>(executeAction,canExecuteAction);
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令执行的方法.</param>
        /// <param name="endAction">结束时执行的方法</param>
        /// <param name="canExecuteAction">能否执行的方法.</param>
        protected ICommand CreateAsyncCommand<TParameter, TResult>(Func<TParameter, TResult> executeAction,
                Action<CommandStatus, Exception, TResult> endAction,
                Func<TParameter, bool> canExecuteAction)
            where TParameter : class
        {
            return new AsyncCommand<TParameter, TResult>(executeAction,endAction,canExecuteAction);
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令执行的方法.</param>
        /// <param name="endAction">结束时执行的方法</param>
        /// <param name="prepare">执行前的方法</param>
        protected ICommand CreateAsyncCommand<TParameter, TResult>(Func<TParameter, bool> prepare,
                Func<TParameter, TResult> executeAction,
                Action<CommandStatus, Exception, TResult> endAction)
            where TParameter : class
        {
            return new AsyncCommand<TParameter, TResult>(prepare,executeAction,endAction);
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令执行的方法.</param>
        /// <param name="endAction">结束时执行的方法</param>
        /// <param name="prepare">执行前的方法</param>
        protected ICommand CreateAsyncCommand<TParameter, TResult>(Func<TParameter, Action<TParameter>, bool> prepare,
                Func<TParameter, TResult> executeAction,
                Action<CommandStatus, Exception, TResult> endAction)
            where TParameter : class
        {
            return new AsyncCommand<TParameter, TResult>(prepare, executeAction, endAction);
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令执行的方法.</param>
        /// <param name="endAction">结束时执行的方法</param>
        /// <param name="prepare">执行前的方法</param>
        /// <param name="canExecuteAction">能否执行的方法.</param>
        protected ICommand CreateAsyncCommand<TParameter, TResult>(Func<TParameter, bool> prepare,
                Func<TParameter, TResult> executeAction,
                Action<CommandStatus, Exception, TResult> endAction,
                Func<TParameter, bool> canExecuteAction)
            where TParameter : class
        {
            return new AsyncCommand<TParameter, TResult>(prepare,executeAction,endAction,canExecuteAction);
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令执行的方法.</param>
        /// <param name="endAction">结束时执行的方法</param>
        /// <param name="prepare">执行前的方法</param>
        /// <param name="canExecuteAction">能否执行的方法.</param>
        protected ICommand CreateAsyncCommand<TParameter, TResult>(Func<TParameter, Action<TParameter>, bool> prepare,
                Func<TParameter, TResult> executeAction,
                Action<CommandStatus, Exception, TResult> endAction,
                Func<TParameter, bool> canExecuteAction)
            where TParameter : class
        {
            return new AsyncCommand<TParameter, TResult>(prepare,executeAction,endAction,canExecuteAction);
        }
    }
}