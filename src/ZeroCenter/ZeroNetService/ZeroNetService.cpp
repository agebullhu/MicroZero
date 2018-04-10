// ZeroNetService.cpp : WinMain 的实现


#include "stdafx.h"
#include "resource.h"
#include "ZeroNetService_i.h"
#include "rpc/crpcservice.h"
#include "NetCommand/ZeroStation.h"
#include "NetCommand/NetDispatcher.h"
#include "NetCommand/BroadcastingStation.h"
#include <direct.h>
#include <stdio.h>
#include <stdlib.h>
#include <signal.h>

#ifndef STRICT
#define STRICT
#endif

#include "targetver.h"

#define _ATL_FREE_THREADED

#define _ATL_NO_AUTOMATIC_NAMESPACE

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// 某些 CString 构造函数将是显式的


#define ATL_NO_ASSERT_ON_DESTROY_NONEXISTENT_WINDOW

#include "resource.h"
#include <atlbase.h>
#include <atlcom.h>
#include <atlctl.h>

using namespace agebull;
using namespace ATL;

class CZeroNetServiceModule : public ATL::CAtlServiceModuleT< CZeroNetServiceModule, IDS_SERVICENAME >
{
public:
	DECLARE_LIBID(LIBID_ZeroNetServiceLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_ZERONETSERVICE, "{C654FDFF-ECA0-46E2-AF11-BBD417BD2417}")
	HRESULT InitializeSecurity() throw()
	{
		// TODO : 调用 CoInitializeSecurity 并为服务提供适当的安全设置
		// 建议 - PKT 级别的身份验证、
		// RPC_C_IMP_LEVEL_IDENTIFY 的模拟级别
		// 以及适当的非 NULL 安全描述符。

		return S_OK;
	}

	HRESULT RegisterAppId(_In_ bool bService=false) throw()
	{
		log_acl_msg("RegisterAppId");
		__super::RegisterAppId(bService);

		HRESULT hr = S_OK;
		if (bService)
		{
			if (IsInstalled())
			{
				SC_HANDLE hSCM = ::OpenSCManagerW(nullptr, nullptr, SERVICE_CHANGE_CONFIG);
				SC_HANDLE hService = nullptr;
				if (hSCM == nullptr)
				{
					hr = AtlHresultFromLastError();
				}
				else
				{
					hService = ::OpenService(hSCM, m_szServiceName, SERVICE_CHANGE_CONFIG);
					if (hService != nullptr)
					{
						::ChangeServiceConfig(hService, SERVICE_NO_CHANGE,
							SERVICE_AUTO_START,// 修改服务为自动启动
							NULL, nullptr, nullptr, nullptr, nullptr, nullptr, nullptr,
							m_szServiceName); // 通过修改资源IDS_SERVICENAME 修改服务的显示名字

						SERVICE_DESCRIPTION Description;
						TCHAR szDescription[1024];
						ZeroMemory(szDescription, 1024);
						ZeroMemory(&Description, sizeof(SERVICE_DESCRIPTION));
						lstrcpy(szDescription, _T("ZeroNet消息中心服务"));
						Description.lpDescription = szDescription;
						::ChangeServiceConfig2(hService, SERVICE_CONFIG_DESCRIPTION, &Description);
						::CloseServiceHandle(hService);
						log_acl_msg("Registe");
					}
					else
					{
						log_acl_msg("Fined");
						hr = AtlHresultFromLastError();
					}
					::CloseServiceHandle(hSCM);
				}
			}
			else
			{
				log_acl_msg("no Installed");
			}
		}
		log_acl_msg("RegisterAppId***");
		return hr;
	}

	HRESULT PreMessageLoop(int nShowCmd) throw()
	{
		log_acl_msg("PreMessageLoop");
		// 让暂停继续按钮可以使用
		m_status.dwControlsAccepted = m_status.dwControlsAccepted | SERVICE_ACCEPT_PAUSE_CONTINUE;

		HRESULT hr = __super::PreMessageLoop(nShowCmd);
		// 微软Bug
		if (hr == S_FALSE)
			hr = S_OK;

		if (SUCCEEDED(hr))
		{
			rpc_service::start();
			// 这个状态一定要修改，否则会出现1053错误，
			// 这个错误我花了很多时间才搞定
			SetServiceStatus(SERVICE_RUNNING);
		}

		return hr;
	}

	HRESULT PostMessageLoop() throw()
	{
		HRESULT hr = __super::PostMessageLoop();

		if (FAILED(hr))
		{
			log_acl_msg("PostMessageLoop FAILED");
			return hr;
		}

		log_acl_msg("PostMessageLoop succed");
		SetServiceStatus(SERVICE_RUNNING);
		return hr;
	}

	void OnStop() throw()
	{
		rpc_service::stop();
		__super::OnStop();
		SetServiceStatus(SERVICE_STOPPED);
	}

	void OnPause() throw()
	{
		log_acl_msg("OnPause");
		__super::OnPause();
		SetServiceStatus(SERVICE_PAUSED);
	}

	void OnContinue() throw()
	{
		log_acl_msg("OnContinue");
		//初始化网络库
		__super::OnContinue();
		SetServiceStatus(SERVICE_RUNNING);
	}
};

CZeroNetServiceModule _AtlModule;



//
extern "C" int WINAPI _tWinMain(HINSTANCE /*hInstance*/, HINSTANCE /*hPrevInstance*/,
	LPTSTR /*lpCmdLine*/, int nShowCmd)
{
	return _AtlModule.WinMain(nShowCmd);
}

