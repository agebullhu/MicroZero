#pragma once
#include "stdinc.h"

#include "NetCommand/net_command.h"
#include "tson/tson_def.h"
#include "tson/tson_deserializer.h"
#include "tson/tson_serializer.h"

#include <cfg/config.h>
#include "redis/redis.h"
#include "NetCommand/command_serve.h"
#include "debug/TraceStack.h"

#define WIN32_LEAN_AND_MEAN             // 从 Windows 头中排除极少使用的资料
// Windows 头文件: 
#include <windows.h>
#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS      // 某些 CString 构造函数将是显式的

//#include <atlbase.h>
//#include <atlstr.h>
