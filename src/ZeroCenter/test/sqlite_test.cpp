#include "../main/main.h"

void do_sqlite()
{
	agebull::zero_net::station_warehouse::clear();
	if (!zero_net::rpc_service::initialize())
		return;
	var tm = boost::posix_time::microsec_clock::local_time();
	vector<vector<zero_net::shared_char>> datas;
	zero_net::queue_storage storage;
	shared_ptr < zero_net::zero_config>  config(new zero_net::zero_config());
	config->station_name = "Test";
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

int test_sqlite()
{
	do_sqlite();
	getchar();
	return 0;
}