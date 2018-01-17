#ifndef ___STDINC_H
#define ___STDINC_H
#pragma once

#include "zeromq/zmq.h"

#include <cassert>
#include <stdio.h>
#include <tchar.h>
#include <stddef.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <iostream>

#include <string>
#include <vector>
#include <sstream>
#include <fstream>
#include <iostream>
#include <algorithm>
#include <set>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <string.h>
#include <time.h>
#include <assert.h>
#include <memory>
#include <queue>

#ifdef _WINDOWS
typedef unsigned long ulong;
typedef unsigned int uint;
typedef __int64 int64_t;
typedef __int64 int64;
typedef unsigned __int64 uint64;
#include <objbase.h>  
#else
#include <uuid/uuid.h>  
typedef uuid_t GUID;
#endif
#endif

typedef unsigned short ushort;
typedef unsigned short u_short;
typedef unsigned char uchar;
typedef unsigned char u_char;
typedef unsigned int u_int;

#include "acl.h"
#include "boostinc.h"
//线程中断
#define thread_sleep(n) boost::this_thread::sleep(boost::posix_time::milliseconds(n))
//启动线程
#define start_thread(func, args) \
{\
	boost::thread thread_xxx(boost::bind<DWORD>(&func, args));\
}

#include "log/mylogger.h"
#include "tm_extend.h"
#include "common.h"
#include "mydecimal.h"
using namespace std;