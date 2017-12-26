#pragma once
#include <stdafx.h>
#include "debug/TraceStack.h"

//…æ≥˝√¸¡Ó
inline void delete_cmd_arg(PNetCommand cmd_arg)
{
	RedisDbScope scope(REDIS_DB_COMMAND);
	delete_from_redis(command_key_fmt, cmd_arg->cmd_identity);
}
//±£¥Ê√¸¡Ó
inline void save_cmd_arg(PNetCommand cmd_arg)
{
	RedisDbScope scope(REDIS_DB_COMMAND);
	write_to_redis(reinterpret_cast<char*>(cmd_arg), get_cmd_len(cmd_arg), command_key_fmt, cmd_arg->cmd_identity);
}
//∂¡»°√¸¡Ó
inline NetCommandArgPtr load_cmd_arg(const char* cmd_identity)
{
	acl::string vl;
	{
		RedisDbScope scope(REDIS_DB_COMMAND);
		vl = read_from_redis(command_key_fmt, cmd_identity);
	}
	if (vl.empty())
	{
		log_error2("√¸¡Óªπ‘≠“Ï≥£(%s)\r\n%s", cmd_identity, get_call_stack());
		return NetCommandArgPtr();
	}
	char* buf = static_cast<char*>(vl.buf());
	char* end = static_cast<char*>(vl.buf_end());
	NetCommandArgPtr ptr(end - buf);
	memcpy(ptr.m_buffer, buf, end - buf);
	return ptr;
}
