/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2017/5/11 23:33:04*/
#pragma once
#ifndef _GBS_STRINGARGUMENT_H
#define _GBS_STRINGARGUMENT_H
#pragma unmanaged

#include <stdafx.h>

using namespace std;
using namespace Agebull::Tson;

namespace Agebull
{
    namespace Rpc
    {
        namespace Globals
        {

            
            // StringArgument类型代号
            const TYPE_INDEX TYPE_INDEX_STRINGARGUMENT = 0x120004;
            //〖文本的参数-参数〗字段索引
            const FIELD_INDEX FIELD_INDEX_STRINGARGUMENT_ARGUMENT = 1;

            struct StringArgument;

            /**
            * @brief StringArgument序列化到字节
            * @param {StringArgument*} field 文本的参数对象指针
            * @param {size_t} len 返回长度
            * @return 字节数组
            */
            char* Serialize(const StringArgument* field,size_t& len);
            
            /**
            * @brief StringArgument序列化到命令参数
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 命令参数
            */
            PNetCommand SerializeToCommand(const StringArgument* field);
            
            
            /**
            * @brief StringArgument序列化到命令参数
            * @param {PNetCommand} command 命令参数
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Serialize(PNetCommand command, const StringArgument* field);
            
            /**
            * @brief StringArgument序列化到字节
            * @param {char*} buffer 字节数组
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Serialize(char* buffer, size_t len, const StringArgument* field);
            
            
            /**
            * @brief StringArgument序列化到序列化器
            * @param {Serializer&} writer 序列化器
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Serialize(Serializer& writer, const StringArgument* field);
            
            /**
            * @brief 从网络命令参数反序列化到StringArgument
            * @param {PNetCommand} command 网络命令参数
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Deserialize(PNetCommand command, StringArgument* field);
            
            /**
            * @brief 从字节反序列化到StringArgument
            * @param {char*} buffer 字节数组指针
            * @param {size_t} len 字节数组长度
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Deserialize(char* buffer, size_t len, StringArgument* field);
            
            /**
            * @brief 从网络命令参数反序列化到StringArgument
            * @param {PNetCommand} command 网络命令参数
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Deserialize(PNetCommand command, StringArgument* field);
            
            /**
            * @brief 从反序列化器反序列化到StringArgument
            * @param {Deserializer&} reader 反序列化器
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Deserialize(Deserializer& reader, StringArgument* field);

            
            /**
            * @brief StringArgument的结构定义
            */
            struct StringArgument
            {
                //参数
                char  Argument[512];
                /**
                * @brief 从网络命令参数反序列化到〖文本的参数〗
                * @param {PNetCommand} command 网络命令参数
                * @return 文本的参数对象指针
                */
                inline StringArgument& operator = (const PNetCommand cmd)
            	{
                    Deserialize(cmd, this);
            		return *this;
            	}
            };
            //文本的参数的合理序列化长度
            const size_t TSON_BUFFER_LEN_STRINGARGUMENT = SERIALIZE_BASE_LEN + sizeof(StringArgument) + 5; 
            

            /**
            * @brief StringArgument序列化到命令参数
            * @param {StringArgument&} field 文本的参数对象
            * @param {PNetCommand} cmd 命令参数
            * @return 命令参数
            */
            inline void operator  << (PNetCommand& cmd, StringArgument& field)
            {
                cmd = SerializeToCommand(&field);
            }
            
            /**
            * @brief StringArgument 快速序列化到命令参数
            * @param {StringArgument&} field 文本的参数对象
            * @param {PNetCommand} cmd 命令参数
            * @return 命令参数
            */
            inline void operator >> (StringArgument& field, PNetCommand& cmd)
            {
                cmd = SerializeToCommand(&field);
            }
            
            /**
            * @brief 命令参数快速反序列化到StringArgument
            * @param {PNetCommand} command 命令参数
            * @param {StringArgument&} field 文本的参数对象
            * @return 无
            */
            inline void operator >> (PNetCommand cmd, StringArgument& field)
            {
                Deserialize(cmd, &field);
            }
            
            /**
            * @brief 从反序列化器反序列化到StringArgument
            * @param {PNetCommand} cmd 命令参数
            * @param {StringArgument&} field 文本的参数对象
            * @return 无
            */
            inline void operator << (StringArgument& field, PNetCommand cmd)
            {
                Deserialize(cmd, &field);
            }

			/**
			* @brief StringArgument保存到redis
			* @param {StringArgument} field 文本的参数
			* @return ID
			*/
			inline int save_to_redis(const StringArgument* field)
			{
				size_t len = 0;
				char* buffer = Serialize(field, len);
				int id = static_cast<int>(incr_redis("i:sarg:StringArgument"));
				write_to_redis(buffer, len, "e:sarg:StringArgument:%d", id);
				return id;
			}
        }
    }
}
#endif