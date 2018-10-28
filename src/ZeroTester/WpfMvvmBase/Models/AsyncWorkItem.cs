using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Agebull.Common.Logging;
using Agebull.Common.Mvvm;

namespace Agebull.EntityModel
{
    /// <summary>
    ///     异步工作节点
    /// </summary>
    public abstract class AsyncJob<TParameter, TResult> : NotificationObject
        where TParameter : class
    {
        /// <summary>
        /// 转为命令
        /// </summary>
        /// <returns></returns>
        public ICommand ToCommand()
        {
            return new AsyncCommand<TParameter, TResult>(Perpare, Execute, End);
        }

        /// <summary>
        /// 转为命令
        /// </summary>
        /// <returns></returns>
        public CommandItem ToCommandItem()
        {
            return new CommandItem
            {
                //Action = ToCommand(),
                Name = JobName,
                Caption = JobName,
                Image = ImageName == null ? null : Application.Current.Resources[ImageName] as ImageSource
            };
        }

        private string _jobName;

        /// <summary>
        ///     任务名称
        /// </summary>
        public string JobName
        {
            get => _jobName;
            set
            {
                if (_jobName == value)
                {
                    return;
                }
                _jobName = value;
                RaisePropertyChanged(() => JobName);
            }
        }

        private string _imageName;

        /// <summary>
        ///     图标名称
        /// </summary>
        public string ImageName
        {
            get => _imageName;
            set
            {
                if (_imageName == value)
                {
                    return;
                }
                _imageName = value;
                RaisePropertyChanged(() => ImageName);
            }
        }

        private CommandStatus _status;

        /// <summary>
        ///     当前状态
        /// </summary>
        public CommandStatus Status
        {
            get => _status;
            set
            {
                if (_status == value)
                {
                    return;
                }
                _status = value;
                RaisePropertyChanged(() => Status);
                RaisePropertyChanged(() => IsBusy);
            }
        }

        /// <summary>
        ///     是否正忙
        /// </summary>
        public bool IsBusy => Status == CommandStatus.Executing;

        private TParameter _parameter;

        /// <summary>
        ///     参数
        /// </summary>
        public TParameter Parameter
        {
            get => _parameter;
            set
            {
                if (Equals(_parameter, value))
                {
                    return;
                }
                _parameter = value;
                RaisePropertyChanged(() => Parameter);
            }
        }

        private TResult _result;

        /// <summary>
        ///     返回值
        /// </summary>
        public TResult Result
        {
            get => _result;
            set
            {
                if (Equals(_result, value))
                {
                    return;
                }
                _result = value;
                RaisePropertyChanged(() => Result);
            }
        }

        /// <summary>
        /// 准备执行
        /// </summary>
        /// <param name="parameter">执行参数</param>
        /// <param name="setParameter"></param>
        /// <returns></returns>
        public bool Perpare(TParameter parameter, Action<TParameter> setParameter)
        {
            Parameter = parameter;
            try
            {
                if (PerpareInner(setParameter))
                {
                    Status = CommandStatus.Executing; 
                    return true;
                }
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
            Exit();
            return false;
        }

        /// <summary>
        /// 准备执行
        /// </summary>
        /// <param name="setParameter"></param>
        /// <returns></returns>
        protected virtual bool PerpareInner(Action<TParameter> setParameter)
        {
            return true;
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        public TResult Execute(TParameter parameter)
        {
            return ExecuteInner(parameter);
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        public abstract TResult ExecuteInner(TParameter parameter);

        /// <summary>
        /// 结束操作
        /// </summary>
        /// <param name="status">执行状态</param>
        /// <param name="ex">异常</param>
        /// <param name="result">返回值</param>
        public virtual void End(CommandStatus status, Exception ex, TResult result)
        {
            Status = status;
            if (status != CommandStatus.Succeed)
            {
                Fail(ex);
            }
            else
            {
                Result = result;
                Succeed(result);
            }
            Exit();
        }

        /// <summary>
        /// 操作成功
        /// </summary>
        /// <param name="result">返回值</param>
        protected virtual void Succeed(TResult result)
        {
        }

        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="ex">异常</param>
        /// <remarks>
        /// 准备发生异常或执行执行发生异常时调用
        /// </remarks>
        protected virtual void Fail(Exception ex)
        {
            LogRecorder.Exception(ex, JobName);
        }

        /// <summary>
        /// 结束
        /// </summary>
        /// <remarks>
        /// 准备返回否或执行无论成功失败都调用
        /// </remarks>
        protected virtual void Exit()
        {
        }
    }
}