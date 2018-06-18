#include "stdafx.h"
#include "service.h"
#include "net/zero_station.h"
#include <mcheck.h>  

int main(int argc, char *argv[])
{
	//加入内存检测
	//setenv("MALLOC_TRACE", "output", 1);
	//mtrace();
	//初始化
	if (!agebull::zmq_net::rpc_service::initialize())
		return 0;
	//agebull::zmq_net::station_warehouse::clear();
	//启动
	agebull::zmq_net::rpc_service::start();
#ifdef _DEBUG
	while (agebull::zmq_net::get_net_state() == agebull::zmq_net::NET_STATE_RUNING)
	{
		std::cout << endl << "Enter command:";
		string line;
		getline(cin, line);
		if (line.length() == 0)
			break;

		if (line == "clear")
		{
			agebull::zmq_net::station_warehouse::clear();
			break;
		}
		std::vector<string> cmdline;
		// boost::is_any_of这里相当于分割规则了  
		boost::split(cmdline, line, boost::is_any_of(" \n\r\t"));
		if (cmdline.empty())
			break;
		std::vector<agebull::zmq_net::shared_char> arguments;
		for (size_t idx = 1; idx < cmdline.size(); idx++)
			arguments.emplace_back(cmdline[idx]);
		const string result = agebull::zmq_net::station_dispatcher::exec_command(cmdline[0].c_str(), arguments);
		std::cout << result << endl;
	}
#endif
	//等待
	agebull::zmq_net::rpc_service::wait_zero();
	//关闭
	agebull::zmq_net::rpc_service::stop();
	return 0;
}
