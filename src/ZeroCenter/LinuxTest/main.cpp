#include <cstdio>
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
//#include <boost/thread.hpp>
//#include <boost/thread/recursive_mutex.hpp>
//#include <boost/thread/shared_mutex.hpp>
//#include <boost/bind.hpp>
//#include <boost/algorithm/string.hpp>
//#include <boost/format.hpp>
//#include <boost/enable_shared_from_this.hpp>
//#include <boost/version.hpp>
//#include <boost/asio/strand.hpp>
////#include <boost/function/function_template.hpp>
//#include <boost/asio/placeholders.hpp>
//#include <boost/asio/deadline_timer.hpp>
//#include <boost/lexical_cast.hpp>
//#include <boost/format.hpp> 
//#include <boost/unordered_map.hpp> 
//#include <boost/interprocess/sync/interprocess_semaphore.hpp>

#include <ctime>
#include <cstdio>
#include <string>

int main()
{
	auto time = boost::posix_time::ptime(boost::posix_time::microsec_clock::universal_time());
    printf("hello from LinuxTest!\n");
    return 0;
}