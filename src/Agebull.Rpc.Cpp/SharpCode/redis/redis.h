#ifndef AGEBULL_REDIS_H
#define AGEBULL_REDIS_H
#pragma once
#ifndef CLIENT
#include <stdinc.h>
#include "TransRedis.h"
using namespace std;

inline acl::string read_str_from_redis(const char* key)
{
	return TransRedis::get_context().read_str_from_redis(key);
}
template<class TArg1>
inline acl::string read_str_from_redis(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	return read_str_from_redis(key);
}
template<class TArg1, class TArg2>
inline acl::string read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	return read_str_from_redis(key);
}
template<class TArg1, class TArg2, class TArg3>
inline acl::string read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	return read_str_from_redis(key);
}

inline std::vector<acl::string> find_redis_keys(const char* key)
{
	return TransRedis::get_context().find_redis_keys(key);
}
template<class TArg1>
inline std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	return find_redis_keys(key);
}
template<class TArg1, class TArg2>
inline std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	return find_redis_keys(key);
}
template<class TArg1, class TArg2, class TArg3>
inline std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	return find_redis_keys(key);
}

inline std::vector<acl::string*> find_from_redis(const char* key)
{
	return TransRedis::get_context().find_from_redis(key);
}
template<class TArg1>
inline std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	return find_from_redis(key);
}
template<class TArg1, class TArg2>
inline std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	return find_from_redis(key);
}
template<class TArg1, class TArg2, class TArg3>
inline std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	return find_from_redis(key);
}
inline acl::string read_from_redis(const char* key)
{
	return TransRedis::get_context().read_from_redis(key);
}
template<class TArg1>
inline acl::string read_from_redis(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	return read_from_redis(key);
}
template<class TArg1, class TArg2>
inline acl::string read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	return read_from_redis(key);
}
template<class TArg1, class TArg2, class TArg3>
inline acl::string read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	return read_from_redis(key);
}

inline void delete_from_redis(const char* key)
{
	return TransRedis::get_context().delete_from_redis(key);
}

template<class TArg1>
inline void delete_from_redis(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	delete_from_redis(key);
}
template<class TArg1, class TArg2>
inline void delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	delete_from_redis(key);
}
template<class TArg1, class TArg2, class TArg3>
inline void delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	delete_from_redis(key);
}

inline size_t incr_redis(const char* key)
{
	return TransRedis::get_context().incr_redis(key);
}
template<class TArg1>
inline size_t incr_redis(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	return incr_redis(key);
}
template<class TArg1, class TArg2>
inline size_t incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	return incr_redis(key);
}
template<class TArg1, class TArg2, class TArg3>
inline size_t incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	return incr_redis(key);
}
inline void write_to_redis(const char* key, const char* bin, size_t len)
{
	return TransRedis::get_context().write_to_redis(key, bin, len);
}
inline void write_json_to_redis(const char* key, const char* json)
{
	return TransRedis::get_context().write_json_to_redis(key, json);
}
template<class TArg1>
inline void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	write_to_redis(key, bin, len);
}
template<class TArg1, class TArg2>
inline void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	write_to_redis(key, bin, len);
}
template<class TArg1, class TArg2, class TArg3>
inline void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	write_to_redis(key, bin, len);
}


inline acl::string read_first_from_redis(const char* key)
{
	return TransRedis::get_context().read_first_from_redis(key);
}
template<class TArg1>
inline acl::string read_first_from_redis(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	return read_first_from_redis(key);
}
template<class TArg1, class TArg2>
inline acl::string read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	return read_first_from_redis(key);
}
template<class TArg1, class TArg2, class TArg3>
inline acl::string read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	return read_first_from_redis(key);
}

inline int append_redis(const char* key,const char* value)
{
	return TransRedis::get_context()->append(key, value);
}


inline bool unlock_from_redis(const char* key)
{
	return TransRedis::get_context().unlock_from_redis(key);
}
template<class TArg1>
inline bool unlock_from_redis(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	return unlock_from_redis(key);
}
template<class TArg1, class TArg2>
inline bool unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	return unlock_from_redis(key);
}
template<class TArg1, class TArg2, class TArg3>
inline bool unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	return unlock_from_redis(key);
}

inline bool lock_from_redis(const char* key)
{
	return TransRedis::get_context().lock_from_redis(key);
}
template<class TArg1>
inline bool lock_from_redis(const char* key_fmt, TArg1 arg)
{
	char key[256];
	sprintf_s(key, key_fmt, arg);
	return lock_from_redis(key);
}
template<class TArg1, class TArg2>
inline bool lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2);
	return lock_from_redis(key);
}
template<class TArg1, class TArg2, class TArg3>
inline bool lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
{
	char key[256];
	sprintf_s(key, key_fmt, arg1, arg2, arg3);
	return lock_from_redis(key);
}
#endif
#endif