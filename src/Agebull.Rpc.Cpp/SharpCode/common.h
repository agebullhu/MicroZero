#pragma once
#ifndef AGEBULL_COMMON_H
#define  AGEBULL_COMMON_H

/**
* @brief º¸÷µ∂‘¿‡
*/
template<class TKey, class TValue>
struct KeyValue
{
	TKey key;
	TValue value;
	KeyValue(TKey k, TValue v)
		: key(k)
		, value(v)
	{

	}
	KeyValue()
	{

	}
};


#endif