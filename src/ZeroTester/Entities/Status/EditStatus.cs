using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.Common.DataModel
{
    /// <summary>
    /// 对象生态状态
    /// </summary>
    public enum EntitySubsist
    {
        /// <summary>
        /// 未知,只读对象被识别为存在
        /// </summary>
        None,
        /// <summary>
        /// 新增未保存
        /// </summary>
        Adding,
        /// <summary>
        /// 新增已保存,相当于Exist,但可用于处理新增保存的后期事件
        /// </summary>
        Added,
        /// <summary>
        /// 已存在
        /// </summary>
        Exist,
        /// <summary>
        /// 将要删除
        /// </summary>
        Deleting,
        /// <summary>
        /// 已经删除
        /// </summary>
        Deleted
    }
    /// <summary>
    /// 编辑状态
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class EditStatus : SerializableStatus
    {

        #region 高级对象

        /// <summary>
        /// 对应的对象
        /// </summary>
        [IgnoreDataMember]
        public EditEntityObject<EditStatus> EditObject
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化的实现
        /// </summary>
        protected override void InitializeInner()
        {
            base.InitializeInner();
            EditObject = this.Object as EditEntityObject<EditStatus>;
        }

        #endregion



        #region 还原与接受

        /// <summary>
        ///     接受修改
        /// </summary>
        internal void AcceptChanged()
        {
            this.AcceptChangedInner();
        }


        /// <summary>
        ///     回退修改
        /// </summary>
        internal void RejectChanged()
        {
            this.RejectChangedInner();
        }

        /// <summary>
        ///     接受修改(只可重写,不可调用)
        /// </summary>
        protected virtual void AcceptChangedInner()
        {
            ResetRecords();
        }

        /// <summary>
        ///     回退修改(只可重写,不可调用)
        /// </summary>
        protected virtual void RejectChangedInner()
        {
            ResetRecords();
        }
        /// <summary>
        /// 重置记录
        /// </summary>
        protected virtual void ResetRecords()
        {
            this._modifiedProperties = null;
        }
        #endregion



        #region 修改状态

        /// <summary>
        ///     阻止配置
        /// </summary>
        [IgnoreDataMember]
        private EditArrestMode _arrest;

        /// <summary>
        ///     阻止配置
        /// </summary>
        [ReadOnly(true), DisplayName("阻止配置"), Category("运行时")]
        public EditArrestMode Arrest
        {
            get
            {
                return this._arrest;
            }
            set
            {
                if (value == this._arrest)
                {
                    return;
                }
                this._arrest = value;
                Object.OnStatusChanged(() => this.Arrest);
            }
        }


        /// <summary>
        ///     对象生态状态
        /// </summary>
        [DataMember]
        private EntitySubsist _subsist;

        /// <summary>
        ///     对象生态状态
        /// </summary>
        [ReadOnly(true), DisplayName("对象生态状态"), Category("运行时")]
        public EntitySubsist Subsist
        {
            get
            {
                return this._subsist;
            }
            set
            {
                if (value == this._subsist)
                {
                    return;
                }
                this._subsist = value;
                Object.OnStatusChanged(() => this.Subsist);
            }
        }

        /// <summary>
        ///     是否已修改
        /// </summary>
        [ReadOnly(true), DisplayName("是否修改"), Category("运行时")]
        public bool IsModified
        {
            get
            {
                return this._modifiedProperties != null && this._modifiedProperties.Count > 0;
            }
        }

        /// <summary>
        ///     修改的属性列表
        /// </summary>
        [DataMember]
        private List<string> _modifiedProperties;

        /// <summary>
        ///     修改的属性列表
        /// </summary>
        [ReadOnly(true), DisplayName("修改的属性列表"), Category("运行时")]
        public List<string> ModifiedProperties
        {
            get
            {
                return this._modifiedProperties ?? (this._modifiedProperties = new List<string>());
            }
        }

        /// <summary>
        ///     是否修改
        /// </summary>
        public bool FieldIsModified(string propertyName)
        {
            return this._modifiedProperties != null && this._modifiedProperties.Contains(propertyName);
        }

        /// <summary>
        ///     设置为非改变
        /// </summary>
        /// <param name="propertyName"> 字段的名字 </param>
        public void SetUnModify(string propertyName)
        {
            if (this._modifiedProperties == null || this._modifiedProperties.Count == 0)
                return;
            if (this._modifiedProperties.Contains(propertyName))
            {
                this._modifiedProperties.Remove(propertyName);
            }
        }

        /// <summary>
        ///     设置为改变
        /// </summary>
        /// <param name="propertyName"> 字段的名字 </param>
        public void SetModify(string propertyName)
        {
            RecordModified(propertyName);
        }

#if CLIENT       
        /// <summary>
        ///     发出所有已修改属性的PropertyChanged事件
        /// </summary>
        public void RaiseModifiedPropertiesChanged()
        {
            if (this._modifiedProperties == null || this._modifiedProperties.Count == 0)
            {
                return;
            }
            foreach (string property in this._modifiedProperties)
            {
                Object.RaisePropertyChangedEvent(property);
            }
        }
#endif

        /// <summary>
        ///     记录属性修改
        /// </summary>
        /// <param name="propertyName">属性</param>
        internal protected void RecordModified(string propertyName)
        {
            if (!Arrest.HasFlag(EditArrestMode.RecordChanged))
            {
                RecordModifiedInner(propertyName);
            }
            if (!Arrest.HasFlag(EditArrestMode.InnerLogical))
            {
                this.EditObject.OnModified(propertyName);
            }
        }


        /// <summary>
        ///     记录属性修改
        /// </summary>
        /// <param name="propertyName">属性</param>
        protected virtual void RecordModifiedInner(string propertyName)
        {
            if (!this.ModifiedProperties.Contains(propertyName))
            {
                this.ModifiedProperties.Add(propertyName);
            }
            if (!Arrest.HasFlag(EditArrestMode.InnerLogical))
            {
                this.EditObject.OnModified(propertyName);
            }
        }
        #endregion

        #region 复制支持

        /// <summary>
        ///     复制修改状态
        /// </summary>
        /// <param name="target">要复制的源</param>
        internal protected void CopyState(EditStatus target)
        {
            CopyStateInner(target);
        }

        /// <summary>
        ///     复制修改状态
        /// </summary>
        /// <param name="target">要复制的源</param>
        protected virtual void CopyStateInner(EditStatus target)
        {
            if (target._modifiedProperties == null || target._modifiedProperties.Count == 0)
            {
                this._modifiedProperties = null;
            }
            else
            {
                this._modifiedProperties = new List<string>(target._modifiedProperties);
            }
        }
        #endregion

        #region 编辑方法

        private bool _isDelete;

        /// <summary>
        ///     是否已删除
        /// </summary>
        /// <returns></returns>
        [ReadOnly(true), DisplayName("是否已删除"), Category("运行时")]
        public bool IsDelete
        {
            get
            {
                return this._isDelete;
            }
            set
            {
                if (_isDelete == value)
                    return;
                _isDelete = value;
                EditObject.OnDelete(value);
                Object.OnStatusChanged(() => this.IsDelete);
            }
        }

        #endregion

        #region 属性修改时

        /// <summary>
        ///     属性修改之前的状态处理
        /// </summary>
        /// <param name="args">属性</param>
        internal void OnBeginPropertyChanging(PropertyChangingEventArgsEx args)
        {
            if (!this.Arrest.HasFlag(EditArrestMode.InnerCheck))
            {
                this.OnBeginPropertyChangingInner(args);
            }
        }

        /// <summary>
        ///     属性修改之前的状态处理
        /// </summary>
        /// <param name="args">属性</param>
        internal void OnEndPropertyChanging(PropertyChangingEventArgsEx args)
        {
            if (!this.Arrest.HasFlag(EditArrestMode.InnerCheck))
            {
                this.OnEndPropertyChangingInner(args);
            }
        }

        /// <summary>
        ///     属性修改之前的状态处理的实现
        /// </summary>
        /// <param name="args">属性</param>
        protected virtual void OnBeginPropertyChangingInner(PropertyChangingEventArgsEx args)
        {

        }

        /// <summary>
        ///     属性修改之前的状态处理的实现
        /// </summary>
        /// <param name="args">属性</param>
        protected virtual void OnEndPropertyChangingInner(PropertyChangingEventArgsEx args)
        {

        }
        #endregion
    }
}