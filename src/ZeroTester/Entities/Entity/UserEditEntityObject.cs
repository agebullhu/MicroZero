
#if CLIENT
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     用户编辑支持的实体对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class UserEditEntityObject<TStatus> : EditEntityObject<TStatus>
            where TStatus : OriginalRecordStatus, new()
    {

        #region 当前焦点属性变化事件

        /// <summary>
        /// 发出当前焦点属性变化的通知
        /// </summary>
        internal void RaiseFocusPropertyChanged(string propertyName)
        {
            this.InvokeInUiThread(this.RaiseFocusPropertyChangedInner, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     当前焦点属性变化
        /// </summary>
        private event PropertyChangedEventHandler focusPropertyChanged;

        /// <summary>
        ///     发出属性修改事件(不受阻止模式影响)
        /// </summary>
        /// <param name="args">属性</param>
        private void RaiseFocusPropertyChangedInner(PropertyChangedEventArgs args)
        {
            if (this.focusPropertyChanged == null)
            {
                return;
            }
            try
            {
                this.focusPropertyChanged(this, args);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, "UserEditEntityObject.RaiseFocusPropertyChangedInner");
                throw;
            }
        }

        /// <summary>
        ///     当前焦点属性变化
        /// </summary>
        public event PropertyChangedEventHandler FocusPropertyChanged
        {
            add
            {
                this.focusPropertyChanged -= value;
                this.focusPropertyChanged += value;
            }
            remove
            {
                this.focusPropertyChanged -= value;
            }
        }

        #endregion
    }
    /// <summary>
    ///     用户编辑支持的实体对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class UserEditEntityObject : UserEditEntityObject<UserEditStatus>
    {

    }
}
#endif