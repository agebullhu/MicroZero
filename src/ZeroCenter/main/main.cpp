#include "main.h"

int main(int argc, char *argv[])
{
	return zero_center_main();
}

int zero_center_main()
{
	//加入内存检测
	//valgrind --tool=memcheck --leak-check=full ./zero_center.out 
	setenv("MALLOC_TRACE", "output", 1);
	mtrace();
	//初始化
	if (!zero_net::rpc_service::initialize())
		return 0;
	//zero_net::station_warehouse::clear();
	//启动
	zero_net::rpc_service::start();
	//等待
	zero_net::rpc_service::wait_zero();
	//关闭
	zero_net::rpc_service::stop();
	return 0;
}

/*
#ifdef _DEBUG
	//agebull::zero_net::station_warehouse::clear();
	while (agebull::zero_net::get_net_state() == agebull::zero_net::NET_STATE_RUNING)
	{
		std::cout << endl << "Enter command:";
		string line;
		getline(cin, line);
		if (line.length() == 0)
			break;

		if (line == "clear")
		{
			agebull::zero_net::station_warehouse::clear();
			break;
		}
		std::vector<string> cmdline;
		// boost::is_any_of这里相当于分割规则了
		boost::split(cmdline, line, boost::is_any_of(" \n\r\t"));
		if (cmdline.empty())
			break;
		std::vector<agebull::zero_net::shared_char> arguments;
		for (size_t idx = 1; idx < cmdline.size(); idx++)
			arguments.emplace_back(cmdline[idx]);
		const string result = agebull::zero_net::station_dispatcher::exec_command(cmdline[0].c_str(), arguments);
		std::cout << result << endl;
	}
#endif
 */