/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2017/12/15 18:24:55*/
#pragma once
#ifndef _AGEBULL_RPC_MANAGER_MACHINEINFO_H
#define _AGEBULL_RPC_MANAGER_MACHINEINFO_H
#pragma unmanaged

#include <stdafx.h>

using namespace std;
using namespace Agebull::Tson;

namespace Agebull
{
    namespace Rpc
    {
        namespace Manager
        {

            
            // MachineInfo类型代号
            const TYPE_INDEX TYPE_INDEX_MACHINEINFO = 0x20001;
            //〖机器信息-服务名称〗字段索引
            const FIELD_INDEX FIELD_INDEX_MACHINEINFO_SERVICENAME = 1;
            //〖机器信息-服务标识〗字段索引
            const FIELD_INDEX FIELD_INDEX_MACHINEINFO_SERVICEKEY = 2;
            //〖机器信息-运行时名称〗字段索引
            const FIELD_INDEX FIELD_INDEX_MACHINEINFO_RUNTIMENAME = 3;
            //〖机器信息-注册时间〗字段索引
            const FIELD_INDEX FIELD_INDEX_MACHINEINFO_JOINDATE = 4;
            //〖机器信息-备注〗字段索引
            const FIELD_INDEX FIELD_INDEX_MACHINEINFO_MEMO = 5;

            struct MachineInfo;

            /**
            * @brief MachineInfo序列化到字节
            * @param {MachineInfo*} field 机器信息对象指针
            * @param {size_t} len 返回长度
            * @return 字节数组
            */
            char* Serialize(const MachineInfo* field,size_t& len);
            
            /**
            * @brief MachineInfo序列化到命令参数
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 命令参数
            */
            PNetCommand SerializeToCommand(const MachineInfo* field);
            
            
            /**
            * @brief MachineInfo序列化到命令参数
            * @param {PNetCommand} command 命令参数
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Serialize(PNetCommand command, const MachineInfo* field);
            
            /**
            * @brief MachineInfo序列化到字节
            * @param {char*} buffer 字节数组
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Serialize(char* buffer, size_t len, const MachineInfo* field);
            
            
            /**
            * @brief MachineInfo序列化到序列化器
            * @param {Serializer&} writer 序列化器
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Serialize(Serializer& writer, const MachineInfo* field);
            
            /**
            * @brief 从网络命令参数反序列化到MachineInfo
            * @param {PNetCommand} command 网络命令参数
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Deserialize(PNetCommand command, MachineInfo* field);
            
            /**
            * @brief 从字节反序列化到MachineInfo
            * @param {char*} buffer 字节数组指针
            * @param {size_t} len 字节数组长度
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Deserialize(char* buffer, size_t len, MachineInfo* field);
            
            /**
            * @brief 从网络命令参数反序列化到MachineInfo
            * @param {PNetCommand} command 网络命令参数
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Deserialize(PNetCommand command, MachineInfo* field);
            
            /**
            * @brief 从反序列化器反序列化到MachineInfo
            * @param {Deserializer&} reader 反序列化器
            * @param {MachineInfo*} field 机器信息对象指针
            * @return 无
            */
            void Deserialize(Deserializer& reader, MachineInfo* field);

            
            /**
            * @brief MachineInfo的结构定义
            */
            struct MachineInfo
            {
                //服务名称
                  ServiceName;
                //服务标识
                  ServiceKey;
                //运行时名称
                  RuntimeName;
                //注册时间
                  JoinDate;
                //备注
                  Memo;
                /**
                * @brief 从网络命令参数反序列化到〖机器信息〗
                * @param {PNetCommand} command 网络命令参数
                * @return 机器信息对象指针
                */
                inline MachineInfo& operator = (const PNetCommand cmd)
            	{
                    Deserialize(cmd, this);
            		return *this;
            	}
            };
            //机器信息的合理序列化长度
            const size_t TSON_BUFFER_LEN_MACHINEINFO = SERIALIZE_BASE_LEN + sizeof(MachineInfo) + 5; 
            

            /**
            * @brief MachineInfo序列化到命令参数
            * @param {MachineInfo&} field 机器信息对象
            * @param {PNetCommand} cmd 命令参数
            * @return 命令参数
            */
            inline void operator  << (PNetCommand& cmd, MachineInfo& field)
            {
                cmd = SerializeToCommand(&field);
            }
            
            /**
            * @brief MachineInfo 快速序列化到命令参数
            * @param {MachineInfo&} field 机器信息对象
            * @param {PNetCommand} cmd 命令参数
            * @return 命令参数
            */
            inline void operator >> (MachineInfo& field, PNetCommand& cmd)
            {
                cmd = SerializeToCommand(&field);
            }
            
            /**
            * @brief 命令参数快速反序列化到MachineInfo
            * @param {PNetCommand} command 命令参数
            * @param {MachineInfo&} field 机器信息对象
            * @return 无
            */
            inline void operator >> (PNetCommand cmd, MachineInfo& field)
            {
                Deserialize(cmd, &field);
            }
            
            /**
            * @brief 从反序列化器反序列化到MachineInfo
            * @param {PNetCommand} cmd 命令参数
            * @param {MachineInfo&} field 机器信息对象
            * @return 无
            */
            inline void operator << (MachineInfo& field, PNetCommand cmd)
            {
                Deserialize(cmd, &field);
            }

        }
    }
}
#endif