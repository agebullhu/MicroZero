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

#endregion

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     事件行为的附加方法
    /// </summary>
    public class BehaviorAction<TDependency> 
            where TDependency : DependencyObject
    {
        /// <summary>
        ///     将事件附加到模型的方法
        /// </summary>
        public Action<TDependency> AttachAction
        {
            get;
            set;
        }

        /// <summary>
        ///     将事件从模型分离的方法
        /// </summary>
        public Action<TDependency> DetachAction
        {
            get;
            set;
        }
    }
    /// <summary>
    ///     事件行为的附加方法
    /// </summary>
    public sealed class DependencyAction : BehaviorAction<DependencyObject> 
    {
    }
}
