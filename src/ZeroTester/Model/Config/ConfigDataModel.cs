using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Agebull.EntityModel.Config;
using Newtonsoft.Json;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    ///     配置基础
    /// </summary>
    public class ConfigDataModel<TConfig> : NotificationObject
        where TConfig : ConfigBase, new()
    {
        #region 设计
        
        /// <summary>
        ///     当前实例
        /// </summary>
        public SolutionConfig Solution => SolutionConfig.Current;

        /// <summary>
        ///     当前类型
        /// </summary>
        public string Type => Config?.GetType().Name;

        /// <summary>
        /// 引用对象键
        /// </summary>
        [DataMember, JsonProperty("ModelKey", NullValueHandling = NullValueHandling.Ignore)]
        internal Guid _modelKey;

        /// <summary>
        /// 引用对象键
        /// </summary>
        /// <remark>
        /// 引用对象键
        /// </remark>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统"), DisplayName("引用对象键"), Description("引用对象键，指实际包装的配置对象的引用")]
        public Guid ModelKey
        {
            get => _modelKey;
            set
            {
                if (_modelKey == value)
                    return;
                BeforePropertyChanged(nameof(ModelKey), _modelKey, value);
                _modelKey = value;
                OnPropertyChanged(nameof(ModelKey));
            }
        }

        private TConfig _config;

        /// <summary>
        /// 实际包装的配置对象
        /// </summary>
        public TConfig Config
        {
            get
            {
                if (_config != null)
                    return _config;
                if (!GlobalConfig.ConfigDictionary.TryGetValue(ModelKey, out ConfigBase model) || !(model is TConfig))
                {
                    TraceMessage.DefaultTrace.Track = $"引用键{ModelKey}无效";
                    ModelKey = Guid.Empty;
                }
                else
                {
                    _config = (TConfig)model;
                }
                return _config;
            }
            set
            {
                if (_config == value)
                    return;
                BeforePropertyChanged(nameof(Config), _config, value);
                _config = value;
                OnPropertyChanged(nameof(Config));
            }
        }

        #endregion

        #region 扩展配置

        /// <summary>
        /// 扩展配置
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        protected List<ConfigItem> _extendConfig;


        /// <summary>
        /// 扩展配置
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统")]
        [DisplayName("扩展配置")]
        public List<ConfigItem> ExtendConfig
        {
            get
            {
                if (_extendConfig != null)
                    return _extendConfig;
                _extendConfig = new List<ConfigItem>();
                BeforePropertyChanged(nameof(ExtendConfig), null, _extendConfig);
                return _extendConfig;
            }
            set
            {
                if (_extendConfig == value)
                    return;
                BeforePropertyChanged(nameof(ExtendConfig), _extendConfig, value);
                _extendConfig = value;
                OnPropertyChanged(nameof(ExtendConfig));
            }
        }

        [IgnoreDataMember, JsonIgnore]
        private ConfigItemList _extendConfigList;
        /// <summary>
        /// 扩展配置
        /// </summary>
        [IgnoreDataMember, JsonIgnore, Browsable(false)]
        public ConfigItemList ExtendConfigList => _extendConfigList ?? (_extendConfigList = new ConfigItemList(ExtendConfig));

        [IgnoreDataMember, JsonIgnore]
        private ConfigItemListBool _extendConfigListBool;

        /// <summary>
        /// 扩展配置
        /// </summary>
        [IgnoreDataMember, JsonIgnore, Browsable(false)]
        public ConfigItemListBool ExtendConfigListBool => _extendConfigListBool ?? (_extendConfigListBool = new ConfigItemListBool(ExtendConfig));

        /// <summary>
        /// 读写扩展配置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                return key == null ? null : ExtendConfig.FirstOrDefault(p => p.Name == key)?.Value;
            }
            set
            {
                if (key == null)
                    return;
                var mv = ExtendConfig.FirstOrDefault(p => p.Name == key);
                if (mv == null)
                {
                    ExtendConfig.Add(new ConfigItem { Name = key, Value = value });
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    ExtendConfig.Remove(mv);
                }
                else
                {
                    mv.Value = value.Trim();
                }
                RaisePropertyChanged(key);
            }
        }
        /// <summary>
        /// 试图取得扩展配置,如果不存在或为空则加入默认值后返回
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="def">默认值</param>
        /// <returns>扩展配置</returns>

        public string TryGetExtendConfig(string key, string def)
        {
            if (key == null)
                return def;
            var mv = ExtendConfig.FirstOrDefault(p => p.Name == key);
            if (mv != null)
                return mv.Value ?? (mv.Value = def);
            ExtendConfig.Add(new ConfigItem { Name = key, Value = def });
            return def;
        }

        #endregion

        #region 系统

        /// <summary>
        /// 是否预定义对象
        /// </summary>
        [IgnoreDataMember, JsonIgnore, Browsable(false), ReadOnly(true)]
        public bool IsPredefined;
        
        /// <summary>
        /// 是否参照对象
        /// </summary>
        [DataMember, JsonProperty("_isReference", NullValueHandling = NullValueHandling.Ignore)]
        internal bool _isReference;

        /// <summary>
        /// 是否参照对象
        /// </summary>
        /// <remark>
        /// 是否参照对象，是则永远只读
        /// </remark>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统"), DisplayName("是否参照对象"), Description("是否参照对象，是则永远只读")]
        public bool IsReference
        {
            get => _isReference;
            set
            {
                if (_isReference == value)
                    return;
                BeforePropertyChanged(nameof(IsReference), _isReference, value);
                _isReference = value;
                OnPropertyChanged(nameof(IsReference));
            }
        }

        /// <summary>
        /// 标识
        /// </summary>
        [DataMember, JsonProperty("_key", NullValueHandling = NullValueHandling.Ignore)]
        internal Guid _key;

        /// <summary>
        /// 标识
        /// </summary>
        /// <remark>
        /// 名称
        /// </remark>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统"), DisplayName("标识"), Description("名称")]
        public Guid Key
        {
            get => _key;
            set
            {
                if (_key == value)
                    return;
                BeforePropertyChanged(nameof(Key), _key, value);
                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataMember, JsonProperty("Identity", NullValueHandling = NullValueHandling.Ignore)]
        internal int _identity;

        /// <summary>
        /// 唯一标识
        /// </summary>
        /// <remark>
        /// 唯一标识
        /// </remark>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统"), DisplayName("唯一标识"), Description("唯一标识")]
        public int Identity
        {
            get => _identity;
            set
            {
                if (_identity == value)
                    return;
                BeforePropertyChanged(nameof(Identity), _identity, value);
                _identity = value;
                OnPropertyChanged(nameof(Identity));
            }
        }

        /// <summary>
        /// 编号
        /// </summary>
        [DataMember, JsonProperty("Index", NullValueHandling = NullValueHandling.Ignore)]
        internal int _index;

        /// <summary>
        /// 编号
        /// </summary>
        /// <remark>
        /// 编号
        /// </remark>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统"), DisplayName("编号"), Description("编号")]
        public int Index
        {
            get => _index;
            set
            {
                if (_index == value)
                    return;
                BeforePropertyChanged(nameof(Index), _index, value);
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        /// <summary>
        /// 废弃
        /// </summary>
        [DataMember, JsonProperty("Discard", NullValueHandling = NullValueHandling.Ignore)]
        internal bool _discard;

        /// <summary>
        /// 废弃
        /// </summary>
        /// <remark>
        /// 废弃
        /// </remark>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统"), DisplayName("废弃"), Description("废弃")]
        public bool Discard
        {
            get => _discard;
            set
            {
                if (_discard == value)
                    return;
                BeforePropertyChanged(nameof(Discard), _discard, value);
                _discard = value;
                OnPropertyChanged(nameof(Discard));
            }
        }

        /// <summary>
        /// 冻结
        /// </summary>
        [DataMember, JsonProperty("IsFreeze", NullValueHandling = NullValueHandling.Ignore)]
        internal bool _isFreeze;

        /// <summary>
        /// 冻结
        /// </summary>
        /// <remark>
        /// 如为真,此配置的更改将不生成代码
        /// </remark>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统"), DisplayName("冻结"), Description("如为真,此配置的更改将不生成代码")]
        public bool IsFreeze
        {
            get => _isFreeze;
            set
            {
                if (_isFreeze == value)
                    return;
                BeforePropertyChanged(nameof(IsFreeze), _isFreeze, value);
                _isFreeze = value;
                OnPropertyChanged(nameof(IsFreeze));
            }
        }

        /// <summary>
        /// 标记删除
        /// </summary>
        [DataMember, JsonProperty("_isDelete", NullValueHandling = NullValueHandling.Ignore)]
        internal bool _isDelete;

        /// <summary>
        /// 标记删除
        /// </summary>
        /// <remark>
        /// 如为真,保存时删除
        /// </remark>
        [IgnoreDataMember, JsonIgnore]
        [Category("系统"), DisplayName("标记删除"), Description("如为真,保存时删除")]
        public bool IsDelete
        {
            get => _isDelete;
            set
            {
                if (_isDelete == value)
                    return;
                BeforePropertyChanged(nameof(IsDelete), _isDelete, value);
                _isDelete = value;
                OnPropertyChanged(nameof(IsDelete));
            }
        }
        #endregion 系统 

        #region 扩展
        
        /// <summary>返回表示当前对象的字符串。</summary>
        /// <returns>表示当前对象的字符串。</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Config?.ToString() ?? "空引用";
        }

        #endregion
        
    }
}