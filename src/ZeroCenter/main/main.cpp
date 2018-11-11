#include "../stdafx.h"
#include "service.h"
#include <mcheck.h>  
#include "../rpc/message_storage.h"
using namespace agebull;
int zero_center_main();
void test_sqlite();
int main(int argc, char *argv[])
{
	//test_sqlite();
	//getchar();
	//return 0;
	return zero_center_main();
}

int zero_center_main()
{
	//加入内存检测
	//setenv("MALLOC_TRACE", "output", 1);
	//mtrace();
	//初始化
	if (!zero_net::rpc_service::initialize())
		return 0;
	//启动
	zero_net::rpc_service::start();
	//等待
	zero_net::rpc_service::wait_zero();
	//关闭
	zero_net::rpc_service::stop();
	return 0;
}

void test_sqlite()
{
	if (!zero_net::rpc_service::initialize())
		return;
	var tm = boost::posix_time::microsec_clock::local_time();
	vector<vector<zero_net::shared_char>> datas;
	zero_net::message_storage storage;
	shared_ptr < zero_net::zero_config>  config(new zero_net::zero_config());
	config->station_name_ = "Test";
	storage.prepare_storage(config);
	auto sp = boost::posix_time::microsec_clock::local_time() - tm;
	log_msg2("prepare(%lldms) : %lld", sp.total_milliseconds(), storage.get_last_id());
	tm = boost::posix_time::microsec_clock::local_time();
	for (int i = 0; i < 10000; i++)
		storage.save("agebull", "test", "abc", "{}", "a", i);
	sp = boost::posix_time::microsec_clock::local_time() - tm;
	log_msg1("write(%lldms)", sp.total_milliseconds());
	tm = boost::posix_time::microsec_clock::local_time();

	storage.load(0, 100000, [](vector<zero_net::shared_char>& data)
	{
	});
	sp = boost::posix_time::microsec_clock::local_time() - tm;
	log_msg1("load(%lldms)", sp.total_milliseconds());
	//for (auto data : datas)
	//{
	//	cout << "row: ";
	//	for (auto col : data)
	//	{
	//		cout << col.get_buffer() << " ";
	//	}
	//	cout << endl;
	//}
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