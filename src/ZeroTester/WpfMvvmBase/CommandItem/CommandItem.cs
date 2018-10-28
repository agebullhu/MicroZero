// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-26
// 修改:2014-12-07
// *****************************************************/

#region 引用

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Agebull.EntityModel;

#endregion

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     表示一个命令节点
    /// </summary>
    public abstract class CommandItemBase : CommandConfig
    {

        #region 状态

        /// <summary>
        ///     图标
        /// </summary>
        ImageSource _image;

        /// <summary>
        ///     图标
        /// </summary>
        public ImageSource Image
        {
            get => IsRoot
                ? null
                : _image ?? (_image = Application.Current.Resources[IconName ?? "imgDefault"] as ImageSource);
            set => _image = value;
        }

        /// <summary>
        ///     是否根
        /// </summary>
        public bool IsRoot { get; set; }

        /// <summary>
        ///     是否线
        /// </summary>
        public bool IsLine { get; set; }

        /// <summary>
        /// 表示分隔线
        /// </summary>
        public static CommandItem Line { get; } = new CommandItem
        {
            IsLine = true
        };


        private bool _isChecked;

        /// <summary>
        ///     是否选中
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }
        private bool _isBusy;

        /// <summary>
        ///     图标
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                    return;
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }

        private Visibility _visibility;

        /// <summary>
        ///     可见
        /// </summary>
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility == value)
                    return;
                _visibility = value;
                RaisePropertyChanged(() => Visibility);
            }
        }
        #endregion

        #region 参数
        /// <summary>
        /// 参数
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public object Parameter => Source;
        #endregion

        #region 命令

        private ICommand _command;


        /// <summary>
        ///     对应的命令
        /// </summary>
        public ICommand Command
        {
            get => _command;
            protected set
            {
                _command = value;
                if (value is INotifyPropertyChanged pp)
                    pp.PropertyChanged += OnCommandPropertyChanged;
            }
        }

        protected void OnCommandPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is IStatusCommand cmd))
                return;
            switch (e.PropertyName)
            {
                case "IsBusy":
                    IsBusy = cmd.IsBusy;
                    break;
                case "Visibility":
                    Visibility = cmd.Visibility;
                    break;
            }
        }
        /// <summary>
        /// 执行
        /// </summary>
        public abstract void Execute(object arg);
        /// <summary>
        /// 准备动作
        /// </summary>
        public Func<CommandItemBase, bool> OnPrepare { get; set; }

        #endregion

        #region 子级

        /// <summary>
        /// 所有按钮
        /// </summary>
        public NotificationList<CommandItemBase> Items { get; set; } = new NotificationList<CommandItemBase>();

        #endregion

    }


    /// <summary>
    ///     表示一个命令节点
    /// </summary>
    public class CommandItem : CommandItemBase
    {
        #region 命令

        public CommandItem()
        {
            SignleSoruce = true;
            Command = new DelegateCommand<object>(DoAction);
        }

        /// <summary>
        ///     对应的命令
        /// </summary>
        public Action<object> Action
        {
            get;
            set;
        }


        void DoAction(object arg)
        {
            if (DoConfirm && MessageBox.Show(ConfirmMessage ?? $"确认执行【{Caption ?? Name}】操作吗?", "对象编辑", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            if (OnPrepare == null || OnPrepare(this))
                Action?.Invoke(arg);
        }
        /// <summary>
        /// 执行
        /// </summary>
        public override void Execute(object arg)
        {
            Action?.Invoke(arg);
        }
        #endregion
    }

    /// <summary>
    ///     表示一个命令节点
    /// </summary>
    public class CommandItem<TArgument> : CommandItemBase
        where TArgument : class
    {
        #region 命令

        public CommandItem()
        {
            SignleSoruce = true;
            Command = new DelegateCommand(DoAction);
        }

        /// <summary>
        ///     对应的命令
        /// </summary>
        public Action<TArgument> Action
        {
            get;
            set;
        }


        /// <summary>
        ///     对应的命令
        /// </summary>
        public TArgument Argument => Source as TArgument;

        void DoAction()
        {
            if (!string.IsNullOrWhiteSpace(ConfirmMessage) && MessageBox.Show(ConfirmMessage, "对象编辑", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            if (OnPrepare == null || OnPrepare(this))
                Action?.Invoke(Argument);
        }
        /// <summary>
        /// 执行
        /// </summary>
        public override void Execute(object arg)
        {
            Action?.Invoke(arg as TArgument);
        }
        #endregion
    }
}