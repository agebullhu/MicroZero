#pragma once
/*  =====================================================================
zhelpers.h
Helper header file for example applications.
=====================================================================
*/

#ifndef __ZHELPERS_H_INCLUDED__
#define __ZHELPERS_H_INCLUDED__

//  Include a bunch of headers that we will need in the examples

#include <zeromq/zmq.h>
#include <assert.h>
#include <signal.h>
#include <stdarg.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#if (!defined (WIN32))
#   include <sys/time.h>
#endif

#if (defined (WIN32))
#   include <windows.h>
#endif

//  Receive 0MQ string from socket and convert into C string
//  Caller must free returned string. Returns NULL if the context
//  is being terminated.

//  Receives all message parts from socket, prints neatly
//
static void
s_dump(void* socket)
{
	int rc;

	zmq_msg_t message;
	rc = zmq_msg_init(&message);
	assert(rc == 0);

	puts("----------------------------------------");
	//  Process all parts of the message
	do
	{
		int size = zmq_msg_recv(&message, socket, 0);
		assert(size >= 0);

		//  Dump the message as text or binary
		char* data = static_cast<char*>(zmq_msg_data(&message));
		assert(data != nullptr);
		int is_text = 1;
		int char_nbr;
		for (char_nbr = 0; char_nbr < size; char_nbr++)
		{
			if (static_cast<unsigned char>(data[char_nbr]) < 32
				|| static_cast<unsigned char>(data[char_nbr]) > 126)
			{
				is_text = 0;
			}
		}

		printf("[%03d] ", size);
		for (char_nbr = 0; char_nbr < size; char_nbr++)
		{
			if (is_text)
			{
				printf("%c", data[char_nbr]);
			}
			else
			{
				printf("%02X", static_cast<unsigned char>(data[char_nbr]));
			}
		}
		printf("\n");
	} while (zmq_msg_more(&message));

	rc = zmq_msg_close(&message);
	assert(rc == 0);
}



//  Sleep for a number of milliseconds
static void
s_sleep(int msecs)
{
#if (defined (WIN32))
	Sleep(msecs);
#else
	struct timespec t;
	t.tv_sec = msecs / 1000;
	t.tv_nsec = (msecs % 1000) * 1000000;
	nanosleep(&t, NULL);
#endif
}

//  Return current system clock as milliseconds
static int64_t s_clock()
{
#if (defined (WIN32))
	SYSTEMTIME st;
	GetSystemTime(&st);
	return static_cast<int64_t>(st.wSecond) * 1000 + st.wMilliseconds;
#else
	struct timeval tv;
	gettimeofday(&tv, NULL);
	return (int64_t)(tv.tv_sec * 1000 + tv.tv_usec / 1000);
#endif
}

//  Print formatted string to stdout, prefixed by date/time and
//  terminated with a newline.

static void s_console(const char* format, ...)
{
	time_t curtime = time(nullptr);
	struct tm* loctime = localtime(&curtime);
	char* formatted = static_cast<char*>(malloc(20));
	strftime(formatted, 20, "%y-%m-%d %H:%M:%S ", loctime);
	printf("%s", formatted);
	free(formatted);

	va_list argptr;
	va_start(argptr, format);
	vprintf(format, argptr);
	va_end(argptr);
	printf("\n");
}

#endif //  __ZHELPERS_H_INCLUDED__
