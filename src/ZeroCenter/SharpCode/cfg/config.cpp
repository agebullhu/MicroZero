#include "config.h"
#include "Psapi.h"

#ifdef WIN32
#include <direct.h>
#else
#include <unistd.h>  
#endif
namespace agebull
{
	std::map<std::string, std::string> config::m_machine_cfg;


	void config::init()
	{
		if (m_machine_cfg.empty())
		{
			std::string path = "";
			GetProcessFilePath(path);
			path.append("\\config.json");
			log_acl_trace(0, 3, path.c_str());

			ACL_VSTREAM *fp = acl_vstream_fopen(path.c_str(), O_RDONLY, 0700, 8192);
			if (fp == nullptr)
				return;
			char buf[1024];
			int ret = 0;
			acl::string cfg;
			while (ret != ACL_VSTREAM_EOF) {
				ret = acl_vstream_gets_nonl(fp, buf, sizeof(buf));
				cfg += buf;
			}
			acl_vstream_fclose(fp);
			read(cfg, m_machine_cfg);
		}
	}

	void config::read(acl::string& str, std::map<std::string, std::string>& cfg)
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
	std::string& config::get_config(const char * name)
	{
		init();
		return m_machine_cfg[name];
	}

	int config::get_int(const char * name)
	{
		init();
		auto vl = m_machine_cfg[name];
		return vl.empty() ? 0 : atoi(vl.c_str());
	}

	bool config::boolean(const char * name)
	{
		init();
		auto vl = m_machine_cfg[name];
		return !vl.empty() && strcasecmp(vl.c_str(), "true");
	}
	int config::number(const char * name)
	{
		init();
		auto vl = m_cfg[name];
		return vl.empty() ? 0 : atoi(vl.c_str());
	}

	config::config(const char* json)
	{
		read(acl::string(json), m_cfg);
	}
	std::string& config::operator[](const char * name)
	{
		return m_cfg[name];
	}
	/* 功  能：获取指定进程所对应的可执行（EXE）文件全路径
	* 参  数：hProcess - 进程句柄。必须具有PROCESS_QUERY_INFORMATION 或者
	PROCESS_QUERY_LIMITED_INFORMATION 权限
	*         sFilePath - 进程句柄hProcess所对应的可执行文件路径
	* 返回值：
	*/
	void GetProcessFilePath(OUT string& sFilePath)
	{
#ifndef WIN32

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
		char buffer[MAX_PATH + 1];
		sFilePath = _getcwd(buffer, MAX_PATH);
#endif
	}
}