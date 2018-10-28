using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Agebull.Common.Logging;
using Agebull.Common.Mvvm;

namespace Agebull.EntityModel
{


    /// <summary>
    /// 表示Task表示异步代理对象
    /// </summary>
    public class StepTackProxy : ITackProxy
    {
        /// <summary>
        /// 表示Task表示异步代理对象的一个执行实例
        /// </summary>
        private sealed class TackStepItem
        {

            /// <summary>
            /// 步骤标识,如果异步执行结束标识不同,这次执行结果将被放弃
            /// </summary>
            public Guid StepKey
            {
                get;
                set;
            }


            /// <summary>
            /// 执行的方法
            /// </summary>
            public Action Action
            {
                get;
                set;
            }

        }

        /// <summary>
        ///     同步命令线程调度器
        /// </summary>
        public Dispatcher Dispatcher
        {
            get;
            set;
        }

        /// <summary>
        /// 步骤标识,如果异步执行结束标识不同,这次执行结果将被放弃
        /// </summary>
        public Guid StepKey
        {
            get;
            set;
        }

        /// <summary>
        ///     当前状态
        /// </summary>
        public CommandStatus Status
        {
            get;
            set;
        }


        /// <summary>
        ///     结束执行方法
        /// </summary>
        public Action Action
        {
            get;
            set;
        }

        /// <summary>
        /// 能否执行
        /// </summary>
        /// <returns>null表示取消任务,treu表示可以立即执行,false表示应该等待前一个任务完成后执行</returns>
        public bool? CanDo()
        {
            return true;
        }

        private Task _task;

        private CancellationToken _token;

        public void Run(Action task)
        {
            try
            {
                Status = CommandStatus.Executing;
                _token = new CancellationToken(false);
                _task = new Task(DoExecute, new TackStepItem { StepKey = StepKey, Action = task }, _token);
                _task.ContinueWith(OnEnd, TaskContinuationOptions.None);
                _task.Start();
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                Status = CommandStatus.Faulted;
                Exist();
            }
        }
        private void DoExecute(object arg)
        {
            lock (this)
            {
                var item = arg as TackStepItem;
                // ReSharper disable PossibleNullReferenceException
                if (item.StepKey == StepKey)
                    item.Action();
                // ReSharper restore PossibleNullReferenceException
            }
        }

        private void OnEnd(Task task)
        {
            lock (this)
            {
                // ReSharper disable PossibleNullReferenceException
                var item = task.AsyncState as TackStepItem;
                if (item.StepKey != StepKey)
                    return;
                // ReSharper restore PossibleNullReferenceException
                _task = null;
                switch (task.Status)
                {
                    case TaskStatus.Faulted:
                        LogRecorder.Exception(task.Exception);
                        Status = CommandStatus.Faulted;
                        break;
                    default:
                        Status = CommandStatus.Succeed;
                        break;
                }
                Dispatcher.BeginInvoke(new Action(Exist));
            }
        }

        public void Exist()
        {
            if (Action != null)
                Action();
        }
    }



    /// <summary>
    /// 表示Task表示异步代理对象(当执行完成的步骤标识不相同时执行结果将被放弃)
    /// </summary>
    public sealed class StepTackProxy<TResult> : ITackProxy<TResult>
    {
        /// <summary>
        /// 表示Task表示异步代理对象的一个执行实例
        /// </summary>
        private sealed class TackStepItem
        {

            /// <summary>
            /// 步骤标识,如果异步执行结束标识不同,这次执行结果将被放弃
            /// </summary>
            public Guid StepKey
            {
                get;
                set;
            }


            /// <summary>
            /// 执行的方法
            /// </summary>
            public Func<TResult> Action
            {
                get;
                set;
            }


        }

        /// <summary>
        ///     同步命令线程调度器
        /// </summary>
        public Dispatcher Dispatcher
        {
            get;
            set;
        }

        /// <summary>
        /// 步骤标识,如果异步执行结束标识不同,这次执行结果将被放弃
        /// </summary>
        public Guid StepKey
        {
            get;
            set;
        }

        /// <summary>
        ///     当前状态
        /// </summary>
        public CommandStatus Status
        {
            get;
            set;
        }

        /// <summary>
        ///     结束执行方法
        /// </summary>
        public Action<TResult> Action
        {
            get;
            set;
        }

        public void Run(Func<TResult> func)
        {
            try
            {
                Status = CommandStatus.Executing;
                var task = new Task<TResult>(DoExecute, new TackStepItem { StepKey = StepKey, Action = func });
                task.ContinueWith(OnEnd, TaskContinuationOptions.None);
                task.Start();
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                Status = CommandStatus.Faulted;
                Exist();
            }
        }
        private TResult DoExecute(object arg)
        {
            lock (this)
            {
                var item = arg as TackStepItem;
                // ReSharper disable PossibleNullReferenceException
                return item.StepKey != StepKey ? default(TResult) : item.Action();
                // ReSharper restore PossibleNullReferenceException
            }
        }
        private void OnEnd(Task<TResult> task)
        {
            // ReSharper disable PossibleNullReferenceException
            lock (this)
            {
                var item = task.AsyncState as TackStepItem;
                if (item.StepKey != StepKey)
                    return;
                switch (task.Status)
                {
                    case TaskStatus.Faulted:
                        LogRecorder.Exception(task.Exception);
                        Status = CommandStatus.Faulted;
                        break;
                    default:
                        Status = CommandStatus.Succeed;
                        break;
                }
                Dispatcher.BeginInvoke(new Action<TResult>(Exist), task.Result);
            }
            // ReSharper restore PossibleNullReferenceException
        }
        /// <summary>
        /// 执行完成
        /// </summary>
        /// <param name="result"></param>
        public void Exist(TResult result = default(TResult ))
        {
            if (Action != null)
                Action(result);
        }
    }
}