// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///  编辑支持的实体对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class EditEntityObject<TStatus> : StatusEntityObject<TStatus>, IEditObject
            where TStatus : EditStatus, new()
    {
        #region 仅可重载方法
        
        /// <summary>
        ///     构建状态对象
        /// </summary>
        protected override TStatus CreateStatus()
        {
            var status = new TStatus();
            status.Initialize(this);
            status.Subsist = EntitySubsist.Exist;
            return status;
        }
            
        /// <summary>
        ///     接受修改
        /// </summary>
        public void AcceptChanged()
        {
            this.AcceptChangedInner();
            this.OnStatusChanged(NotificationStatusType.Modified);
        }


        /// <summary>
        ///     回退修改
        /// </summary>
        public void RejectChanged()
        {
            this.RejectChangedInner();
            this.OnStatusChanged(NotificationStatusType.Modified);
        }

        /// <summary>
        ///     接受修改(只可重写,不可调用)
        /// </summary>
        protected virtual void AcceptChangedInner()
        {
            __EntityStatus.AcceptChanged();
        }

        /// <summary>
        ///     回退修改(只可重写,不可调用)
        /// </summary>
        protected virtual void RejectChangedInner()
        {
            __EntityStatus.RejectChanged();
        }

        /// <summary>
        ///     已被修改(不可调用,只可重写)
        /// </summary>
        internal protected virtual void OnModified(string field)
        {

        }

        #endregion

        #region 修改状态

        /// <summary>
        ///     是否修改
        /// </summary>
        bool IEditObject.IsModified
        {
            get
            {
                return __EntityStatus.IsModified;
            }
        }

        /// <summary>
        ///     是否新增
        /// </summary>
        bool IEditObject.IsAdd
        {
            get
            {
                return __EntityStatus.Subsist == EntitySubsist.Adding;
            }
        }
        /// <summary>
        ///     是否修改
        /// </summary>
        public bool FieldIsModified(string propertyName)
        {
            return this.__EntityStatus.FieldIsModified(propertyName);
        }

        /// <summary>
        ///     设置为非改变
        /// </summary>
        /// <param name="propertyName"> 字段的名字 </param>
        public virtual void SetUnModify(string propertyName)
        {
            this.__EntityStatus.SetUnModify(propertyName);
        }

        /// <summary>
        ///     设置为改变
        /// </summary>
        /// <param name="propertyName"> 字段的名字 </param>
        public void SetModify(string propertyName)
        {
            this.RecordModifiedInner(propertyName);
        }


        /// <summary>
        ///     记录属性修改
        /// </summary>
        /// <param name="propertyName">属性</param>
        protected sealed override void RecordModifiedInner(string propertyName)
        {
            if (!this.__EntityStatus.Arrest.HasFlag(EditArrestMode.RecordChanged))
            {
                this.__EntityStatus.RecordModified(propertyName);
            }
        }

        #endregion

        #region 复制支持

        /// <summary>
        ///     复制修改状态
        /// </summary>
        /// <param name="target">要复制的源</param>
        protected void CopyState(EditEntityObject target)
        {
            this.__EntityStatus.CopyState(target.__EntityStatus);
        }

        #endregion

        #region 编辑方法

        /// <summary>
        ///     是否已删除
        /// </summary>
        /// <returns></returns>
        bool IEditObject.IsDelete
        {
            get
            {
                return this.__EntityStatus.IsDelete;
            }
        }

        /// <summary>
        ///     对象已删除的同步处理(只可重写,不可调用)
        /// </summary>
        internal protected virtual void OnDelete(bool isDelete)
        {

        }

        #endregion

        #region 属性修改中事件


#if CLIENT
        /// <summary>
        ///     发出所有已修改属性的PropertyChanged事件
        /// </summary>
        public void RaiseModifiesPropertiesChanged()
        {
            this.__EntityStatus.RaiseModifiedPropertiesChanged();
        }
        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="propertyName">属性</param>
        protected sealed override void RaisePropertyChangedEventInner(string propertyName)
        {
            if (!this.__EntityStatus.Arrest.HasFlag(EditArrestMode.PropertyChangedEvent))
            {
                base.RaisePropertyChangedEventInner(propertyName);
            }
        }

        /// <summary>
        ///     属性修改事件(属性为空表示删除)
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging
        {
            add
            {
                this.propertyChanging -= value;
                this.propertyChanging += value;
            }
            remove
            {
                this.propertyChanging -= value;
            }
        }

        /// <summary>
        ///     属性修改事件前事件(属性为空表示删除)
        /// </summary>
        private event PropertyChangingEventHandler propertyChanging;
        /// <summary>
        ///     发出属性修改中事件
        /// </summary>
        /// <param name="args">属性</param>
        private void RaisePropertyChangedInner(PropertyChangingEventArgsEx args)
        {
            if (this.propertyChanging == null)
            {
                return;
            }
            try
            {
                this.propertyChanging(this, args);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, "EditEntityObject.RaisePropertyChangedInner");
                throw;
            }
        }
#endif

        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="propertyName">属性</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        /// <returns>返回否表示不可修改</returns>
        public bool OnPropertyChanging(string propertyName, object oldValue, object newValue)
        {
            return OnPropertyChanging(new PropertyChangingEventArgsEx(propertyName, oldValue, newValue));
        }

        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="action">属性</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        /// <returns>返回否表示不可修改</returns>
        protected bool OnPropertyChanging<T>(Expression<Func<T>> action, T oldValue, T newValue)
        {
            return OnPropertyChanging(new PropertyChangingEventArgsEx(GetPropertyName(action), oldValue, newValue));
        }

        /// <summary>
        ///     处理属性修改中事件
        /// </summary>
        /// <param name="args">属性</param>
        /// <returns>返回否表示不可修改</returns>
        private bool OnPropertyChanging(PropertyChangingEventArgsEx args)
        {
            if (!this.__EntityStatus.Arrest.HasFlag(EditArrestMode.InnerCheck))
            {
                if (!this.BeginPropertyChanging(args))
                    return false;
                this.__EntityStatus.OnBeginPropertyChanging(args);
            }
#if CLIENT
            if (!this.__EntityStatus.Arrest.HasFlag(EditArrestMode.PropertyChangingEvent))
            {
                this.InvokeInUiThread(this.RaisePropertyChangedInner, args);
            }
#endif
            if (!this.__EntityStatus.Arrest.HasFlag(EditArrestMode.InnerCheck))
            {
                this.EndPropertyChanging(args);
                this.__EntityStatus.OnEndPropertyChanging(args);
            }
            return true;
        }

        /// <summary>
        ///     在属性将被修改前的检查是否可以修改
        /// </summary>
        /// <param name="args">属性</param>
        /// <returns>返回否表示拒绝修改</returns>
        protected virtual bool BeginPropertyChanging(PropertyChangingEventArgsEx args)
        {
            return true;
        }

        /// <summary>
        ///     属性修改准备完成时调用
        /// </summary>
        /// <param name="args">属性</param>
        protected virtual void EndPropertyChanging(PropertyChangingEventArgsEx args)
        {
        }

        #endregion

        #region 后期修改事件

        /// <summary>
        /// 属性修改的后期处理(保存后)
        /// </summary>
        /// <remarks>
        /// 对当前对象的属性的更改,请自行保存,否则将丢失
        /// </remarks>
        public void LaterPeriodByModify()
        {
            OnLaterPeriodBySignleModified(this.__EntityStatus.Subsist , this.__EntityStatus.ModifiedProperties);
            OnLaterPeriodByModified(this.__EntityStatus.Subsist, this.__EntityStatus.ModifiedProperties);
        }

        /// <summary>
        /// 单个属性修改的后期处理(保存后)
        /// </summary>
        /// <param name="subsist">当前实体生存状态</param>
        /// <param name="modifieds">修改列表</param>
        /// <remarks>
        /// 对当前对象的属性的更改,请自行保存,否则将丢失
        /// </remarks>
        protected virtual void OnLaterPeriodBySignleModified(EntitySubsist subsist, List<string> modifieds)
        {

        }

        /// <summary>
        /// 组合属性修改的后期处理(保存后)
        /// </summary>
        /// <param name="subsist">当前实体生存状态</param>
        /// <param name="modifieds">修改列表</param>
        /// <remarks>
        /// 对当前对象的属性的更改,请自行保存,否则将丢失
        /// </remarks>
        protected virtual void OnLaterPeriodByModified(EntitySubsist subsist, List<string> modifieds)
        {
            
        }
        #endregion
    }

    /// <summary>
    ///     编辑支持的实体对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class EditEntityObject : EditEntityObject<EditStatus>
    {

    }
}
