// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-27
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Windows;
using System.Windows.Interactivity;
using Agebull.Common.Logging;

#endregion

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     普通控件的事件行为类
    /// </summary>
    public class DependencyBehavior<TDependency> : Behavior<TDependency>
            where TDependency : DependencyObject
    {
        /// <summary>
        ///     绑定对象的方法名称
        /// </summary>
        public static readonly DependencyProperty BehaviorActionProperty;

        private bool _isAttached;

        static DependencyBehavior()
        {
            BehaviorActionProperty = DependencyProperty.Register("BehaviorAction",
                    typeof (BehaviorAction<TDependency>),
                    typeof (DependencyBehavior<TDependency>),
                    new UIPropertyMetadata(null, OnBehaviorActionPropertyChanged));
        }

        /// <summary>
        ///     绑定对象的方法名称
        /// </summary>
        public BehaviorAction<TDependency> BehaviorAction
        {
            get => (BehaviorAction<TDependency>)GetValue(BehaviorActionProperty);
            set => SetValue(BehaviorActionProperty, value);
        }

        private static void OnBehaviorActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DependencyBehavior<TDependency> eb) || !eb._isAttached || Equals(e.OldValue, e.NewValue))
            {
                return;
            }
            try
            {
                if (e.OldValue is BehaviorAction<TDependency> action && action.DetachAction != null)
                {
                    action.DetachAction(eb.AssociatedObject);
                }
                action = e.NewValue as BehaviorAction<TDependency>;
                if (action != null && action.AttachAction != null)
                {
                    action.AttachAction(eb.AssociatedObject);
                }
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
            }
        }

        /// <summary>
        ///     在行为附加到 AssociatedObject 后调用。
        /// </summary>
        /// <remarks>
        ///     替代它以便将功能挂钩到 AssociatedObject。
        /// </remarks>
        protected override sealed void OnAttached()
        {
            base.OnAttached();
            _isAttached = true;
            if (BehaviorAction == null || BehaviorAction.AttachAction == null)
            {
                return;
            }
            try
            {
                BehaviorAction.AttachAction(AssociatedObject);
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
            }
        }

        /// <summary>
        ///     在行为与其 AssociatedObject 分离时（但在它实际发生之前）调用。
        /// </summary>
        /// <remarks>
        ///     替代它以便将功能从 AssociatedObject 中解除挂钩。
        /// </remarks>
        protected override sealed void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null && BehaviorAction.DetachAction != null)
            {
                try
                {
                    BehaviorAction.DetachAction(AssociatedObject);
                }
                catch (Exception exception)
                {
                    LogRecorder.Exception(exception);
                }
            }
        }
    }

    /// <summary>
    ///     普通依赖对象行为附加
    /// </summary>
    public sealed class DependencyBehavior : DependencyBehavior<DependencyObject>
    {
    }
    /// <summary>
    ///     普通依赖对象行为附加
    /// </summary>
    public sealed class ElementBehavior : DependencyBehavior<FrameworkElement>
    {
    }
}
