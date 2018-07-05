#ifndef _LOG_MYLOGGER_H
#define _LOG_MYLOGGER_H
#pragma once
#include "../stdinc.h"
using namespace std;
namespace agebull
{
#define DEBUG_BASE			100
#define DEBUG_TIMER		    (DEBUG_BASE + 1)
#define DEBUG_CALL			(DEBUG_BASE + 2)
#define DEBUG_RESULT		(DEBUG_BASE + 3)
#define DEBUG_PUB			(DEBUG_BASE + 4)
#define DEBUG_SUB			(DEBUG_BASE + 5)
#define DEBUG_VOTE			(DEBUG_BASE + 6)
#define DEBUG_CONFIG		("100:6; 101:6; 102:6; 103:6; 104:6; 106:6; 106:6")

	/**
	* \bref 初始化日志
	*/
	acl::string log_init();

	void log_acl_msg(const char* msg);
	void log_acl_warn(const char* fname, int line, const char* func, const char* msg);
	void log_acl_error(const char* fname, int line, const char* func, const char* msg);
	void log_acl_fatal(const char* fname, int line, const char* func, const char* msg);
	void log_acl_debug(int section, int  level, const char* fname, int line, const char* func, const char* msg);
	void log_acl_trace(int section, int  level, const char* msg);

	/**
	* \bref 输出到DEBUG窗口
	*/
	void out_debug(string msg);


#ifndef LOG_SPRINTF_FORMAT

#define log_warn(msg)  \
	log_acl_warn__FILE__, __LINE__, __FUNCTION__, msg)

#define log_fatal(msg)  \
	log_acl_fatal__FILE__, __LINE__, __FUNCTION__,msg)


#define log_msg( msg) \
	log_acl_msg(msg)
#define log_msg1(msg,arg) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg);\
		log_acl_msg(_msg_);\
	}
#define log_msg2(msg,arg1,arg2) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2);\
		log_acl_msg(_msg_);\
	}
#define log_msg3(msg,arg1,arg2,arg3) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3);\
		log_acl_msg(_msg_);\
	}
#define log_msg4(msg,arg1,arg2,arg3,arg4) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4);\
		log_acl_msg(_msg_);\
	}
#define log_msg5(msg,arg1,arg2,arg3,arg4,arg5) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5);\
		log_acl_msg(_msg_);\
	}
#define log_msg6(msg,arg1,arg2,arg3,arg4,arg5,arg6) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6);\
		log_acl_msg(_msg_);\
	}
#define log_msg7(msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6 , arg7);\
		log_acl_msg(_msg_);\
	}

#define log_error( msg) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__, msg)
#define log_error1(msg,arg) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg);\
		log_acl_error(__FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_error2(msg,arg1,arg2) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2);\
		log_acl_error(__FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_error3(msg,arg1,arg2,arg3) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3);\
		log_acl_error(__FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_error4(msg,arg1,arg2,arg3,arg4) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4);\
		log_acl_error(__FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_error5(msg,arg1,arg2,arg3,arg4,arg5) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5);\
		log_acl_error(__FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_error6(msg,arg1,arg2,arg3,arg4,arg5,arg6) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6);\
		log_acl_error(__FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_error7(msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6 , arg7);\
		log_acl_error(__FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_error8(msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6 , arg7,arg8);\
		log_acl_error(__FILE__, __LINE__, __FUNCTION__, _msg_);\
	}

#define log_debug(section, level, msg) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, msg)
#define log_debug1(section, level,msg,arg) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg);\
		log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_debug2(section, level,msg,arg1,arg2) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2);\
		log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_debug3(section, level,msg,arg1,arg2,arg3) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3);\
		log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_debug4(section, level,msg,arg1,arg2,arg3,arg4) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4);\
		log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_debug5(section, level,msg,arg1,arg2,arg3,arg4,arg5) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5);\
		log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, _msg_);\
	}
#define log_debug6(section, level,msg,arg1,arg2,arg3,arg4,arg5,arg6) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6);\
		log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, _msg_)\
	}
#define log_debug7(section, level,msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6 , arg7);\
		log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, _msg_);\
	}

#define log_trace(section, level, msg) \
	log_acl_trace(section, level, msg)
#define log_trace1(section, level,msg,arg) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg);\
		log_acl_trace(section, level, _msg_);\
	}
#define log_trace2(section, level,msg,arg1,arg2) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2);\
		log_acl_trace(section, level, _msg_);\
	}
#define log_trace3(section, level,msg,arg1,arg2,arg3) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3);\
		log_acl_trace(section, level, _msg_);\
	}
#define log_trace4(section, level,msg,arg1,arg2,arg3,arg4) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4);\
		log_acl_trace(section, level, _msg_);\
	}
#define log_trace5(section, level,msg,arg1,arg2,arg3,arg4,arg5) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5);\
		log_acl_trace(section, level, _msg_);\
	}
#define log_trace6(section, level,msg,arg1,arg2,arg3,arg4,arg5,arg6) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6);\
		log_acl_trace(section, level, _msg_)\
	}
#define log_trace7(section, level,msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7) \
	{\
		char _msg_[4096];\
		sprintf(_msg_,msg,arg1 , arg2 , arg3 , arg4 , arg5 , arg6 , arg7);\
		log_acl_trace(section, level, _msg_);\
	}
#else

#define log_warn(msg)  \
	log_acl_warn(__FILE__, __LINE__, __FUNCTION__, msg)

#define log_fatal(msg)  \
	log_acl_fatal(__FILE__, __LINE__, __FUNCTION__,msg)

#define log_msg(msg) \
	log_acl_msg(msg)
#define log_msg1(msg,arg) \
	log_acl_msg((boost::format(msg) % arg).str())
#define log_msg2(msg,arg1,arg2) \
	log_acl_msg((boost::format(msg) % arg1 % arg2).str())
#define log_msg3(msg,arg1,arg2,arg3) \
	log_acl_msg((boost::format(msg) % arg1 % arg2 % arg3).str())
#define log_msg4(msg,arg1,arg2,arg3,arg4) \
	log_acl_msg((boost::format(msg) % arg1 % arg2 % arg3 % arg4).str())
#define log_msg5(msg,arg1,arg2,arg3,arg4,arg5) \
	log_acl_msg((boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5).str())
#define log_msg6(msg,arg1,arg2,arg3,arg4,arg5,arg6) \
	log_acl_msg((boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5 % arg6).str())
#define log_msg7(msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7) \
	log_acl_msg((boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5 % arg6 % arg7).str())

#define log_error(msg) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__,  msg)
#define log_error1(msg,arg) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg).str())
#define log_error2(msg,arg1,arg2) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2).str())
#define log_error3(msg,arg1,arg2,arg3) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3).str())
#define log_error4(msg,arg1,arg2,arg3,arg4) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3 % arg4).str())
#define log_error5(msg,arg1,arg2,arg3,arg4,arg5) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5).str())
#define log_error6(msg,arg1,arg2,arg3,arg4,arg5,arg6) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5 % arg6).str())
#define log_error7(msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7) \
	log_acl_error(__FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5 % arg6 % arg7).str())

#define log_debug(section, level, msg) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, msg)
#define log_debug1(section, level,msg,arg) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg).str())
#define log_debug2(section, level,msg,arg1,arg2) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2).str())
#define log_debug3(section, level,msg,arg1,arg2,arg3) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3).str())
#define log_debug4(section, level,msg,arg1,arg2,arg3,arg4) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3 % arg4).str())
#define log_debug5(section, level,msg,arg1,arg2,arg3,arg4,arg5) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5).str())
#define log_debug6(section, level,msg,arg1,arg2,arg3,arg4,arg5,arg6) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5 % arg6).str())
#define log_debug7(section, level,msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7) \
	log_acl_debug(section, level, __FILE__, __LINE__, __FUNCTION__, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5 % arg6 % arg7).str())


#define log_trace(section, level, msg) \
	log_acl_trace(section, level, msg)
#define log_trace1(section, level,msg,arg) \
	log_acl_trace(section, level, (boost::format(msg) % arg).str())
#define log_trace2(section, level,msg,arg1,arg2) \
	log_acl_trace(section, level, (boost::format(msg) % arg1 % arg2).str())
#define log_trace3(section, level,msg,arg1,arg2,arg3) \
	log_acl_trace(section, level, (boost::format(msg) % arg1 % arg2 % arg3).str())
#define log_trace4(section, level,msg,arg1,arg2,arg3,arg4) \
	log_acl_trace(section, level, (boost::format(msg) % arg1 % arg2 % arg3 % arg4).str())
#define log_trace5(section, level,msg,arg1,arg2,arg3,arg4,arg5) \
	log_acl_trace(section, level, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5).str())
#define log_trace6(section, level,msg,arg1,arg2,arg3,arg4,arg5,arg6) \
	log_acl_trace(section, level, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5 % arg6).str())
#define log_trace7(section, level,msg,arg1,arg2,arg3,arg4,arg5,arg6,arg7) \
	log_acl_trace(section, level, (boost::format(msg) % arg1 % arg2 % arg3 % arg4 % arg5 % arg6 % arg7).str())

#endif

#endif
}