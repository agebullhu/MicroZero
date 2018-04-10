#include "SimpleEvent.h"

#ifdef _MSC_VER

simple_event::simple_event()
{
	cond = CreateEvent(nullptr, true, false, nullptr);
}

simple_event::~simple_event()
{
	CloseHandle(cond);
}

void simple_event::SignalEvent() const
{
	PulseEvent(cond);
}

void simple_event::WaitEvent(DWORD time_out) const
{
	WaitForSingleObject(cond, time_out);
}


#endif //_MSC_VER


#ifdef __GNUC__	
#include <errno.h>

simple_event::simple_event()
{
        bIsSignal = false;
	pthread_cond_init( &cond, NULL);
	pthread_mutex_init( &mutex, NULL);
}

simple_event::~simple_event()
{
	pthread_cond_destroy( &cond);
	pthread_mutex_destroy( &mutex);
}


void simple_event::SignalEvent()
{
    pthread_mutex_lock(&mutex);
	bIsSignal =true;
	pthread_cond_signal( &cond);
    pthread_mutex_unlock(&mutex);
}

void simple_event::WaitEvent()
{
    pthread_mutex_lock(&mutex);
    bIsSignal = false;
    while(!bIsSignal){
		pthread_cond_wait( &cond, &mutex);
    }
    pthread_mutex_unlock(&mutex);

}


#endif // __GNUC__

