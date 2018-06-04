#ifndef ___STDINC_H
#define ___STDINC_H
#pragma once
#include <omp.h>
#include <cassert>
#include <stdio.h>
#include <stddef.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <iostream>

#include <cstdio>
#include <csignal>
#include <ctime>
#include <string>

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
//#include <uuid/uuid.h>  

#include "acl/acl_cpp/lib_acl.hpp"
#include "acl/lib_acl.h"
#include "zeromq/zmq.h"
#include "boostinc.h"

//typedef uuid_t GUID;
typedef unsigned short ushort;
typedef unsigned char uchar;
typedef unsigned int uint;
typedef unsigned int DWORD;
typedef long long int64;
typedef unsigned long long uint64;
typedef void* HANDLE;

//线程中断
#define thread_sleep(n) boost::this_thread::sleep(boost::posix_time::milliseconds(n))
//启动线程
#define start_thread(func, args) \
{\
	boost::thread thread_xxx(boost::bind<DWORD>(&func, args));\
}

#define MAX_PATH 512


using namespace std;
#define var auto
#define astring acl::string;
#endif