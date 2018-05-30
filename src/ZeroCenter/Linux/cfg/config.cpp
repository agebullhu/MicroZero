#include "config.h"
#if WIN32
#include "Psapi.h"
#else
#include <unistd.h>  
#endif

namespace agebull
{
	std::map<std::string, std::string> config::global_cfg_;
	/**
	* \brief 系统根目录
	*/
	acl::string config::root_path;

	/**
	* \brief 全局配置初始化
	*/
	void config::init()
	{
		if (global_cfg_.empty())
		{
			char buf[1024];
			std::string path=getcwd(buf, 512);
			path.append("/config.json");
			//log_acl_trace(0, 3, path.c_str());

			ACL_VSTREAM *fp = acl_vstream_fopen(path.c_str(), O_RDONLY, 0700, 8192);
			if (fp == nullptr)
				return;
			int ret = 0;
			acl::string cfg;
			while (ret != ACL_VSTREAM_EOF) {
				ret = acl_vstream_gets_nonl(fp, buf, sizeof(buf));
				cfg += buf;
			}
			acl_vstream_fclose(fp);
			read(cfg.c_str(), global_cfg_);
		}
	}
	/**
	* \brief 读取配置内容
	*/
	void config::read(const char* str, std::map<std::string, std::string>& cfg)
	{
		cfg.clear();
		acl::json json;
		json.update(str);
		acl::json_node* iter = json.first_node();
		while (iter)
		{
			if (iter->tag_name())
			{
				cfg.insert(std::make_pair(iter->tag_name(), iter->get_text()));
			}
			iter = json.next_node();
		}
	}
	/**
	* \brief 构造
	* \param json JSON内容
	*/
	config::config(const char* json)
	{
		read(acl::string(json), value_map_);
	}
	/**
	* \brief 取全局配置
	* \param name 名称
	* \return 值
	*/
	std::string& config::get_global_string(const char * name)
	{
		init();
		return global_cfg_[name];
	}

	/**
	* \brief 取全局配置
	* \param name 名称
	* \return 值
	*/
	int config::get_global_int(const char * name)
	{
		init();
		auto vl = global_cfg_[name];
		return vl.empty() ? 0 : atoi(vl.c_str());
	}
	/**
	* \brief 取全局配置
	* \param name 名称
	* \return 值
	*/
	bool config::get_global_bool(const char * name)
	{
		init();
		auto vl = global_cfg_[name];
		return !vl.empty() && strcasecmp(vl.c_str(), "true") == 0;
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 布尔
	*/
	bool config::boolean(const char * name)
	{
		init();
		auto vl = value_map_[name];
		return !vl.empty() && strcasecmp(vl.c_str(), "true") == 0;
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 数字
	*/
	int config::number(const char * name)
	{
		init();
		auto vl = value_map_[name];
		return vl.empty() ? 0 : atoi(vl.c_str());
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 文本
	*/
	std::string& config::operator[](const char * name)
	{
		return value_map_[name];
	}

	/**
	* \brief 获取指定进程所对应的可执行（EXE）文件全路径
	* \param sFilePath - 进程句柄hProcess所对应的可执行文件路径
	* /
	void get_process_file_path(string& sFilePath)
	{
#if WIN32

		char tsFileDosPath[MAX_PATH + 1];
		ZeroMemory(tsFileDosPath, sizeof(char)*(MAX_PATH + 1));

		HANDLE hProcess = GetCurrentProcess();
		DWORD re = GetProcessImageFileNameA(hProcess, tsFileDosPath, MAX_PATH + 1);
		CloseHandle(hProcess);
		if (0 == re)
		{
			return;
		}

		// 获取Logic Drive String长度
		UINT uiLen = GetLogicalDriveStrings(0, nullptr);
		if (0 == uiLen)
		{
			return;
		}

		char* pLogicDriveString = new char[uiLen + 1];
		ZeroMemory(pLogicDriveString, uiLen + 1);
		uiLen = GetLogicalDriveStringsA(uiLen, pLogicDriveString);
		if (0 == uiLen)
		{
			delete[]pLogicDriveString;
			return;
		}

		char szDrive[3] = " :";
		char* pDosDriveName = new char[MAX_PATH];
		char* pLogicIndex = pLogicDriveString;

		do
		{
			szDrive[0] = *pLogicIndex;
			uiLen = QueryDosDeviceA(szDrive, pDosDriveName, MAX_PATH);
			if (0 == uiLen)
			{
				if (ERROR_INSUFFICIENT_BUFFER != GetLastError())
				{
					break;
				}

				delete[]pDosDriveName;
				pDosDriveName = new char[uiLen + 1];
				uiLen = QueryDosDeviceA(szDrive, pDosDriveName, uiLen + 1);
				if (0 == uiLen)
				{
					break;
				}
			}

			uiLen = strlen(pDosDriveName);
			if (0 == _strnicmp(tsFileDosPath, pDosDriveName, uiLen))
			{
				sFilePath.append(szDrive);
				sFilePath.append(tsFileDosPath + uiLen);
				break;
			}

			while (*pLogicIndex++);
		} while (*pLogicIndex);

		delete[]pLogicDriveString;
		delete[]pDosDriveName;
#else
		char buf[512];
		getcwd(buf, 512);
		sFilePath = buf;
#endif
	}*/

}