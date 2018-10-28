// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     保存原始值的编辑对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class OriginalRecordStatus : EditStatus
    {
        #region 原始值

        /// <summary>
        ///     被修改属性的原始值
        /// </summary>
        [IgnoreDataMember]
        private Dictionary<string, object> _originalValues;

        /// <summary>
        ///     被修改属性的原始值
        /// </summary>
        public Dictionary<string, object> OriginalValues
        {
            get
            {
                return this._originalValues ?? (this._originalValues = new Dictionary<string, object>());
            }
        }

        /// <summary>
        ///     属性修改之前的状态处理的实现
        /// </summary>
        /// <param name="args">属性</param>
        protected override void OnBeginPropertyChangingInner(PropertyChangingEventArgsEx args)
        {
            if (!this.OriginalValues.ContainsKey(args.PropertyName))
            {
                this.OriginalValues.Add(args.PropertyName, args.OldValue);
            }
        }

        /// <summary>
        ///     属性修改之前的状态处理的实现
        /// </summary>
        /// <param name="args">属性</param>
        protected override void OnEndPropertyChangingInner(PropertyChangingEventArgsEx args)
        {
            if (Equals(this.OriginalValues[args.PropertyName], args.NewValue))
            {
                this.OriginalValues.Remove(args.PropertyName);
                this.SetUnModify(args.PropertyName);
            }
            else
            {
                this.SetModify(args.PropertyName);
            }
        }

        /// <summary>
        ///     回退修改
        /// </summary>
        protected override void RejectChangedInner()
        {
            if (this._originalValues != null)
            {
                using (new EditScope(this))
                {
                    foreach (KeyValuePair<string, object> kv in this._originalValues)
                    {
                        this.EditObject.SetValue(kv.Key, kv.Value);
                    }
                }
            }
            base.RejectChangedInner();
        }
        /// <summary>
        /// 重置记录
        /// </summary>
        protected override void ResetRecords()
        {
            base.ResetRecords();
            this._originalValues.Clear();
        }
        #endregion


        #region 复制支持

        /// <summary>
        ///     复制修改状态
        /// </summary>
        /// <param name="target">要复制的源</param>
        protected override void CopyStateInner(EditStatus target)
        {
            var recordStatus = target as OriginalRecordStatus;
            if (recordStatus == null)
                return;
            if (recordStatus._originalValues == null || recordStatus._originalValues.Count == 0)
            {
                this._originalValues = null;
            }
            else
            {
                this._originalValues = new Dictionary<string, object>(recordStatus._originalValues);
            }
        }

        #endregion

    }
}
