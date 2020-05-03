using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Agebull.Common.Mvvm
{
    public interface IGridSelectionBinding
    {
        IList SelectColumns { set; }
    }
    /// <summary>
    ///     表格自动生成字段的委托,用DisplayNameAttribute作为列头,用BotwerableAttribute控制是否显示
    /// </summary>
    public sealed class DataGridGenertColumnsBehavior : Behavior<DataGrid>
    {
        /// <summary>
        ///     在行为附加到 AssociatedObject 后调用。
        /// </summary>
        /// <remarks>
        ///     替代它以便将功能挂钩到 AssociatedObject。
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            //this.AssociatedObject.AutoGenerateColumns = true;
            AssociatedObject.AutoGeneratingColumn += AssociatedObject_AutoGeneratingColumn;
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (AssociatedObject.DataContext is IGridSelectionBinding binding)
                binding.SelectColumns = AssociatedObject.SelectedItems;
        }

        private void AssociatedObject_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (!(e.PropertyDescriptor is MemberDescriptor descriptor))
                return;
            if (!descriptor.IsBrowsable)
            {
                e.Cancel = true;
            }
            else if (!string.IsNullOrEmpty(descriptor.DisplayName))
            {
                e.Column.Header = descriptor.DisplayName;
            }
        }

        /// <summary>
        ///     在行为与其 AssociatedObject 分离时（但在它实际发生之前）调用。
        /// </summary>
        /// <remarks>
        ///     替代它以便将功能从 AssociatedObject 中解除挂钩。
        /// </remarks>
        protected override void OnDetaching()
        {
            AssociatedObject.AutoGeneratingColumn -= AssociatedObject_AutoGeneratingColumn;
            if (AssociatedObject.DataContext is IGridSelectionBinding)
                AssociatedObject.SelectionChanged -= OnSelectionChanged;
            base.OnDetaching();
        }
    }
}