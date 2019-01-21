#ifndef MH_STDINC_H
#define MH_STDINC_H
#pragma once
#include <omp.h>
#include <cassert>
#include <cstdio>
#include <cstddef>
#include <cstring>
#include <cstdio>
#include <cstdlib>
#include <iostream>

#include <utility>
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
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstdarg>
#include <cstring>
#include <ctime>
#include <cassert>
#include <memory>
#include <queue>
#include <uuid/uuid.h>  
#include "acl/acl_cpp/lib_acl.hpp"
#include "acl/lib_acl.h"
#include "zmq.h"
#include "boostinc.h"
#include <netdb.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <cstdio>
#include <csignal>
#include <ctime>
#include <execinfo.h>
#include <string>

//typedef uuid_t GUID;
typedef unsigned short ushort;
typedef unsigned char uchar;
typedef unsigned int uint;
typedef unsigned int dword;
typedef long long int64;
typedef unsigned long long uint64;
typedef void* handle;

//线程中断
#define THREAD_SLEEP(n) boost::this_thread::sleep(boost::posix_time::milliseconds(n))
//启动线程
#define START_THREAD(func, args) \
{\
	boost::thread thread_xxx(boost::bind<DWORD>(&func, args));\
}

#define MAX_PATH 512


using namespace std;
#define var auto
#define astring acl::string;
#endif