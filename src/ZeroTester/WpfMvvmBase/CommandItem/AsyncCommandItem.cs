using System;
using System.Diagnostics;
using System.Windows;

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     表示一个命令节点
    /// </summary>
    public class AsyncCommandItem<TParameter, TResult> : CommandItemBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        public AsyncCommandItem(Func<TParameter, bool> prepare, Func<TParameter, TResult> exceute, Action<CommandStatus, Exception, TResult> end)
        {
            Prepare = prepare;
            Exceute = exceute;
            End = end;
            TargetType = typeof(TParameter);
            Command = new AsyncCommand<TParameter, TResult>(DoPrepare, Exceute, DoEnd);
        }
        /// <summary>
        /// 构造
        /// </summary>
        public AsyncCommandItem(Func<TParameter, Action<TParameter>, bool> prepare, Func<TParameter, TResult> exceute, Action<CommandStatus, Exception, TResult> end)
        {
            Prepare3 = prepare;
            Exceute = exceute;
            End = end;
            TargetType = typeof(TParameter);
            Command = new AsyncCommand<TParameter, TResult>(DoPrepare, Exceute, DoEnd);
        }

        /// <summary>
        /// 构造
        /// </summary>
        public AsyncCommandItem(Func<TParameter> prepare, Func<TParameter, TResult> exceute, Action<CommandStatus, Exception, TResult> end)
        {
            Prepare2 = prepare;
            Exceute = exceute;
            End = end;
            TargetType = typeof(TParameter);
            Command = new AsyncCommand<TParameter, TResult>(DoPrepare, Exceute, DoEnd);
        }

        #region 命令

        private bool DoPrepare(TParameter parameter, Action<TParameter> action)
        {
            if (DoConfirm && 
                MessageBox.Show(ConfirmMessage ?? $"确认执行【{Caption}】操作吗?", "对象编辑", MessageBoxButton.YesNo) !=
                MessageBoxResult.Yes)
            {
                return false;
            }
            if (OnPrepare != null && !OnPrepare(this))
                return false;

            return DoPrepareInner( parameter, action);
        }


        private bool DoPrepareInner(TParameter parameter, Action<TParameter> action)
        {
            if (Prepare3 != null)
            {
                if (!Prepare3(parameter, action))
                    return false;
            }
            else
            {
                if (Prepare != null)
                {
                    if (!Prepare(parameter))
                        return false;
                }
                else if (Prepare2 != null)
                {
                    parameter = Prepare2();
                    if (Equals(parameter, default(TParameter)))
                        return false;
                }
                action?.Invoke(parameter);
            }
            return true;
        }
        private void DoEnd(CommandStatus status, Exception exception, TResult result)
        {
            if (End != null)
            {
                End(status, exception, result);
            }
            else if (exception != null)
            {
                MessageBox.Show($"发生异常【{exception.Message}】", "对象编辑");
                Trace.WriteLine(exception, "DoEnd");
            }
            else if (status != CommandStatus.Succeed)
            {
                MessageBox.Show("未成功", "对象编辑");
            }
        }
        /// <summary>
        /// 准备行为
        /// </summary>
        public Func<TParameter> Prepare2;

        /// <summary>
        /// 准备行为
        /// </summary>
        public Func<TParameter, Action<TParameter>, bool> Prepare3;

        /// <summary>
        /// 准备行为
        /// </summary>
        public Func<TParameter, bool> Prepare;
        /// <summary>
        /// 执行行为
        /// </summary>
        public Func<TParameter, TResult> Exceute;
        /// <summary>
        /// 结束行为
        /// </summary>
        public Action<CommandStatus, Exception, TResult> End;
        #endregion


        /// <summary>
        /// 执行
        /// </summary>
        public override void Execute(object arg)
        {
            TParameter parameter = (TParameter)arg;
            if (!DoPrepareInner(parameter, p => parameter = p))
                return;
            try
            {
                var result = Exceute.Invoke(parameter);
                End(CommandStatus.Succeed, null, result);
            }
            catch (Exception e)
            {
                End(CommandStatus.Faulted, e, default(TResult));
            }
        }
    }
}