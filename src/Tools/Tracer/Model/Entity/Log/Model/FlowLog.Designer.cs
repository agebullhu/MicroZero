/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2019/1/17 16:55:16*/
#region
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Agebull.EntityModel.Common;


#endregion

namespace MicroZero.Devops.ZeroTracer
{
    /// <summary>
    /// 以流程方式记录日志
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class FlowLogData : IIdentityData
    {
        #region 构造
        
        /// <summary>
        /// 构造
        /// </summary>
        public FlowLogData()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize();
        #endregion

        #region 基本属性


        /// <summary>
        /// 修改主键
        /// </summary>
        public void ChangePrimaryKey(long id)
        {
            _id = id;
        }
        /// <summary>
        /// 流水号
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public long _id;

        partial void OnIdGet();

        partial void OnIdSet(ref long value);

        partial void OnIdLoad(ref long value);

        partial void OnIdSeted();

        
        /// <summary>
        /// 流水号
        /// </summary>
        [DataMember , JsonIgnore , ReadOnly(true) , DisplayName(@"流水号")]
        public long Id
        {
            get
            {
                OnIdGet();
                return this._id;
            }
            set
            {
                if(this._id == value)
                    return;
                //if(this._id > 0)
                //    throw new Exception("主键一旦设置就不可以修改");
                OnIdSet(ref value);
                this._id = value;
                this.OnPropertyChanged(_DataStruct_.Real_Id);
                OnIdSeted();
            }
        }
        /// <summary>
        /// 请求标识
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _requestId;

        partial void OnRequestIdGet();

        partial void OnRequestIdSet(ref string value);

        partial void OnRequestIdSeted();

        
        /// <summary>
        /// 请求标识
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("requestId", NullValueHandling = NullValueHandling.Ignore) , DisplayName(@"请求标识")]
        public  string RequestId
        {
            get
            {
                OnRequestIdGet();
                return this._requestId;
            }
            set
            {
                if(this._requestId == value)
                    return;
                OnRequestIdSet(ref value);
                this._requestId = value;
                OnRequestIdSeted();
                this.OnPropertyChanged(_DataStruct_.Real_RequestId);
            }
        }
        /// <summary>
        /// 根站点
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _rootStation;

        partial void OnRootStationGet();

        partial void OnRootStationSet(ref string value);

        partial void OnRootStationSeted();

        
        /// <summary>
        /// 根站点
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("rootStation", NullValueHandling = NullValueHandling.Ignore) , DisplayName(@"根站点")]
        public  string RootStation
        {
            get
            {
                OnRootStationGet();
                return this._rootStation;
            }
            set
            {
                if(this._rootStation == value)
                    return;
                OnRootStationSet(ref value);
                this._rootStation = value;
                OnRootStationSeted();
                this.OnPropertyChanged(_DataStruct_.Real_RootStation);
            }
        }
        /// <summary>
        /// 根命令
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _rootCommand;

        partial void OnRootCommandGet();

        partial void OnRootCommandSet(ref string value);

        partial void OnRootCommandSeted();

        
        /// <summary>
        /// 根命令
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("rootCommand", NullValueHandling = NullValueHandling.Ignore) , DisplayName(@"根命令")]
        public  string RootCommand
        {
            get
            {
                OnRootCommandGet();
                return this._rootCommand;
            }
            set
            {
                if(this._rootCommand == value)
                    return;
                OnRootCommandSet(ref value);
                this._rootCommand = value;
                OnRootCommandSeted();
                this.OnPropertyChanged(_DataStruct_.Real_RootCommand);
            }
        }
        /// <summary>
        /// 记录时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public DateTime _recordDate;

        partial void OnRecordDateGet();

        partial void OnRecordDateSet(ref DateTime value);

        partial void OnRecordDateSeted();

        
        /// <summary>
        /// 记录时间
        /// </summary>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("recordDate", NullValueHandling = NullValueHandling.Ignore) , JsonConverter(typeof(MyDateTimeConverter)) , DisplayName(@"记录时间")]
        public  DateTime RecordDate
        {
            get
            {
                OnRecordDateGet();
                return this._recordDate;
            }
            set
            {
                if(this._recordDate == value)
                    return;
                OnRecordDateSet(ref value);
                this._recordDate = value;
                OnRecordDateSeted();
                this.OnPropertyChanged(_DataStruct_.Real_RecordDate);
            }
        }
        /// <summary>
        /// 流程内容的Json表示
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _flowJson;

        partial void OnFlowJsonGet();

        partial void OnFlowJsonSet(ref string value);

        partial void OnFlowJsonSeted();

        
        /// <summary>
        /// 流程内容的Json表示
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("flowJson", NullValueHandling = NullValueHandling.Ignore) , DisplayName(@"流程内容的Json表示")]
        public  string FlowJson
        {
            get
            {
                OnFlowJsonGet();
                return this._flowJson;
            }
            set
            {
                if(this._flowJson == value)
                    return;
                OnFlowJsonSet(ref value);
                this._flowJson = value;
                OnFlowJsonSeted();
                this.OnPropertyChanged(_DataStruct_.Real_FlowJson);
            }
        }

        #region 接口属性

        #endregion
        #region 扩展属性

        #endregion
        #endregion


        #region 名称的属性操作

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected override void SetValueInner(string property, object value)
        {
            if(property == null) return;
            switch(property.Trim().ToLower())
            {
            case "id":
                this.Id = (long)Convert.ToDecimal(value);
                return;
            case "requestid":
                this.RequestId = value == null ? null : value.ToString();
                return;
            case "rootstation":
                this.RootStation = value == null ? null : value.ToString();
                return;
            case "rootcommand":
                this.RootCommand = value == null ? null : value.ToString();
                return;
            case "recorddate":
                this.RecordDate = Convert.ToDateTime(value);
                return;
            case "flowjson":
                this.FlowJson = value == null ? null : value.ToString();
                return;
            }

            //System.Diagnostics.Trace.WriteLine(property + @"=>" + value);

        }

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void SetValueInner(int index, object value)
        {
            switch(index)
            {
            case _DataStruct_.Id:
                this.Id = Convert.ToInt64(value);
                return;
            case _DataStruct_.RequestId:
                this.RequestId = value == null ? null : value.ToString();
                return;
            case _DataStruct_.RootStation:
                this.RootStation = value == null ? null : value.ToString();
                return;
            case _DataStruct_.RootCommand:
                this.RootCommand = value == null ? null : value.ToString();
                return;
            case _DataStruct_.RecordDate:
                this.RecordDate = Convert.ToDateTime(value);
                return;
            case _DataStruct_.FlowJson:
                this.FlowJson = value == null ? null : value.ToString();
                return;
            }
        }


        /// <summary>
        ///     读取属性值
        /// </summary>
        /// <param name="property"></param>
        protected override object GetValueInner(string property)
        {
            switch(property)
            {
            case "id":
                return this.Id;
            case "requestid":
                return this.RequestId;
            case "rootstation":
                return this.RootStation;
            case "rootcommand":
                return this.RootCommand;
            case "recorddate":
                return this.RecordDate;
            case "flowjson":
                return this.FlowJson;
            }

            return null;
        }


        /// <summary>
        ///     读取属性值
        /// </summary>
        /// <param name="index"></param>
        protected override object GetValueInner(int index)
        {
            switch(index)
            {
                case _DataStruct_.Id:
                    return this.Id;
                case _DataStruct_.RequestId:
                    return this.RequestId;
                case _DataStruct_.RootStation:
                    return this.RootStation;
                case _DataStruct_.RootCommand:
                    return this.RootCommand;
                case _DataStruct_.RecordDate:
                    return this.RecordDate;
                case _DataStruct_.FlowJson:
                    return this.FlowJson;
            }

            return null;
        }

        #endregion

        #region 复制
        

        partial void CopyExtendValue(FlowLogData source);

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        protected override void CopyValueInner(DataObjectBase source)
        {
            var sourceEntity = source as FlowLogData;
            if(sourceEntity == null)
                return;
            this._id = sourceEntity._id;
            this._requestId = sourceEntity._requestId;
            this._rootStation = sourceEntity._rootStation;
            this._rootCommand = sourceEntity._rootCommand;
            this._recordDate = sourceEntity._recordDate;
            this._flowJson = sourceEntity._flowJson;
            CopyExtendValue(sourceEntity);
            this.__EntityStatus.SetModified();
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="source">复制的源字段</param>
        public void Copy(FlowLogData source)
        {
                this.Id = source.Id;
                this.RequestId = source.RequestId;
                this.RootStation = source.RootStation;
                this.RootCommand = source.RootCommand;
                this.RecordDate = source.RecordDate;
                this.FlowJson = source.FlowJson;
        }
        #endregion

        #region 后期处理
        

        /// <summary>
        /// 单个属性修改的后期处理(保存后)
        /// </summary>
        /// <param name="subsist">当前实体生存状态</param>
        /// <param name="modifieds">修改列表</param>
        /// <remarks>
        /// 对当前对象的属性的更改,请自行保存,否则将丢失
        /// </remarks>
        protected override void OnLaterPeriodBySignleModified(EntitySubsist subsist,byte[] modifieds)
        {
            if (subsist == EntitySubsist.Deleting)
            {
                OnIdModified(subsist,false);
                OnRequestIdModified(subsist,false);
                OnRootStationModified(subsist,false);
                OnRootCommandModified(subsist,false);
                OnRecordDateModified(subsist,false);
                OnFlowJsonModified(subsist,false);
                return;
            }
            else if (subsist == EntitySubsist.Adding || subsist == EntitySubsist.Added)
            {
                OnIdModified(subsist,true);
                OnRequestIdModified(subsist,true);
                OnRootStationModified(subsist,true);
                OnRootCommandModified(subsist,true);
                OnRecordDateModified(subsist,true);
                OnFlowJsonModified(subsist,true);
                return;
            }
            else if(modifieds != null && modifieds[6] > 0)
            {
                OnIdModified(subsist,modifieds[_DataStruct_.Real_Id] == 1);
                OnRequestIdModified(subsist,modifieds[_DataStruct_.Real_RequestId] == 1);
                OnRootStationModified(subsist,modifieds[_DataStruct_.Real_RootStation] == 1);
                OnRootCommandModified(subsist,modifieds[_DataStruct_.Real_RootCommand] == 1);
                OnRecordDateModified(subsist,modifieds[_DataStruct_.Real_RecordDate] == 1);
                OnFlowJsonModified(subsist,modifieds[_DataStruct_.Real_FlowJson] == 1);
            }
        }

        /// <summary>
        /// 流水号修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 请求标识修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnRequestIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 根站点修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnRootStationModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 根命令修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnRootCommandModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 记录时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnRecordDateModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 流程内容的Json表示修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnFlowJsonModified(EntitySubsist subsist,bool isModified);
        #endregion

        #region 数据结构

        /// <summary>
        /// 实体结构
        /// </summary>
        [IgnoreDataMember,Browsable (false)]
        public override EntitySturct __Struct
        {
            get
            {
                return _DataStruct_.Struct;
            }
        }
        /// <summary>
        /// 实体结构
        /// </summary>
        public class _DataStruct_
        {
            /// <summary>
            /// 实体名称
            /// </summary>
            public const string EntityName = @"FlowLog";
            /// <summary>
            /// 实体标题
            /// </summary>
            public const string EntityCaption = @"流程日志";
            /// <summary>
            /// 实体说明
            /// </summary>
            public const string EntityDescription = @"以流程方式记录日志";
            /// <summary>
            /// 实体标识
            /// </summary>
            public const int EntityIdentity = 0x0;
            /// <summary>
            /// 实体说明
            /// </summary>
            public const string EntityPrimaryKey = "Id";
            
            
            /// <summary>
            /// 流水号的数字标识
            /// </summary>
            public const byte Id = 1;
            
            /// <summary>
            /// 流水号的实时记录顺序
            /// </summary>
            public const int Real_Id = 0;

            /// <summary>
            /// 请求标识的数字标识
            /// </summary>
            public const byte RequestId = 2;
            
            /// <summary>
            /// 请求标识的实时记录顺序
            /// </summary>
            public const int Real_RequestId = 1;

            /// <summary>
            /// 根站点的数字标识
            /// </summary>
            public const byte RootStation = 3;
            
            /// <summary>
            /// 根站点的实时记录顺序
            /// </summary>
            public const int Real_RootStation = 2;

            /// <summary>
            /// 根命令的数字标识
            /// </summary>
            public const byte RootCommand = 4;
            
            /// <summary>
            /// 根命令的实时记录顺序
            /// </summary>
            public const int Real_RootCommand = 3;

            /// <summary>
            /// 记录时间的数字标识
            /// </summary>
            public const byte RecordDate = 5;
            
            /// <summary>
            /// 记录时间的实时记录顺序
            /// </summary>
            public const int Real_RecordDate = 4;

            /// <summary>
            /// 流程内容的Json表示的数字标识
            /// </summary>
            public const byte FlowJson = 6;
            
            /// <summary>
            /// 流程内容的Json表示的实时记录顺序
            /// </summary>
            public const int Real_FlowJson = 5;

            /// <summary>
            /// 实体结构
            /// </summary>
            public static readonly EntitySturct Struct = new EntitySturct
            {
                EntityName = EntityName,
                Caption    = EntityCaption,
                Description= EntityDescription,
                PrimaryKey = EntityPrimaryKey,
                EntityType = EntityIdentity,
                Properties = new Dictionary<int, PropertySturct>
                {
                    {
                        Real_Id,
                        new PropertySturct
                        {
                            Index        = Id,
                            Name         = "Id",
                            Title        = "流水号",
                            Caption      = @"流水号",
                            Description  = @"流水号",
                            ColumnName   = "id",
                            PropertyType = typeof(long),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            CanImport    = false,
                            CanExport    = false
                        }
                    },
                    {
                        Real_RequestId,
                        new PropertySturct
                        {
                            Index        = RequestId,
                            Name         = "RequestId",
                            Title        = "请求标识",
                            Caption      = @"请求标识",
                            Description  = @"请求标识",
                            ColumnName   = "request_id",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            CanImport    = false,
                            CanExport    = false
                        }
                    },
                    {
                        Real_RootStation,
                        new PropertySturct
                        {
                            Index        = RootStation,
                            Name         = "RootStation",
                            Title        = "根站点",
                            Caption      = @"根站点",
                            Description  = @"根站点",
                            ColumnName   = "root_station",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            CanImport    = false,
                            CanExport    = false
                        }
                    },
                    {
                        Real_RootCommand,
                        new PropertySturct
                        {
                            Index        = RootCommand,
                            Name         = "RootCommand",
                            Title        = "根命令",
                            Caption      = @"根命令",
                            Description  = @"根命令",
                            ColumnName   = "root_command",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            CanImport    = false,
                            CanExport    = false
                        }
                    },
                    {
                        Real_RecordDate,
                        new PropertySturct
                        {
                            Index        = RecordDate,
                            Name         = "RecordDate",
                            Title        = "记录时间",
                            Caption      = @"记录时间",
                            Description  = @"记录时间",
                            ColumnName   = "record_date",
                            PropertyType = typeof(DateTime),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            CanImport    = false,
                            CanExport    = false
                        }
                    },
                    {
                        Real_FlowJson,
                        new PropertySturct
                        {
                            Index        = FlowJson,
                            Name         = "FlowJson",
                            Title        = "流程内容的Json表示",
                            Caption      = @"流程内容的Json表示",
                            Description  = @"流程内容的Json表示",
                            ColumnName   = "flow_json",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            CanImport    = false,
                            CanExport    = false
                        }
                    }
                }
            };
        }
        #endregion

    }
}