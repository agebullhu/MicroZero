// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-27
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.Windows;

#endregion

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     控件事件行为基类,请使用ElementEventBehavior或派生强类型的对象,以便在XAML中构造
    /// </summary>
    /// <typeparam name="TControl"></typeparam>
    public class EventBehavior<TControl> : DependencyBehavior<TControl>
            where TControl : UIElement
    {
    }

    /// <summary>
    ///     控件事件行为基类,请使用ElementEventBehavior或派生强类型的对象,以便在XAML中构造
    /// </summary>
    public class EventBehavior : EventBehavior<FrameworkElement>
    {
    }
}
