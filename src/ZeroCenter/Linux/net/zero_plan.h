#pragma once
#ifndef _ZERO_PLAN_H_
#define _ZERO_PLAN_H_
#include "../stdinc.h"
#include "../sharp_char.h"
#include "../cfg/config.h"

namespace agebull
{
	namespace zmq_net
	{

		/**
		* \brief 计划类型
		*/
		enum class plan_date_type
		{
			/**
			* \brief 无计划，立即发送
			*/
			none,
			/**
			* \brief 在指定的时间发送
			*/
			time,
			/**
			* \brief 分钟间隔后发送
			*/
			minute,
			/**
			* \brief 小时间隔后发送
			*/
			hour,
			/**
			* \brief 日间隔后发送
			*/
			day,
			/**
			* \brief 周间隔后发送
			*/
			week,
			/**
			* \brief 月间隔后发送
			*/
			month,
			/**
			* \brief 年间隔后发送
			*/
			year
		};
		/**
		* \brief 消息
		*/
		struct plan_message
		{
			/**
			* \brief 消息标识
			*/
			uint32_t plan_id;

			/**
			* \brief 计划类型
			*/
			plan_date_type plan_type;

			/**
			* \brief 类型值
			*/
			int plan_value;

			/**
			* \brief 重复次数,0不重复 >0重复次数,-1永久重复
			*/
			int plan_repet;

			/**
			* \brief 执行次数
			*/
			int real_repet;

			/**
			* \brief 发起者提供的标识
			*/
			string request_id;

			/**
			* \brief 发起者
			*/
			string request_caller;
			/**
			* \brief 消息描述
			*/
			sharp_char messages_description;

			/**
			* \brief 消息内容
			*/
			vector<sharp_char> messages;

			/**
			* \brief 从JSON中反序列化
			*/
			void read_plan(const char* plan)
			{
				acl::json json;
				json.update(plan);
				acl::json_node* iter = json.first_node();
				while (iter)
				{
					int idx = strmatchi(5, iter->tag_name(), "plan_type", "plan_value", "plan_repet");
					switch (idx)
					{
					case 0:
						plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
						break;
					case 1:
						plan_value = static_cast<int>(*iter->get_int64());
						break;
					case 2:
						plan_repet = static_cast<int>(*iter->get_int64());
						break;
					default: break;
					}
					iter = json.next_node();
				}
			}
			void read_json(acl::string& val)
			{
				acl::json json;
				json.update(val);
				acl::json_node* iter = json.first_node();
				while (iter)
				{
					const int idx = strmatchi(10, iter->tag_name(), "plan_id", "plan_type", "plan_value", "plan_repet", "real_repet", "request_caller", "request_id", "messages_description", "messages");
					switch (idx)
					{
					case 0:
						plan_id = static_cast<int>(*iter->get_int64());
						break;
					case 1:
						plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
						break;
					case 2:
						plan_value = static_cast<int>(*iter->get_int64());
						break;
					case 3:
						plan_repet = static_cast<int>(*iter->get_int64());
						break;
					case 4:
						real_repet = static_cast<int>(*iter->get_int64());
						break;
					case 5:
						request_caller = iter->get_string();
						break;
					case 6:
						request_id = iter->get_string();
						break;
					case 7:
						messages_description = iter->get_string();
						break;
					case 8:
					{
						acl::json arr = iter->get_json();
						acl::json_node* iter_arr = arr.first_node();
						while (iter_arr)
						{
							messages.emplace_back(iter_arr->get_string());
							iter_arr = arr.next_node();
						}
					}
					break;
					default: break;
					}
					iter = json.next_node();
				}
			}
			acl::string write_json()
			{
				acl::json json;
				acl::json_node& node = json.create_node();
				node.add_number("plan_id", plan_id);
				if (plan_type > plan_date_type::none)
				{
					node.add_number("plan_type", static_cast<int>(plan_type));
					node.add_number("plan_value", plan_value);
					node.add_number("plan_repet", plan_repet);
					node.add_number("real_repet", real_repet);
				}
				node.add_text("request_id", request_id.c_str());
				node.add_text("request_caller", request_caller.c_str());
				node.add_text("messages_description", messages_description.get_buffer());
				acl::json_node& array = node.add_array(true);
				array.set_tag("messages");
				for (const auto& line : messages)
				{
					array.add_array_text(*line);
				}
				return node.to_string();
			}
		};
	}
}
#endif //!_ZERO_PLAN_H_