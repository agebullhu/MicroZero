// stdafx.h : 标准系统包含文件的包含文件，
// 或是经常使用但不常更改的
// 特定于项目的包含文件

#pragma once

#ifndef STRICT
#define STRICT
#endif

#include "targetver.h"

#define _ATL_FREE_THREADED

#define _ATL_NO_AUTOMATIC_NAMESPACE

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// 某些 CString 构造函数将是显式的


#define ATL_NO_ASSERT_ON_DESTROY_NONEXISTENT_WINDOW

#include "resource.h"
//#include <atlbase.h>
//#include <atlcom.h>
//#include <atlctl.h>



#include "stdinc.h"

#include "NetCommand/net_command.h"
#include "tson/tson_def.h"
#include "tson/tson_deserializer.h"
#include "tson/tson_serializer.h"

#include <cfg/config.h>
#include "redis/redis.h"
#include "NetCommand/command_serve.h"

#include "debug/TraceStack.h"
