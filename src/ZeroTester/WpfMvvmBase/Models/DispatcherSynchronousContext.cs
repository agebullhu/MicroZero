using System;
using System.Windows.Threading;

namespace Agebull.EntityModel
{
    /// <summary>
    /// 线程调度器上下文 
    /// </summary>
    public sealed class DispatcherSynchronousContext : ISynchronousContext
    {
        /// <summary>
        /// 线程调度器
        /// </summary>
        public Dispatcher Dispatcher { get; set; }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        public void InvokeInUiThread(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.Invoke(action);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        public void BeginInvokeInUiThread<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 args1, T2 args2, T3 arg3, T4 arg4)
        {
            if (Dispatcher.CheckAccess())
            {
                action(args1, args2, arg3, arg4);
            }
            else
            {
                Dispatcher.BeginInvoke(action, args1, args2, arg3, arg4);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args"></param>
        public void InvokeInUiThread<T>(Action<T> action, T args)
        {
            if (Dispatcher.CheckAccess())
            {
                action(args);
            }
            else
            {
                Dispatcher.Invoke(action, args);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        public void InvokeInUiThread<T1, T2>(Action<T1, T2> action, T1 args1, T2 args2)
        {
            if (Dispatcher.CheckAccess())
            {
                action(args1, args2);
            }
            else
            {
                Dispatcher.Invoke(action, args1, args2);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        /// <param name="arg3"></param>
        public void InvokeInUiThread<T1, T2, T3>(Action<T1, T2, T3> action, T1 args1, T2 args2, T3 arg3)
        {
            if (Dispatcher.CheckAccess())
            {
                action(args1, args2, arg3);
            }
            else
            {
                Dispatcher.Invoke(action, args1, args2, arg3);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        public void InvokeInUiThread<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 args1, T2 args2, T3 arg3, T4 arg4)
        {
            if (Dispatcher.CheckAccess())
            {
                action(args1, args2, arg3, arg4);
            }
            else
            {
                Dispatcher.Invoke(action, args1, args2, arg3, arg4);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        public void BeginInvokeInUiThread(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.BeginInvoke(action);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args"></param>
        public void BeginInvokeInUiThread<T>(Action<T> action, T args)
        {
            if (Dispatcher.CheckAccess())
            {
                action(args);
            }
            else
            {
                Dispatcher.BeginInvoke(action, args);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        public void BeginInvokeInUiThread<T1, T2>(Action<T1, T2> action, T1 args1, T2 args2)
        {
            if (Dispatcher.CheckAccess())
            {
                action(args1, args2);
            }
            else
            {
                Dispatcher.BeginInvoke(action, args1, args2);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        /// <param name="arg3"></param>
        public void BeginInvokeInUiThread<T1, T2, T3>(Action<T1, T2, T3> action, T1 args1, T2 args2, T3 arg3)
        {
            if (Dispatcher.CheckAccess())
            {
                action(args1, args2, arg3);
            }
            else
            {
                Dispatcher.BeginInvoke(action, args1, args2, arg3);
            }
        }
    }
}