using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.EntityModel.Config
{
    /// <summary>
    ///     配置基础
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class SimpleConfig : NotificationObject
    {
        /// <summary>
        ///     名称
        /// </summary>
        [DataMember, JsonProperty("_name", NullValueHandling = NullValueHandling.Ignore)] private string _name;

        /// <summary>
        ///     名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore, Category("设计支持"), DisplayName(@"名称")]
        public virtual string Name
        {
            get => _name;
            set
            {
                var now = !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
                if (_name == now)
                {
                    return;
                }
                BeforePropertyChanged(nameof(Name), _name, now);
                _name = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                RaisePropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        ///     标题
        /// </summary>
        [DataMember, JsonProperty("_caption", NullValueHandling = NullValueHandling.Ignore)]
        protected string _caption;

        /// <summary>
        ///     标题
        /// </summary>
        [IgnoreDataMember, JsonIgnore, Category("设计支持"), DisplayName(@"标题")]
        public virtual string Caption
        {
            get => WorkContext.InCoderGenerating ? _caption ?? _name : _caption;
            set
            {
                if (_caption == value)
                {
                    return;
                }
                if (value == _name)
                    value = null;
                BeforePropertyChanged(nameof(Caption), _caption, value);
                _caption = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                RaisePropertyChanged(nameof(Caption));
            }
        }

        /// <summary>
        ///     说明
        /// </summary>
        [DataMember, JsonProperty("_description", NullValueHandling = NullValueHandling.Ignore)]
        protected string _description;

        /// <summary>
        ///     说明
        /// </summary>
        [IgnoreDataMember, JsonIgnore, Category("设计支持"), DisplayName(@"说明")]
        public virtual string Description
        {
            get => WorkContext.InCoderGenerating ? _description ?? Caption : _description;
            set
            {
                var now = !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
                if (_description == now)
                {
                    return;
                }
                if (value == _caption)
                    value = null;
                BeforePropertyChanged(nameof(Description), _description, now);
                _description = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                RaisePropertyChanged(nameof(Description));
            }
        }
        private string _remark;
        /// <summary>
        /// 参见
        /// </summary>
        [IgnoreDataMember, JsonIgnore, Category("设计支持"), DisplayName(@"参见")]
        public virtual string Remark
        {
            get => _remark;
            set
            {
                var now = !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
                if (_remark == now)
                {
                    return;
                }
                BeforePropertyChanged(nameof(Description), _description, now);
                _remark = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                RaisePropertyChanged(nameof(Description));
            }
        }
        /// <summary>
        /// 显示文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name == null
                ? Caption
                : Caption == null
                    ? Name
                    : $"{Name}({Caption})";
        }

        /// <summary>
        /// 字段复制
        /// </summary>
        /// <param name="dest"></param>
        /// <returns></returns>
        public void Copy(SimpleConfig dest)
        {
            using (WorkModelScope.CreateScope(WorkModel.Loding))
            {
                CopyFrom(dest);
            }
        }

        /// <summary>
        /// 字段复制
        /// </summary>
        /// <param name="dest"></param>
        /// <returns></returns>
        protected virtual void CopyFrom(SimpleConfig dest)
        {
            Name = dest.Name;
            Caption = dest.Caption;
            Description = dest.Description;
            Remark = dest.Remark;
        }
    }
}