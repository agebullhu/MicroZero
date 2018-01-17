#pragma once
#include "../mysql/DBMySQL.h"


namespace GBS
{
	namespace Futures
	{
		namespace DataModel
		{
			template<class TData, int FieldCount>
			class MySqlAccessBase
			{
			public:
				//数据库对象
				std::shared_ptr<CDBMySQL> m_msql;
			public:
				/**
				* @brief 析构
				*/
				virtual ~MySqlAccessBase()
				{
					m_msql->Close();
				}

				/**
				* @brief 默认构造
				*/
				MySqlAccessBase()
					: m_msql(new CDBMySQL())
				{
					m_msql->Open();
				}
				/**
				* @brief 复制构造
				* @param {shared_ptr<CDBMySQL>} mysql 数据库对象
				*/
				MySqlAccessBase(shared_ptr<CDBMySQL> mysql)
					: m_msql(mysql)
				{
				}
				/**
				* @brief 新增SQL语句生成
				* @param {TData} data 数据
				*/
				virtual string InsertSql(TData data) = 0;

				/**
				* @brief 新增
				* @param {DATA_STATE} state 数据状态
				*/
				void Insert(TData data)
				{
					string sql = InsertSql(data);
					string key = m_msql->ExecuteScalar(sql);
					data.SetValue(nullptr, key.c_str());//空字段表示写主键
				}
				/**
				* @brief 更新SQL语句生成
				* @param {TData} data 数据
				*/
				virtual string UpdateSql(TData data) = 0;

				/**
				* @brief 新增
				* @param {DATA_STATE} state 数据状态
				*/
				void Update(TData data)
				{
					string sql = UpdateSql(data);
					m_msql->Execute(sql);
				}
				/**
				* @brief 查询SQL语句生成
				* @param {string} condition 查询条件
				*/
				virtual string QuerySql(string condition) = 0;

				/**
				* @brief 查询
				* @param {string} condition 查询条件
				*/
				vector<TData> Query(string condition)
				{
					string sql = QuerySql(condition);
					return m_msql->Query<TData>(sql);
				}
				/**
				* @brief 删除SQL语句生成
				* @param {string} condition 删除条件
				*/
				virtual string DeleteSql(string condition) = 0;

				/**
				* @brief 删除
				* @param {string} condition 删除条件
				*/
				void Delete(string condition)
				{
					string sql = DeleteSql(condition);
					m_msql->Execute(sql);
				}
			};
		}
	}

}

