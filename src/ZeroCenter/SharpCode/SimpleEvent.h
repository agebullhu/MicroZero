#ifndef SIMPLE_EVENT_H
#define SIMPLE_EVENT_H

#ifdef _MSC_VER
#include <WinSock2.h>
#include <Windows.h>
#endif //_MSC_VER

#ifdef __GNUC__
#include <pthread.h>
#endif //__GNUC__

class SimpleEvent
{

public:
	SimpleEvent();
	~SimpleEvent();

	void SignalEvent() const;
	void WaitEvent(DWORD time_out = INFINITE) const;

private:	
#ifdef __GNUC__
	pthread_cond_t cond;
	pthread_mutex_t mutex;
    bool bIsSignal;
#endif // __GNUC__
	
#ifdef _MSC_VER
	HANDLE cond;
#endif //_MSC_VER

};

#endif //SIMPLE_EVENT_H
