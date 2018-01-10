#pragma once
#if BOOST_VERSION < 105000
#define XT_TIME_UTC boost::TIME_UTC
#else
#define XT_TIME_UTC boost::TIME_UTC_
#endif
// treat some warnings as errors
#ifdef WIN32
#pragma warning (error: 4002)
#pragma warning (error: 4667)
#pragma warning (error: 4715)
#pragma warning (error: 4390)       // if (xxx) ;
#pragma warning (error: 4172)       // return address of local temp memory
#endif // WIN32

#include <boost/shared_ptr.hpp>
/*
这两个文件不要在预编译头文件中include
否则在release下如果使用预编译头文件会最终link出错（至少在vs2008 sp1中如此）
#include <boost/asio.hpp>
#include <boost/asio/io_service.hpp>
*/
#ifndef WIN32
#include <boost/asio.hpp>
#include <boost/asio/io_service.hpp>
#endif

#include <iostream>
#include <string>
#include <boost/thread.hpp>
#include <boost/thread/recursive_mutex.hpp>
#include <boost/thread/shared_mutex.hpp>
#include <boost/bind.hpp>
#include <boost/algorithm/string.hpp>
#include <boost/format.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/version.hpp>
#include <boost/asio/strand.hpp>
//#include <boost/function/function_template.hpp>
#include <boost/asio/placeholders.hpp>
#include <boost/asio/deadline_timer.hpp>
#include <boost/lexical_cast.hpp>
#include <boost/format.hpp> 
#include <boost/unordered_map.hpp> 
#include <boost/interprocess/sync/interprocess_semaphore.hpp>
