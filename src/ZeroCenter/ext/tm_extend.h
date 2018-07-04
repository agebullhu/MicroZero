#ifndef _TM_EXTEND_H
#define _TM_EXTEND_H
#pragma once
#include <ctime>
#include <cstdio>
#include <string>
namespace agebull
{
	inline void today_str(char *str, int zone)
	{
		time_t tt = time(nullptr);
		tt += 3600 * zone;
		tm* date = localtime(&tt);
		sprintf(str, "%d-%d-%d", date->tm_year, date->tm_mon, date->tm_mday);
	}
	//5分钟时间区间
	inline int i_time_5minute()
	{
		time_t tt = time(nullptr);
		tm* date = localtime(&tt);
		return (date->tm_hour << 8) + date->tm_min - (date->tm_min % 5);
	}
	//数字时间(HHmm)
	inline int time_by_hhmm()
	{
		time_t tt = time(nullptr);
		tm* date = localtime(&tt);
		return (date->tm_hour << 8) + date->tm_min;
	}
	inline void date_now(tm& date)
	{
		time_t tt = time(nullptr);
		tm* d = localtime(&tt);
		memcpy(&date, d, sizeof(tm));
	}
	inline tm date_now()
	{
		time_t tt = time(nullptr);
		tm* date = localtime(&tt);
		return *date;
	}
	inline int i_today()
	{
		time_t tt = time(nullptr);
		tm* date = localtime(&tt);
		return (date->tm_year << 16) + (date->tm_mon << 8) + date->tm_mday;
	}
	inline int i_yesterday()
	{
		time_t tt = time(nullptr);
		tt -= 86400;
		tm* date = localtime(&tt);
		return (date->tm_year << 16) + (date->tm_mon << 8) + date->tm_mday;
	}
	//5分钟时间区间
	inline int i_time_5minute(int zone)
	{
		time_t tt = time(nullptr);
		tt += 3600 * zone;
		tm* date = localtime(&tt);
		return (date->tm_hour << 8) + date->tm_min - (date->tm_min % 5);
	}
	inline int i_today(int zone)
	{
		time_t tt = time(nullptr);
		tt += 3600 * zone;
		tm* date = localtime(&tt);
		return (date->tm_year << 16) + (date->tm_mon << 8) + date->tm_mday;
	}
	inline int i_yesterday(int zone)
	{
		time_t tt = time(nullptr);
		tt += 3600 * zone - 86400;
		tm* date = localtime(&tt);
		return (date->tm_year << 16) + (date->tm_mon << 8) + date->tm_mday;
	}
	//数字时间(HHmm)
	inline int time_by_hhmm(int zone)
	{
		time_t tt = time(nullptr);
		tt += 3600 * zone;
		tm* date = localtime(&tt);
		return (date->tm_hour << 8) + date->tm_min;
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

#define time_span(ms) boost::posix_time::ptime(boost::posix_time::microsec_clock::universal_time()) + boost::posix_time::microseconds(ms)


#define date_str(str)\
{\
	time_t tt = time(nullptr);\
	tm* date = localtime(&tt);\
	sprintf(str, "%d-%d-%d", date->tm_year,date->tm_mon,date->tm_mday);\
}
#define trade_day(commodity) time_by_hhmm(commodity->TimeZone) >= commodity->OpenTime ? i_today(commodity->TimeZone) : i_yesterday(commodity->TimeZone)
}
#endif