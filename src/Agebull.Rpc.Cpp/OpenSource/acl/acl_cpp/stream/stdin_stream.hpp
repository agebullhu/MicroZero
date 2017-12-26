#pragma once
#include "acl/acl_cpp/acl_cpp_define.hpp"
#include "acl/acl_cpp/stream/istream.hpp"

namespace acl {

/**
 * 标准输入流，该类对象仅能进行读操作
 */

class ACL_CPP_API stdin_stream : public istream
{
public:
	stdin_stream();
	~stdin_stream();
};

} // namespace acl
