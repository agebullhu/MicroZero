// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.Runtime;

#endregion

namespace System.ComponentModel
{
    /// <summary>
    ///     System.ComponentModel.INotifyPropertyChanging.PropertyChanged 事件提供数据。
    /// </summary>
    public sealed class PropertyChangingEventArgsEx : PropertyChangingEventArgs
    {
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertyChangingEventArgsEx(string propertyName, object oldValue, object newValue)
                : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        ///     原始的属性
        /// </summary>
        public object OldValue
        {
            get;
            private set;
        }

        /// <summary>
        ///     更改的属性
        /// </summary>
        public object NewValue
        {
            get;
            set;
        }

        /// <summary>
        ///     新值是否在处理过程中修改
        /// </summary>
        public bool ValueIsChaged
        {
            get;
            set;
        }

        /// <summary>
        ///     修改必须被阻止
        /// </summary>
        public bool Arrest
        {
            get;
            set;
        }
    }

}
