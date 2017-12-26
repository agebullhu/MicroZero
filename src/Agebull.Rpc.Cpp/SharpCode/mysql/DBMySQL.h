#pragma once
#include <stdafx.h>
using namespace std;

namespace GBS
{
	namespace Futures
	{
		namespace DataModel
		{

			typedef struct
			{
				string  m_host;
				int     m_port;
				string  m_db;
				string  m_user;
				string  m_password;
			}MySQLConf;

			class CDBMySQL
			{
			public:
				string  m_message;
				int     m_state;

				CDBMySQL();
				CDBMySQL(MySQLConf& mysqlConf);
				~CDBMySQL();
				bool Open();
				bool Close();

				/// <summary>
				///     执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
				/// </summary>
				/// <param name="sql">SQL语句</param>
				/// <param name="args">参数</param>
				/// <returns>操作的第一行第一列或空</returns>
				/// <remarks>
				///     注意,如果有参数时,都是匿名参数,请使用?序号的形式访问参数
				/// </remarks>
				string ExecuteScalar(const string& sql);
				/// <summary>
				///     执行查询，并返回查询所影响的行数
				/// </summary>
				/// <param name="sql">SQL语句</param>
				/// <returns>查询所影响的行数</returns>
				int CDBMySQL::Execute(const string& sql);
				/// <summary>
				///     执行查询，并返回结果数据
				/// </summary>
				/// <param name="sql">SQL语句</param>
				/// <returns>结果数据</returns>
				template<typename TData>
				std::vector<TData> Query(const string& sql);

			protected:
				string m_serverIP;
				unsigned int m_serverPort;
				string m_mysqlUser;
				string m_mysqlPass;
				string m_mysqlDb;
				MYSQL*   m_mysql;
				boost::mutex m_mutex;
			private:
				//void PingDataBase();
				//boost::asio::io_service m_ios;
				//std::shared_ptr<boost::asio::deadline_timer> m_timer;
			};

		}
	}
}