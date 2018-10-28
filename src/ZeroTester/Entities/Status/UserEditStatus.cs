#if CLIENT
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     用户编辑支持的状态对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class UserEditStatus : OriginalRecordStatus
    {
        /// <summary>
        /// 对应的对象
        /// </summary>
        [IgnoreDataMember]
        public UserEditEntityObject<UserEditStatus> UserEditObject
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
            UserEditObject = this.Object as UserEditEntityObject<UserEditStatus>;
        }

        /// <summary>
        ///     记录每一步修改
        /// </summary>
        [IgnoreDataMember]
        private List<EditRecordItem> _editIndexRecords;

        /// <summary>
        ///     记录每一步修改
        /// </summary>
        public List<EditRecordItem> EditIndexRecords
        {
            get
            {
                return this._editIndexRecords ?? (this._editIndexRecords = new List<EditRecordItem>());
            }
        }

        /// <summary>
        ///     当前编辑记录在第几号
        /// </summary>
        public int CurrentEditIndex
        {
            get;
            private set;
        }

        /// <summary>
        ///     能否重做
        /// </summary>
        public bool CanReDo
        {
            get
            {
                return this.EditIndexRecords.Count > 0 && this.CurrentEditIndex < (this.EditIndexRecords.Count - 1);
            }
        }

        /// <summary>
        ///     能否撤销
        /// </summary>
        public bool CanUnDo
        {
            get
            {
                return this.EditIndexRecords.Count > 0 && this.CurrentEditIndex > 0 && this.CurrentEditIndex <= (this.EditIndexRecords.Count - 1);
            }
        }

        /// <summary>
        /// 重置记录
        /// </summary>
        protected override void ResetRecords()
        {
            base.ResetRecords();
            this._editIndexRecords.Clear();
            this.CurrentEditIndex = -1;
            this.Object.OnStatusChanged(NotificationStatusType.CanReDo);
            this.Object.OnStatusChanged(NotificationStatusType.CanUnDo);
        }

        /// <summary>
        ///     属性修改之前的状态处理的实现
        /// </summary>
        /// <param name="args">属性</param>
        protected override void OnEndPropertyChangingInner(PropertyChangingEventArgsEx args)
        {
            base.OnEndPropertyChangingInner(args);
            this.AddEditIndexRecord(args.PropertyName, args.OldValue);
        }

        private void AddEditIndexRecord(string property, object value)
        {
            if (this.CanReDo)
            {
                this.EditIndexRecords.RemoveRange(this.CurrentEditIndex, this.EditIndexRecords.Count - this.CurrentEditIndex - 1);
            }
            this.EditIndexRecords.Add(new EditRecordItem
            {
                Property = property,
                Value = value
            });
            this.CurrentEditIndex = this.EditIndexRecords.Count - 1;

            this.Object.OnStatusChanged(NotificationStatusType.CanReDo);
            this.Object.OnStatusChanged(NotificationStatusType.CanUnDo);
        }

        /// <summary>
        ///     重做
        /// </summary>
        public void ReDo()
        {
            this.CurrentEditIndex++;
            if (!this.CanReDo)
            {
                return;
            }
            this.EditObject.SetValue(this.EditIndexRecords[this.CurrentEditIndex].Property, this.EditIndexRecords[this.CurrentEditIndex].Value);
        }

        /// <summary>
        ///     撤销
        /// </summary>
        public void UnDo()
        {
            if (!this.CanUnDo)
            {
                return;
            }
            this.SetEditIndexRecordValue();
            this.CurrentEditIndex--;
        }

        private void SetEditIndexRecordValue()
        {
            this.EditObject.SetValue(this.EditIndexRecords[this.CurrentEditIndex].Property, this.EditIndexRecords[this.CurrentEditIndex].Value);
            this.UserEditObject.RaiseFocusPropertyChanged(this.EditIndexRecords[this.CurrentEditIndex].Property);
        }
    }
}
#endif