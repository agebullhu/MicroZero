#include "main.h"

int main(int argc, char *argv[])
{
	return zero_center_main();
}
void wait_commane();
int zero_center_main()
{
	cout << "*********************************************" << endl;
	//加入内存检测
	//valgrind --tool=memcheck --leak-check=full mtrace ./zero_center.out output
	//setenv("MALLOC_TRACE", "output", 1);
	//mtrace();
	//初始化
	if (zero_net::rpc_service::initialize())
	{
		//zero_net::station_warehouse::clear();
		//启动
		zero_net::rpc_service::start();
		//wait_commane();
		//等待
		zero_net::rpc_service::wait_zero();
		//关闭
		zero_net::rpc_service::stop();
	}
	cout << endl << "*********************************************" << endl;
	return 0;
}

void wait_commane()
{
#ifdef _DEBUG_
	//agebull::zero_net::station_warehouse::clear();
	while (agebull::zero_net::get_net_state() == zero_net::zero_def::net_state::runing)
	{
		std::cout << endl << "Enter command:";
		string line;
		getline(cin, line);
		if (line.length() == 0)
			continue;

		if (line == "restart")
		{
			agebull::zero_net::station_dispatcher::instance->restart();
			continue;
		}
		if (line == "close")
		{
			break;
		}
		//std::vector<string> cmdline;
		//// boost::is_any_of这里相当于分割规则了
		//boost::split(cmdline, line, boost::is_any_of(" \n\r\t"));
		//if (cmdline.empty())
		//	continue;
		//std::vector<agebull::zero_net::shared_char> arguments;
		//for (size_t idx = 1; idx < cmdline.size(); idx++)
		//	arguments.emplace_back(cmdline[idx]);
		//const char result = agebull::zero_net::station_dispatcher::exec_command(cmdline[0].c_str(), arguments);
		//std::cout << result << endl;
	}
#endif

}