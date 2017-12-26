#ifndef _TM_EXTEND_H
#define _TM_EXTEND_H
#pragma once
#include <ctime>
#include <cstdio>
#include <string>

#ifndef CLR
inline void today_str(char *str, int zone)
{
	time_t tt = time(nullptr);
	tt += 3600 * zone;
	tm date;
	localtime_s(&date, &tt);
	sprintf(str, "%d-%d-%d", date.tm_year, date.tm_mon, date.tm_mday);
}
#endif

//5分钟时间区间
inline int i_time_5minute()
{
	time_t tt = time(nullptr);
	tm date;
	localtime_s(&date, &tt);
	return (date.tm_hour << 8) + date.tm_min - (date.tm_min % 5);
}
//数字时间(HHmm)
inline int time_by_hhmm()
{
	time_t tt = time(nullptr);
	tm date;
	localtime_s(&date, &tt);
	return (date.tm_hour << 8) + date.tm_min;
}
inline void date_now(tm& date)
{
	time_t tt = time(nullptr);
	localtime_s(&date, &tt);
}
inline tm date_now()
{
	tm date;
	time_t tt = time(nullptr);
	localtime_s(&date, &tt);
	return date;
}
inline int i_today()
{
	time_t tt = time(nullptr);
	tm today;
	localtime_s(&today, &tt);
	return (today.tm_year << 16) + (today.tm_mon << 8) + today.tm_mday;
}
inline int i_yesterday()
{
	time_t tt = time(nullptr);
	tt -= 86400;
	tm today;
	localtime_s(&today, &tt);
	return (today.tm_year << 16) + (today.tm_mon << 8) + today.tm_mday;
}
//5分钟时间区间
inline int i_time_5minute(int zone)
{
	time_t tt = time(nullptr);
	tt += 3600 * zone;
	tm date;
	localtime_s(&date, &tt);
	return (date.tm_hour << 8) + date.tm_min - (date.tm_min % 5);
}
inline int i_today(int zone)
{
	time_t tt = time(nullptr);
	tt += 3600 * zone;
	tm today;
	localtime_s(&today, &tt);
	return (today.tm_year << 16) + (today.tm_mon << 8) + today.tm_mday;
}
inline int i_yesterday(int zone)
{
	time_t tt = time(nullptr);
	tt += 3600 * zone - 86400;
	tm today;
	localtime_s(&today, &tt);
	return (today.tm_year << 16) + (today.tm_mon << 8) + today.tm_mday;
}
//数字时间(HHmm)
inline int time_by_hhmm(int zone)
{
	time_t tt = time(nullptr);
	tt += 3600 * zone;
	tm date;
	localtime_s(&date, &tt);
	return (date.tm_hour << 8) + date.tm_min;
}
#define date_str(str)\
{\
	time_t tt = time(nullptr);\
	tm date;\
	localtime_s(&date, &tt);\
	sprintf_s(str, "%d-%d-%d", date.tm_year,date.tm_mon,date.tm_mday);\
}

template<size_t _Nm> inline FILETIME string2ftime(const char(&timeStr)[_Nm])
{
	FILETIME ft;
	if (timeStr[0] == 0)
		memset(&ft, 0, sizeof(FILETIME));
	else
	{
		SYSTEMTIME st;
		sscanf_s(timeStr, "%d-%d-%d %d:%d:%d.%d",
			&(st.wYear),
			&(st.wMonth),
			&(st.wDay),
			&(st.wHour),
			&(st.wMinute),
			&(st.wSecond),
			&(st.wMilliseconds));
		SystemTimeToFileTime(&st, &ft);
	}
	return ft;
}
template<size_t _Nm> inline tm string2time(const char(&timeStr)[_Nm])
{
	struct tm stTm;
	if (timeStr[0] == 0)
		memset(&stTm, 0, sizeof(tm));
	else
		sscanf_s(timeStr, "%d-%d-%d %d:%d:%d",
			&(stTm.tm_year),
			&(stTm.tm_mon),
			&(stTm.tm_mday),
			&(stTm.tm_hour),
			&(stTm.tm_min),
			&(stTm.tm_sec));
	return stTm;
}
template<size_t _Nm> inline void time2string(tm t, char(&timeStr)[_Nm])
{
	if (t.tm_year == 0 || t.tm_mday == 0)
		timeStr[0] = 0;
	else
		std::strftime(timeStr, sizeof(timeStr), "%Y-%m-%d %H:%M:%S", &t);
}
template<size_t _Nm> inline void time2string(char(&timeStr)[_Nm], tm t)
{
	if (t.tm_year == 0 || t.tm_mday == 0)
		timeStr[0] = 0;
	else
		std::strftime(timeStr, sizeof(timeStr), "%Y-%m-%d %H:%M:%S", &t);
}
#ifdef CLR
#pragma managed
using namespace System;
inline void FromClr(tm& t, DateTime^ dt)
{
	if (dt == DateTime::MinValue)
		memset(&t, 0, sizeof(tm));
	else
	{
		t.tm_year = dt->Year - 1900;
		t.tm_mon = dt->Month - 1;
		t.tm_mday = dt->Day;
		t.tm_year = dt->Year;
		t.tm_hour = dt->Hour;
		t.tm_min = dt->Minute;
		t.tm_sec = dt->Second;
	}
}
inline DateTime ToClr(tm t)
{
	DateTime^ dt = nullptr;
	if (t.tm_year <= 0 || t.tm_year > 200 || t.tm_mday <= 0 || t.tm_mday > 31 || t.tm_mon < 0 || t.tm_mon > 11)
		dt = gcnew DateTime();
	else
		dt = gcnew DateTime(1900 + t.tm_year, t.tm_mon + 1, t.tm_mday, t.tm_hour, t.tm_min, t.tm_sec);
	return *dt;
}
#endif
#define trade_day(commodity) time_by_hhmm(commodity->TimeZone) >= commodity->OpenTime ? i_today(commodity->TimeZone) : i_yesterday(commodity->TimeZone)
#endif