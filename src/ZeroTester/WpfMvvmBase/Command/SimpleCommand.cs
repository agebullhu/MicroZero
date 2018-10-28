using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Agebull.Common.Logging;

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     无参数的标准委托命令
    /// </summary>
    public sealed class SimpleCommand<TArgument> : IStatusCommand
    {
        public TArgument Argument { get; set; }

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
            RaisePropertyChangedInner(new PropertyChangedEventArgs(propertyName));
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


        /// <summary>
        ///     可视化
        /// </summary>
        public Visibility Visibility => _canExecuteAction == null || _canExecuteAction() ? Visibility.Visible : Visibility.Collapsed;


        /// <summary>
        ///     检测executeAction能否执行状态的方法
        /// </summary>
        private readonly Func<bool> _canExecuteAction;

        /// <summary>
        ///     命令主体方法
        /// </summary>
        private readonly Action<TArgument> _executeAction;

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="argument">参数</param>
        /// <param name="executeAction">命令主体方法</param>
        public SimpleCommand(TArgument argument, Action<TArgument> executeAction)
            : this(argument, executeAction, () => true)
        {
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="argument">参数</param>
        /// <param name="executeAction">命令主体方法</param>
        /// <param name="canExecuteAction">检测executeAction能否执行状态的方法</param>
        public SimpleCommand(TArgument argument, Action<TArgument> executeAction, Func<bool> canExecuteAction)
        {
            Argument = argument;
            if (executeAction == null || canExecuteAction == null)
            {
                throw new ArgumentNullException(nameof(executeAction), @"命令方法不能为空");
            }
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="argument">参数</param>
        /// <param name="executeAction">命令主体方法</param>
        /// <param name="canExecuteAction">检测executeAction能否执行状态的方法</param>
        /// <param name="model">侦测变化事件的模型</param>
        public SimpleCommand(TArgument argument, Action<TArgument> executeAction, INotifyPropertyChanged model, Func<bool> canExecuteAction)
            : this(argument, executeAction, canExecuteAction)
        {
            model.PropertyChanged += OnModelPropertyChanged;
        }

        /// <summary>
        ///     是否正忙
        /// </summary>
        public bool IsBusy => false;

        /// <summary>
        ///     当前状态
        /// </summary>
        public CommandStatus Status => CommandStatus.None;

        /// <summary>
        ///     定义在调用此命令时调用的方法。
        /// </summary>
        /// <param name="parameter">此命令使用的数据。如果此命令不需要传递数据，则该对象可以设置为 null。</param>
        void ICommand.Execute(object parameter)
        {
            _executeAction(Argument);
        }

        /// <summary>
        ///     可执行状态变化的事件
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        ///     定义用于确定此命令是否可以在其当前状态下执行的方法。
        /// </summary>
        /// <returns>
        ///     如果可以执行此命令，则为 true；否则为 false。
        /// </returns>
        /// <param name="parameter">此命令使用的数据。如果此命令不需要传递数据，则该对象可以设置为 null。</param>
        bool ICommand.CanExecute(object parameter)
        {
            return _canExecuteAction == null || _canExecuteAction();
        }

        /// <summary>
        ///     根据绑定的模型更新可执行状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(Visibility));
        }

        /// <summary>
        ///     引发可执行状态变化的事件
        /// </summary>
        private void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     引发可执行状态变化的事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }
    }
}