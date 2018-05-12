// WindowsService.cpp : WinMain 的实现

#include "stdafx.h"
#include "rpc/CRpcService.h"
#include "net/ZeroStation.h"
#include "net/NetDispatcher.h"
#include "net/BroadcastingStation.h"

using namespace agebull;
void my_function(int sig) { // can be called asynchronously
	boost::thread(boost::bind(agebull::distory_net_command));
}

int main(int argc, char *argv[])
{
	if (!rpc_service::initialize())
		return 0;
	//agebull::zmq_net::station_warehouse::clear();
	rpc_service::start();
	signal(SIGINT, my_function);
	while (get_net_state() == NET_STATE_RUNING)
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
		std::vector<agebull::zmq_net::sharp_char> arguments;
		for (size_t idx = 1; idx < cmdline.size(); idx++)
			arguments.emplace_back(cmdline[idx]);
		const string result = agebull::zmq_net::station_dispatcher::exec_command(cmdline[0].c_str(), arguments);
		std::cout << result << endl;
	}
	rpc_service::stop();
	std::cout << endl << "shutdown";
	thread_sleep(200);
	return 0;
}
