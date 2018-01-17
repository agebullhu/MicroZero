#include "../Stdafx.h"
#include "mysql/mysql_com.h"
#include "DBMySQL.h"

namespace GBS
{
	namespace Futures
	{
		namespace DataModel
		{

			CDBMySQL::CDBMySQL()
				: m_state(0)
				, m_serverPort(0), m_mysql(nullptr)
			{
			}


			CDBMySQL::CDBMySQL(MySQLConf& mysqlConf)
				: m_state(0)
				, m_mysql(nullptr)
				//, m_timer(new boost::asio::deadline_timer(m_ios, boost::posix_time::seconds(10)))
			{
				m_serverIP = mysqlConf.m_host;
				m_serverPort = mysqlConf.m_port;
				m_mysqlUser = mysqlConf.m_user;
				m_mysqlPass = mysqlConf.m_password;
				m_mysqlDb = mysqlConf.m_db;
			}
			CDBMySQL::~CDBMySQL()
			{
				Close();
			}

			bool CDBMySQL::Close()
			{
				//m_timer->cancel();
				boost::lock_guard<boost::mutex> guard(m_mutex);
				if (nullptr != m_mysql)
				{
					mysql_close(m_mysql);
					return true;
				}
				return false;
			}

			bool CDBMySQL::Open()
			{
				if (m_mysql != nullptr)
					return true;

				MYSQL* mysql = mysql_init(nullptr);
				if (mysql != nullptr && mysql_real_connect(mysql, m_serverIP.c_str(), m_mysqlUser.c_str(), m_mysqlPass.c_str(), m_mysqlDb.c_str(), m_serverPort, nullptr, CLIENT_MULTI_STATEMENTS))
				{
					if (mysql_set_server_option(mysql, MYSQL_OPTION_MULTI_STATEMENTS_ON))
					{
						boost::lock_guard<boost::mutex> guard(m_mutex);
						m_mysql = nullptr;
						return false;
					}

					my_bool reconnect = 1;
					if (mysql_options(mysql, MYSQL_OPT_RECONNECT, &reconnect))
					{
						boost::lock_guard<boost::mutex> guard(m_mutex);
						m_mysql = nullptr;
						return false;
					}

					if (mysql_set_character_set(mysql, "utf8"))
					{
						boost::lock_guard<boost::mutex> guard(m_mutex);
						m_mysql = nullptr;
						return false;
					}
				}
				else
				{
					boost::lock_guard<boost::mutex> guard(m_mutex);
					m_mysql = nullptr;
					return false;
				}
				logger_debug(DEBUG_TEST1, 1, "DBMySQL connect to %s:%d success!", m_serverIP.c_str(), m_serverPort);
				{
					boost::lock_guard<boost::mutex> guard(m_mutex);
					m_mysql = mysql;
				}

				//m_timer->expires_from_now(boost::posix_time::seconds(5));
				//m_timer->async_wait(boost::bind(&CDBMySQL::PingDataBase, this, _1));
				return true;
			}
			/*/通过心跳检查数据库连接是否存在
			void CDBMySQL::PingDataBase()
			{
				bool reinit = false;
				{
					boost::lock_guard<boost::mutex> guard(m_mutex);
					if (m_mysql != nullptr)
					{
						unsigned long olderId = mysql_thread_id(m_mysql);
						mysql_ping(m_mysql);
						unsigned long newId = mysql_thread_id(m_mysql);
						reinit = olderId != newId;
					}
				}

				if (reinit)
				{
					Close();
					Open();
				}
				else
				{
					m_timer->expires_from_now(boost::posix_time::seconds(5));
					m_timer->async_wait(boost::bind(&CDBMySQL::PingDataBase, this, _1));
				}
			}*/

			/// <summary>
			///     执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
			/// </summary>
			/// <param name="sql">SQL语句</param>
			/// <returns>操作的第一行第一列或空</returns>
			string CDBMySQL::ExecuteScalar(const string& sql)
			{
				boost::lock_guard<boost::mutex> guard(m_mutex);
				m_state = 0;
				m_message = "";
				if (mysql_real_query(m_mysql, sql.c_str(), sql.size()))
				{
					m_message = "执行失败";
					m_state = -1;
					return "";
				}
				MYSQL_RES* pRes = mysql_store_result(m_mysql);
				if (pRes == nullptr)
				{
					m_message = "执行失败";
					m_state = -2;
					return "";
				}
				string result;
				MYSQL_ROW sqlRow = mysql_fetch_row(pRes);
				if (nullptr == sqlRow)
				{
					m_message = "没有返回值";
					m_state = -3;
					mysql_free_result(pRes);
				}
				else
				{
					sqlRow = mysql_fetch_row(pRes);
					result = sqlRow[0];
				}
				mysql_free_result(pRes);
				return result;
			}
			/// <summary>
			///     执行查询，并返回查询所影响的行数
			/// </summary>
			/// <param name="sql">SQL语句</param>
			/// <returns>查询所影响的行数</returns>
			int CDBMySQL::Execute(const string& sql)
			{
				boost::lock_guard<boost::mutex> guard(m_mutex);
				m_state = 0;
				m_message = "";
				if (mysql_real_query(m_mysql, sql.c_str(), sql.size()))
				{
					m_message = "执行失败";
					m_state = -1;
					return -1;
				}
				MYSQL_RES* pRes = mysql_store_result(m_mysql);
				if (pRes == nullptr)
				{
					m_message = "执行失败";
					m_state = -2;
					return -1;
				}
				int result = 0;
				MYSQL_ROW sqlRow = mysql_fetch_row(pRes);
				if (nullptr == sqlRow)
				{
					m_message = "没有返回值";
					m_state = -3;
					mysql_free_result(pRes);
				}
				else
				{
					sqlRow = mysql_fetch_row(pRes);
					result = boost::lexical_cast<int>(sqlRow[0]);
				}
				mysql_free_result(pRes);
				return result;
			}
			/// <summary>
			///     执行查询，并返回结果数据
			/// </summary>
			/// <param name="sql">SQL语句</param>
			/// <returns>结果数据</returns>
			template<typename TData>
			std::vector<TData> CDBMySQL::Query(const string& sql)
			{
				std::vector<TData> result;
				boost::lock_guard<boost::mutex> guard(m_mutex);
				vector<IDataPtr> datas;
				m_state = 0;
				if (mysql_real_query(m_mysql, sql.c_str(), sql.size()))
				{
					m_message = ("查询失败");
					m_state = -1;
					return result;
				}
				MYSQL_RES* pRes = mysql_store_result(m_mysql);
				if (pRes == nullptr)
				{
					m_message = "执行失败";
					m_state = -2;
					return result;
				}
				MYSQL_FIELD* fields = nullptr;
				int numFields = mysql_num_fields(pRes);
				fields = mysql_fetch_fields(pRes);
				MYSQL_ROW sqlRow = mysql_fetch_row(pRes);
				while (nullptr != sqlRow)
				{
					TData data;
					memset(&data, 0, sizeof(TData));
					for (int i = 0; i < numFields; ++i)
					{
						data.SetValue(fields[i].name, sqlRow[i]);
					}
					sqlRow = mysql_fetch_row(pRes);
				}
				mysql_free_result(pRes);
				return result;
			}

		}
	}
}