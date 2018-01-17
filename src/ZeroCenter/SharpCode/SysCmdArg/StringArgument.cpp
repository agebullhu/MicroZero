/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2017/5/11 23:33:04*/
#include <stdafx.h>
#include "StringArgument.h"

using namespace std;
using namespace agebull::Tson;

#pragma unmanaged

namespace agebull
{
    namespace Rpc
    {
        namespace Globals
        {

            /**
            * @brief StringArgument序列化到字节
            * @param {StringArgument*} field 文本的参数对象指针
            * @param {size_t} len 返回长度
            * @return 字节数组
            */
            char* Serialize(const StringArgument* field,size_t& len)
            {
            	Serializer writer;
            	writer.CreateBuffer(TSON_BUFFER_LEN_STRINGARGUMENT, false);
            	Serialize(writer, field);
            	len = writer.GetDataLen();
            	return writer.GetBuffer();
            }
            
            
            /**
            * @brief StringArgument序列化到命令参数
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 命令参数
            */
            PNetCommand SerializeToCommand(const StringArgument* field)
            {
            	size_t len = TSON_BUFFER_LEN_STRINGARGUMENT + NETCOMMAND_HEAD_LEN;
            	char* buffer = new char[len];
            	memset(buffer, 0, NETCOMMAND_HEAD_LEN);
            	Serializer writer(get_cmd_buffer(buffer), static_cast<size_t>(len - NETCOMMAND_HEAD_LEN));
            	Serialize(writer, field);
            	return reinterpret_cast<PNetCommand>(buffer);
            }
            
            /**
            * @brief StringArgument序列化到命令参数
            * @param {PNetCommand} command 命令参数
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Serialize(PNetCommand command, const StringArgument* field)
            {
            	Serializer writer(get_cmd_buffer(command), static_cast<size_t>(get_cmd_len(command)));
            	Serialize(writer, field);
            }
            
            /**
            * @brief StringArgument序列化到字节
            * @param {char*} buffer 字节数组
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Serialize(char* buffer, size_t len, const StringArgument* field)
            {
            	Serializer writer(buffer, len);
            	Serialize(writer, field);
            }
            
            /**
            * @brief 从网络命令参数反序列化到StringArgument
            * @param {PNetCommand} command 网络命令参数
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Deserialize(PNetCommand command, StringArgument* field)
            {
            	Deserializer reader(get_cmd_buffer(command), static_cast<size_t>(get_cmd_len(command)));
            	Deserialize(reader, field);
            }
            
            /**
            * @brief 从字节反序列化到StringArgument
            * @param {char*} buffer 字节数组指针
            * @param {size_t} len 字节数组长度
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Deserialize(char* buffer, size_t len, StringArgument* field)
            {
            	Deserializer reader(buffer, len);
            	Deserialize(reader, field);
            }
            
            
            /**
            * @brief StringArgument序列化到序列化器
            * @param {Serializer&} writer 序列化器
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Serialize(Serializer& writer,const StringArgument* field)
            {
            	writer.Begin(TYPE_INDEX_STRINGARGUMENT, static_cast<char>(1));
                if(field == nullptr)
                {
                    writer.End();
                    return;
                }
                if(!writer.str_is_empty(field->Argument))//参数
                {
                    writer.WriteIndex(FIELD_INDEX_STRINGARGUMENT_ARGUMENT);
                    writer.WriteStr(field->Argument);
                }
                writer.End();
            }
            
            
            /**
            * @brief 从反序列化器反序列化到StringArgument
            * @param {Deserializer&} reader 反序列化器
            * @param {StringArgument*} field 文本的参数对象指针
            * @return 无
            */
            void Deserialize(Deserializer& reader, StringArgument* field)
            {
                memset(field, 0 , sizeof(StringArgument));
                reader.Begin();	
            	while(!reader.IsEof())
            	{
            		FIELD_INDEX idx = reader.ReadByte();
                    //OBJ_TYPE type = reader.ReadByte();
            		switch(idx)
            		{
                    case FIELD_INDEX_STRINGARGUMENT_ARGUMENT://参数
                    {
                        reader.ReadStr(field->Argument);
                        break;
                    }
                    }
                }
                reader.End();
            }

        }
    }
}