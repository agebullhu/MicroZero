/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2017/12/15 18:24:55*/
#include <stdafx.h>
#include "MachineInfo.h"

using namespace std;
using namespace Agebull::Tson;

#pragma unmanaged

namespace Agebull
{
    namespace Rpc
    {
        namespace Manager
        {

            /**
            * @brief MachineInfo序列化到字节
            * @param {MachineInfo*} field 机器信息对象指针
            * @param {size_t} len 返回长度
            * @return 字节数组
            */
            char* Serialize(const MachineInfo* field,size_t& len)
            {
            	Serializer writer;
            	writer.CreateBuffer(TSON_BUFFER_LEN_MACHINEINFO, false);
            	Serialize(writer, field);
            	len = writer.GetDataLen();
            	return writer.GetBuffer();
            }
            
            
            /**
            * @brief MachineInfo序列化到命令参数
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 命令参数
            */
            PNetCommand SerializeToCommand(const MachineInfo* field)
            {
            	size_t len = TSON_BUFFER_LEN_MACHINEINFO + NETCOMMAND_HEAD_LEN;
            	char* buffer = new char[len];
            	memset(buffer, 0, NETCOMMAND_HEAD_LEN);
            	Serializer writer(get_cmd_buffer(buffer), static_cast<size_t>(len - NETCOMMAND_HEAD_LEN));
            	Serialize(writer, field);
            	return reinterpret_cast<PNetCommand>(buffer);
            }
            
            /**
            * @brief MachineInfo序列化到命令参数
            * @param {PNetCommand} command 命令参数
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Serialize(PNetCommand command, const MachineInfo* field)
            {
            	Serializer writer(get_cmd_buffer(command), static_cast<size_t>(get_cmd_len(command)));
            	Serialize(writer, field);
            }
            
            /**
            * @brief MachineInfo序列化到字节
            * @param {char*} buffer 字节数组
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Serialize(char* buffer, size_t len, const MachineInfo* field)
            {
            	Serializer writer(buffer, len);
            	Serialize(writer, field);
            }
            
            /**
            * @brief 从网络命令参数反序列化到MachineInfo
            * @param {PNetCommand} command 网络命令参数
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Deserialize(PNetCommand command, MachineInfo* field)
            {
            	Deserializer reader(get_cmd_buffer(command), static_cast<size_t>(get_cmd_len(command)));
            	Deserialize(reader, field);
            }
            
            /**
            * @brief 从字节反序列化到MachineInfo
            * @param {char*} buffer 字节数组指针
            * @param {size_t} len 字节数组长度
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Deserialize(char* buffer, size_t len, MachineInfo* field)
            {
            	Deserializer reader(buffer, len);
            	Deserialize(reader, field);
            }
            
            
            /**
            * @brief MachineInfo序列化到序列化器
            * @param {Serializer&} writer 序列化器
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Serialize(Serializer& writer,const MachineInfo* field)
            {
            	writer.Begin(TYPE_INDEX_MACHINEINFO, static_cast<char>(1));
                if(field == nullptr)
                {
                    writer.End();
                    return;
                }
                if(!writer.is_empty(field->ServiceName))//服务名称
                {
                    writer.WriteIndex(FIELD_INDEX_MACHINEINFO_SERVICENAME);
                    writer.Write(field->ServiceName);
                }
                if(!writer.is_empty(field->ServiceKey))//服务标识
                {
                    writer.WriteIndex(FIELD_INDEX_MACHINEINFO_SERVICEKEY);
                    writer.Write(field->ServiceKey);
                }
                if(!writer.is_empty(field->RuntimeName))//运行时名称
                {
                    writer.WriteIndex(FIELD_INDEX_MACHINEINFO_RUNTIMENAME);
                    writer.Write(field->RuntimeName);
                }
                if(!writer.is_empty(field->JoinDate))//注册时间
                {
                    writer.WriteIndex(FIELD_INDEX_MACHINEINFO_JOINDATE);
                    writer.Write(field->JoinDate);
                }
                if(!writer.is_empty(field->Memo))//备注
                {
                    writer.WriteIndex(FIELD_INDEX_MACHINEINFO_MEMO);
                    writer.Write(field->Memo);
                }
                writer.End();
            }
            
            
            /**
            * @brief 从反序列化器反序列化到MachineInfo
            * @param {Deserializer&} reader 反序列化器
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Deserialize(Deserializer& reader, MachineInfo* field)
            {
                memset(field, 0 , sizeof(MachineInfo));
                reader.Begin();	
            	while(!reader.IsEof())
            	{
            		FIELD_INDEX idx = reader.ReadByte();
                    //OBJ_TYPE type = reader.ReadByte();
            		switch(idx)
            		{
                    case FIELD_INDEX_MACHINEINFO_SERVICENAME://服务名称
                    {
                        reader.Read(field->ServiceName);
                        break;
                    }
                    case FIELD_INDEX_MACHINEINFO_SERVICEKEY://服务标识
                    {
                        reader.Read(field->ServiceKey);
                        break;
                    }
                    case FIELD_INDEX_MACHINEINFO_RUNTIMENAME://运行时名称
                    {
                        reader.Read(field->RuntimeName);
                        break;
                    }
                    case FIELD_INDEX_MACHINEINFO_JOINDATE://注册时间
                    {
                        reader.Read(field->JoinDate);
                        break;
                    }
                    case FIELD_INDEX_MACHINEINFO_MEMO://备注
                    {
                        reader.Read(field->Memo);
                        break;
                    }
                    }
                }
                reader.End();
            }

        }
    }
}