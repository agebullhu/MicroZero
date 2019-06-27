/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2019/1/17 17:18:44*/
#region
using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using Agebull.EntityModel.Common;
#endregion

namespace MicroZero.Devops.ZeroTracer.DataAccess
{
    /// <summary>
    /// 以流程方式记录日志
    /// </summary>
    public partial class FlowLogDataAccess
    {
        /// <summary>
        /// 构造
        /// </summary>
        public FlowLogDataAccess()
        {
            Name = FlowLogData._DataStruct_.EntityName;
            Caption = FlowLogData._DataStruct_.EntityCaption;
            Description = FlowLogData._DataStruct_.EntityDescription;
        }
        

        #region 基本SQL语句

        /// <summary>
        /// 表的唯一标识
        /// </summary>
        public override int TableId => FlowLogData._DataStruct_.EntityIdentity;

        /// <summary>
        /// 读取表名
        /// </summary>
        protected sealed override string ReadTableName
        {
            get
            {
                return @"tb_zt_flow_log";
            }
        }

        /// <summary>
        /// 写入表名
        /// </summary>
        protected sealed override string WriteTableName
        {
            get
            {
                return @"tb_zt_flow_log";
            }
        }

        /// <summary>
        /// 主键
        /// </summary>
        protected sealed override string PrimaryKey => FlowLogData._DataStruct_.EntityPrimaryKey;

        /// <summary>
        /// 全表读取的SQL语句
        /// </summary>
        protected sealed override string FullLoadFields
        {
            get
            {
                return @"
    `id` AS `Id`,
    `request_id` AS `RequestId`,
    `root_station` AS `RootStation`,
    `root_command` AS `RootCommand`,
    `record_date` AS `RecordDate`,
    `flow_json` AS `FlowJson`";
            }
        }

        

        /// <summary>
        /// 插入的SQL语句
        /// </summary>
        protected sealed override string InsertSqlCode
        {
            get
            {
                return @"
INSERT INTO `tb_zt_flow_log`
(
    `request_id`,
    `root_station`,
    `root_command`,
    `record_date`,
    `flow_json`
)
VALUES
(
    ?RequestId,
    ?RootStation,
    ?RootCommand,
    ?RecordDate,
    ?FlowJson
);
SELECT @@IDENTITY;";
            }
        }

        /// <summary>
        /// 全部更新的SQL语句
        /// </summary>
        protected sealed override string UpdateSqlCode
        {
            get
            {
                return @"
UPDATE `tb_zt_flow_log` SET
       `request_id` = ?RequestId,
       `root_station` = ?RootStation,
       `root_command` = ?RootCommand,
       `record_date` = ?RecordDate,
       `flow_json` = ?FlowJson
 WHERE `id` = ?Id;";
            }
        }
        #endregion


        #region 字段

        /// <summary>
        ///  所有字段
        /// </summary>
        static string[] _fields = new string[]{ "Id","RequestId","RootStation","RootCommand","RecordDate","FlowJson" };

        /// <summary>
        ///  所有字段
        /// </summary>
        public sealed override string[] Fields
        {
            get
            {
                return _fields;
            }
        }

        /// <summary>
        ///  字段字典
        /// </summary>
        public static Dictionary<string, string> fieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Id" , "id" },
            { "RequestId" , "request_id" },
            { "request_id" , "request_id" },
            { "RootStation" , "root_station" },
            { "root_station" , "root_station" },
            { "RootCommand" , "root_command" },
            { "root_command" , "root_command" },
            { "RecordDate" , "record_date" },
            { "record_date" , "record_date" },
            { "FlowJson" , "flow_json" },
            { "flow_json" , "flow_json" }
        };

        /// <summary>
        ///  字段字典
        /// </summary>
        public sealed override Dictionary<string, string> FieldMap
        {
            get { return fieldMap ; }
        }
        #endregion
        #region 方法实现


        /// <summary>
        /// 载入数据
        /// </summary>
        /// <param name="reader">数据读取器</param>
        /// <param name="entity">读取数据的实体</param>
        protected sealed override void LoadEntity(MySqlDataReader reader,FlowLogData entity)
        {
            if (!reader.IsDBNull(0))
                entity._id = (long)reader.GetInt64(0);
            if (!reader.IsDBNull(1))
                entity._requestId = reader.GetString(1).ToString();
            if (!reader.IsDBNull(2))
                entity._rootStation = reader.GetString(2).ToString();
            if (!reader.IsDBNull(3))
                entity._rootCommand = reader.GetString(3).ToString();
            if (!reader.IsDBNull(4))
                try { entity._recordDate = reader.GetMySqlDateTime(4).Value; } catch { }
            if (!reader.IsDBNull(5))
                entity._flowJson = reader.GetString(5).ToString();
        }

        /// <summary>
        /// 得到字段的DbType类型
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <returns>参数</returns>
        protected sealed override MySqlDbType GetDbType(string field)
        {
            switch (field)
            {
                case "Id":
                    return MySqlDbType.Int64;
                case "RequestId":
                    return MySqlDbType.VarString;
                case "RootStation":
                    return MySqlDbType.VarString;
                case "RootCommand":
                    return MySqlDbType.VarString;
                case "RecordDate":
                    return MySqlDbType.DateTime;
                case "FlowJson":
                    return MySqlDbType.VarString;
            }
            return MySqlDbType.VarChar;
        }


        /// <summary>
        /// 设置插入数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        /// <returns>返回真说明要取主键</returns>
        private void CreateFullSqlParameter(FlowLogData entity, MySqlCommand cmd)
        {
            //02:流水号(Id)
            cmd.Parameters.Add(new MySqlParameter("Id",MySqlDbType.Int64){ Value = entity.Id});
            //03:请求标识(RequestId)
            var isNull = string.IsNullOrWhiteSpace(entity.RequestId);
            var parameter = new MySqlParameter("RequestId",MySqlDbType.VarString , isNull ? 10 : (entity.RequestId).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.RequestId;
            cmd.Parameters.Add(parameter);
            //04:根站点(RootStation)
            isNull = string.IsNullOrWhiteSpace(entity.RootStation);
            parameter = new MySqlParameter("RootStation",MySqlDbType.VarString , isNull ? 10 : (entity.RootStation).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.RootStation;
            cmd.Parameters.Add(parameter);
            //05:根命令(RootCommand)
            isNull = string.IsNullOrWhiteSpace(entity.RootCommand);
            parameter = new MySqlParameter("RootCommand",MySqlDbType.VarString , isNull ? 10 : (entity.RootCommand).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.RootCommand;
            cmd.Parameters.Add(parameter);
            //06:记录时间(RecordDate)
            isNull = entity.RecordDate.Year < 1900;
            parameter = new MySqlParameter("RecordDate",MySqlDbType.DateTime);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.RecordDate;
            cmd.Parameters.Add(parameter);
            //07:流程内容的Json表示(FlowJson)
            isNull = string.IsNullOrWhiteSpace(entity.FlowJson);
            parameter = new MySqlParameter("FlowJson",MySqlDbType.VarString , isNull ? 10 : (entity.FlowJson).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.FlowJson;
            cmd.Parameters.Add(parameter);
        }


        /// <summary>
        /// 设置更新数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        protected sealed override void SetUpdateCommand(FlowLogData entity, MySqlCommand cmd)
        {
            cmd.CommandText = UpdateSqlCode;
            CreateFullSqlParameter(entity,cmd);
        }


        /// <summary>
        /// 设置插入数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        /// <returns>返回真说明要取主键</returns>
        protected sealed override bool SetInsertCommand(FlowLogData entity, MySqlCommand cmd)
        {
            cmd.CommandText = InsertSqlCode;
            CreateFullSqlParameter(entity, cmd);
            return true;
        }

        #endregion

    }
    
    partial class ZeroTracerDb
    {


        /// <summary>
        /// 以流程方式记录日志的结构语句
        /// </summary>
        private TableSql _tb_zt_flow_logSql = new TableSql
        {
            TableName = "tb_zt_flow_log",
            PimaryKey = "Id"
        };


        /// <summary>
        /// 以流程方式记录日志数据访问对象
        /// </summary>
        private FlowLogDataAccess _flowLogs;

        /// <summary>
        /// 以流程方式记录日志数据访问对象
        /// </summary>
        public FlowLogDataAccess FlowLogs
        {
            get
            {
                return this._flowLogs ?? ( this._flowLogs = new FlowLogDataAccess{ DataBase = this});
            }
        }
    }
}