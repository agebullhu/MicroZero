#pragma once
#include <stdinc.h>

class ApiRoute
{
	/*
	* @brief API服务名称
	*/
	string _serviceName;

	/*
	* @brief 路由地址
	*/
	string _routeAddress;

	/*
	* @brief 工作地址
	*/
	string _workerAddress;

	/*
	* @brief 心跳地址
	*/
	string _heartAddress;
	/*
	* @brief 调用句柄
	*/
	void* _routeSocket;

	/*
	* @brief 工作句柄
	*/
	void* _workSocket;
	/*
	* @brief 心跳句柄
	*/
	void* _heartSocket;

	/*
	* @brief 应该继续执行
	*/
	bool _isRuning;

	/*
	* @brief API主机集合
	*/
	map<string, string> workers;
	/*
	* @brief API主机集合
	*/
	vector<string> _hosts;

	/*
	* @brief 当前工作者下标
	*/
	int _nowWorkIndex;
public:
	/*
	* @brief 构造
	*/
	ApiRoute(const char* service, const char* routeAddress, const char* workerAddress, const char* heartAddress);
	/*
	* @brief 析构
	*/
	~ApiRoute();
	/*
	* @brief 执行
	*/
	static DWORD ApiRoute::start(ApiRoute& route)
	{
		route.poll();
		return 0;
	};
	/*
	* @brief 结束
	*/
	static DWORD ApiRoute::end(ApiRoute& route)
	{
		route._isRuning = false;
		thread_sleep(1000);
		return 0;
	};
private:
	/*
	* @brief 执行
	*/
	void poll();
	/*
	* @brief 工作集合的响应
	*/
	void onWorkerPollIn();
	/*
	* @brief 调用集合的响应
	*/
	void onCallerPollIn();
	bool snedToWorker(char* work, char* client_addr, char* request);
	/*
	* 心跳的响应
	*/
	void onHeartbeat();

	/*
	* @brief 取下一个工作对象
	*/
	char* getNextWorker();
	void leftWorker(char* name);
	void joinWorker(char* name, char* address,bool ready=false);
};
/*
 * 运行一个路由线程
 */
inline void startRoute(ApiRoute& route)
{
	boost::thread thrds_s1(boost::bind(ApiRoute::start, route));
}
