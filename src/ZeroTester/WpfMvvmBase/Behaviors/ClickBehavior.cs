// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-27
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

#endregion

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     单击事件行为类
    /// </summary>
    public sealed class ClickBehavior : Behavior<UIElement>
    {
        /// <summary>
        ///     绑定的命令
        /// </summary>
        public static readonly DependencyProperty CommandProperty;

        /// <summary>
        ///     绑定的命令
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty;

        /// <summary>
        ///     绑定的命令
        /// </summary>
        public static readonly DependencyProperty IsDoubleClickProperty;

        static ClickBehavior()
        {
            CommandProperty = DependencyProperty.Register("Command",
                    typeof (ICommand),
                    typeof (ClickBehavior),
                    new UIPropertyMetadata(null, OnCommandPropertyChanged));

            CommandParameterProperty = DependencyProperty.Register("CommandProperty",
                    typeof (object),
                    typeof (ClickBehavior));

            IsDoubleClickProperty = DependencyProperty.Register("IsDoubleClick",
                    typeof (bool),
                    typeof (ClickBehavior));
        }

        /// <summary>
        ///     命令
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        ///     是否双击
        /// </summary>
        public bool IsDoubleClick
        {
            get => (bool)GetValue(IsDoubleClickProperty);
            set => SetValue(IsDoubleClickProperty, value);
        }

        /// <summary>
        ///     命令参数
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ClickBehavior eb) || Equals(e.OldValue, e.NewValue))
            {
                return;
            }
            if (e.OldValue is ICommand cmd)
            {
                cmd.CanExecuteChanged -= eb.OnCanExecuteChanged;
            }
            cmd = e.NewValue as ICommand;
            if (cmd != null)
            {
                cmd.CanExecuteChanged += eb.OnCanExecuteChanged;
            }
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            AssociatedObject.IsEnabled = Command.CanExecute(CommandProperty);
        }

        /// <summary>
        ///     在行为附加到 AssociatedObject 后调用。
        /// </summary>
        /// <remarks>
        ///     替代它以便将功能挂钩到 AssociatedObject。
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        /// <summary>
        ///     在行为与其 AssociatedObject 分离时（但在它实际发生之前）调用。
        /// </summary>
        /// <remarks>
        ///     替代它以便将功能从 AssociatedObject 中解除挂钩。
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        /// <summary>
        ///     单击时执行命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 && !IsDoubleClick)
            {
                return;
            }
            e.Handled = true;
            ICommand cmd = Command;
            object par = CommandParameter;
            if (cmd.CanExecute(par))
            {
                cmd.CanExecute(par);
            }
        }
    }
}
