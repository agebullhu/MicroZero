#pragma once
#include "acl/acl_cpp/acl_cpp_define.hpp"
#include "acl/acl_cpp/stream/ostream.hpp"

namespace acl {

/**
 * 标准输出流，该类对象仅能进行写操作
 */

class ACL_CPP_API stdout_stream : public ostream
{
public:
	stdout_stream();
	~stdout_stream();
};

} // namespace acl
