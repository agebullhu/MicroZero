using System;
using System.ComponentModel;
using System.Windows;
using Agebull.EntityModel;
using Agebull.Common.Logging;

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     命令基类
    /// </summary>
    public class CommandBase : INotifyPropertyChanged, IStatus
    {
        #region 能否执行处理

        private EventHandler _canExecuteChanged;

        private INotifyPropertyChanged _detect;

        /// <summary>
        ///     侦测可执行状态变化的对象
        /// </summary>
        public INotifyPropertyChanged Detect
        {
            get => _detect;
            set
            {
                if (_detect != null)
                {
                    _detect.PropertyChanged += OnDetectPropertyChanged;
                }
                if (Equals(_detect, value))
                {
                    return;
                }
                _detect = value;
                value.PropertyChanged += OnDetectPropertyChanged;
                OnCanExecuteChanged();
            }
        }

        private void OnDetectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
        
        /// <summary>
        ///     命令能否执行的事件
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => _canExecuteChanged += value;
            remove => _canExecuteChanged -= value;
        }

        /// <summary>
        ///     发出可执行状态变化的消息
        /// </summary>
        protected void OnCanExecuteChanged()
        {
            if (_canExecuteChanged == null)
            {
                return;
            }
            InvokeInUiThread<object>(OnCanExecuteChangedInner, null);
        }

        /// <summary>
        ///     发出可执行状态变化的消息
        /// </summary>
        protected void OnCanExecuteChangedInner(object par)
        {
            try
            {
                _canExecuteChanged(this, par as  EventArgs);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
        }

        #endregion


        #region 状态
        private Visibility _visibility;

        /// <inheritdoc />
        /// <summary>
        ///     图标
        /// </summary>
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility == value)
                {
                    return;
                }
                _visibility = value;
                RaisePropertyChanged(nameof(Visibility));
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
                RaisePropertyChanged(nameof(Status));
                RaisePropertyChanged(nameof(IsBusy));
            }
        }

        /// <summary>
        ///     是否正忙
        /// </summary>
        public bool IsBusy => Status == CommandStatus.Executing;

        #endregion

        #region PropertyChanged

        /// <summary>
        /// 同步上下文
        /// </summary>
        public ISynchronousContext Synchronous
        {
            get;
            set;
        }


        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args"></param>
        protected void InvokeInUiThread<T>(Action<T> action, T args)
        {
            if (Synchronous == null)
            {
                action(args);
            }
            else
            {
                Synchronous.BeginInvokeInUiThread(action, args);
            }
        }

        /// <summary>
        ///     属性修改事件(属性为空表示删除)
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                propertyChanged -= value;
                propertyChanged += value;
            }
            remove => propertyChanged -= value;
        }

        /// <summary>
        ///     属性修改事件
        /// </summary>
        private event PropertyChangedEventHandler propertyChanged;

        /// <summary>
        ///     发出属性修改事件(不受阻止模式影响)
        /// </summary>
        /// <param name="propertyName">属性</param>
        private void RaisePropertyChanged(string propertyName)
        {
            if (propertyChanged == null)
            {
                return;
            }
            InvokeInUiThread(RaisePropertyChangedInner, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="args">属性</param>
        private void RaisePropertyChangedInner(PropertyChangedEventArgs args)
        {
            try
            {
                propertyChanged?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
        }

        #endregion
    }
}