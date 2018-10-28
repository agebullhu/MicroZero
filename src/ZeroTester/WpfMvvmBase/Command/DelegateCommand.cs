// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-24
// 修改:2014-12-07
// *****************************************************/

#region 引用

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Agebull.Common.Logging;

#endregion

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     无参数的标准委托命令
    /// </summary>
    public sealed class DelegateCommand : IStatusCommand
    {
        /// <summary>
        /// 空方法,以便于生成一个什么也不做的命令
        /// </summary>
        private static void EmptyAction()
        {

        }

        public object Tag { get; set; }

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

        private static DelegateCommand _emptyCommand;
        /// <summary>
        /// 空命令
        /// </summary>
        public static ICommand EmptyCommand => _emptyCommand ?? (_emptyCommand = new DelegateCommand(EmptyAction));

        /// <summary>
        ///     默认总是为真的检测executeAction能否执行状态的方法,目的是防止构建无数的相同无用的Action浪费内存
        /// </summary>
        /// <returns></returns>
        public static readonly Func<bool> DefaultCanExecuteAction = () => true;
        /// <summary>
        ///     检测executeAction能否执行状态的方法
        /// </summary>
        private readonly Func<bool> _canExecuteAction;

        /// <summary>
        ///     命令主体方法
        /// </summary>
        private readonly Action _executeAction;

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令主体方法</param>
        public DelegateCommand(Action executeAction)
            : this(executeAction, DefaultCanExecuteAction)
        {
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令主体方法</param>
        /// <param name="canExecuteAction">检测executeAction能否执行状态的方法</param>
        public DelegateCommand(Action executeAction, Func<bool> canExecuteAction)
        {
            if (executeAction == null || canExecuteAction == null)
            {
                throw new ArgumentNullException("executeAction", @"命令方法不能为空");
            }
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令主体方法</param>
        /// <param name="canExecuteAction">检测executeAction能否执行状态的方法</param>
        /// <param name="model">侦测变化事件的模型</param>
        public DelegateCommand(Action executeAction, INotifyPropertyChanged model, Func<bool> canExecuteAction)
            : this(executeAction, canExecuteAction)
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
            Execute();
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
        ///     执行命令
        /// </summary>
        public void Execute()
        {
            _executeAction();
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
        #region 能否执行处理

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

        #endregion
    }

    /// <summary>
    ///     带参数的标准委托命令
    /// </summary>
    /// <typeparam name="TParameter">参数类型</typeparam>
    public sealed class DelegateCommand<TParameter> : IStatusCommand
            where TParameter : class
    {
        #region 构造

        /// <summary>
        ///     默认总是为真的检测executeAction能否执行状态的方法,目的是防止构建无数的相同无用的Action浪费内存
        /// </summary>
        /// <returns></returns>
        public static readonly Func<TParameter, bool> DefaultCanExecuteAction = p => true;

        /// <summary>
        ///     检测executeAction能否执行状态的方法
        /// </summary>
        private readonly Func<TParameter, bool> _canExecuteAction;

        /// <summary>
        ///     命令主体方法
        /// </summary>
        private readonly Action<TParameter> _executeAction;

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令主体方法</param>
        public DelegateCommand(Action<TParameter> executeAction)
            : this(executeAction, null, null)
        {
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令主体方法</param>
        /// <param name="model">侦测变化事件的模型</param>
        public DelegateCommand(Action<TParameter> executeAction, TParameter model)
            : this(executeAction, model, null)
        {
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令主体方法</param>
        /// <param name="canExecuteAction">检测executeAction能否执行状态的方法</param>
        public DelegateCommand(Action<TParameter> executeAction, Func<TParameter, bool> canExecuteAction)
            : this(executeAction, null, canExecuteAction)
        {
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="executeAction">命令主体方法</param>
        /// <param name="canExecuteAction">检测executeAction能否执行状态的方法</param>
        /// <param name="model">侦测变化事件的模型</param>
        public DelegateCommand(Action<TParameter> executeAction, TParameter model, Func<TParameter, bool> canExecuteAction)
        {
            if (executeAction == null)
            {
                throw new ArgumentNullException("executeAction", @"命令方法不能为空");
            }
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
            Parameter = model;
            if (model is INotifyPropertyChanged changed)
            {
                changed.PropertyChanged += OnModelPropertyChanged;
            }
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

        #endregion

        #region 能否执行处理

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

        #endregion
        #region 定义

        private TParameter _parameter;
        /// <summary>
        ///     命令参数)
        /// </summary>
        /// <remarks>
        ///     如果Parameter不为空,执行命令时忽略从绑定传递的参数,使用Parameter作为参数调用executeAction
        ///     如果Parameter为空,则和之前一致,将从绑定传递的参数转为TParameter类型(使用as)调用executeAction
        /// </remarks>
        public TParameter Parameter
        {
            get => _parameter;
            set
            {
                if (_parameter == value)
                    return;
                
                _parameter = value;
                Detect= value as INotifyPropertyChanged;
                RaisePropertyChanged(nameof(Parameter));
            }
        }

        /// <summary>
        ///     是否正忙
        /// </summary>
        public bool IsBusy => false;

        /// <summary>
        ///     当前状态
        /// </summary>
        public CommandStatus Status => CommandStatus.None;

        #endregion

        #region 基础实现

        /// <summary>
        ///     定义在调用此命令时调用的方法。
        /// </summary>
        /// <param name="parameter">绑定传递的参数(如果已设置Parameter属性,参数无效)</param>
        /// <remarks>
        ///     如果Parameter不为空,执行命令时忽略从绑定传递的参数,使用Parameter作为参数调用executeAction
        ///     如果Parameter为空,则和之前一致,将从绑定传递的参数转为TParameter类型(使用as)调用executeAction
        /// </remarks>
        void ICommand.Execute(object parameter)
        {
            _executeAction(Parameter ?? parameter as TParameter);
        }

        /// <summary>
        ///     定义用于确定此命令是否可以在其当前状态下执行的方法。
        /// </summary>
        /// <returns>
        ///     如果可以执行此命令，则为 true；否则为 false。
        /// </returns>
        /// <param name="parameter">此命令使用的数据。如果此命令不需要传递数据，则该对象可以设置为 null。</param>
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter as TParameter);
        }

        /// <summary>
        ///     可执行状态变化的事件
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        ///     定义在调用此命令时调用的方法。
        /// </summary>
        /// <param name="parameter">传递的参数(即使已设置Parameter属性,也使用这个参数)</param>
        public void Execute(TParameter parameter)
        {
            _executeAction(parameter);
        }

        /// <summary>
        ///     执行方法
        /// </summary>
        /// <remarks>
        ///     如果Parameter不为空,执行命令时忽略从绑定传递的参数,使用Parameter作为参数调用executeAction
        ///     如果Parameter为空,使用null作为参数调用executeAction
        /// </remarks>
        public void Execute()
        {
            _executeAction(Parameter);
        }

        /// <summary>
        ///     检测是否可以执行
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(TParameter parameter)
        {
            return _canExecuteAction == null || _canExecuteAction(Parameter ?? parameter);
        }

        /// <summary>
        ///     引发可执行状态变化的事件
        /// </summary>
        private void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     引发可执行状态变化的事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        #endregion


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
        private Visibility _visibility;

        /// <summary>
        ///     可视
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
    }
}
