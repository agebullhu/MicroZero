#include "stdafx.h"
#include "service.h"
#include "net/zero_station.h"
#include "net/station_dispatcher.h"

int main(int argc, char *argv[])
{
	if (!agebull::zmq_net::rpc_service::initialize())
		return 0;
	//agebull::zmq_net::station_warehouse::clear();
	agebull::zmq_net::rpc_service::start();
	signal(SIGINT, agebull::zmq_net::on_sig);
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
		std::vector<agebull::zmq_net::sharp_char> arguments;
		for (size_t idx = 1; idx < cmdline.size(); idx++)
			arguments.emplace_back(cmdline[idx]);
		const string result = agebull::zmq_net::station_dispatcher::exec_command(cmdline[0].c_str(), arguments);
		std::cout << result << endl;
	}
	/*agebull::zmq_net::rpc_service::stop();*/
	thread_sleep(200);
#else
	agebull::zmq_net::wait_zero();
#endif
	return 0;
}
