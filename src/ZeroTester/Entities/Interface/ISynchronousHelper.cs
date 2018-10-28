#if CLIENT
using System;

namespace Agebull.EntityModel
{
    /// <summary>
    /// 同步助手
    /// </summary>
    public interface ISynchronousContext
    {
        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        void InvokeInUiThread(Action action);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        void BeginInvokeInUiThread(Action action);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args"></param>
        void BeginInvokeInUiThread<T>(Action<T> action, T args);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        void BeginInvokeInUiThread<T1, T2>(Action<T1, T2> action, T1 args1, T2 args2);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        /// <param name="arg3"></param>
        void BeginInvokeInUiThread<T1, T2, T3>(Action<T1, T2, T3> action, T1 args1, T2 args2, T3 arg3);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        void BeginInvokeInUiThread<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 args1, T2 args2, T3 arg3, T4 arg4);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args"></param>
        void InvokeInUiThread<T>(Action<T> action, T args);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        void InvokeInUiThread<T1, T2>(Action<T1, T2> action, T1 args1, T2 args2);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        /// <param name="arg3"></param>
        void InvokeInUiThread<T1, T2, T3>(Action<T1, T2, T3> action, T1 args1, T2 args2, T3 arg3);

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        void InvokeInUiThread<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 args1, T2 args2, T3 arg3, T4 arg4);
    }
}
#endif