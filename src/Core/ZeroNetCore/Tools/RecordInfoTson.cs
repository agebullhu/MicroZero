using Agebull.Common.Logging;
using Agebull.Common.Tson;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>记录信息</summary>
    public class RecordInfoTson
    {
        enum MyIndex
        {
            /// <summary>本地日志</summary>
            Local = 1,
            /// <summary>时间</summary>
            Time,
            /// <summary>日志记录序号</summary>
            Index,
            /// <summary>线程ID</summary>
            ThreadID,
            /// <summary>日志ID</summary>
            RequestID,
            /// <summary>机器</summary>
            Machine,
            /// <summary>名称</summary>
            Name,
            /// <summary>格式化消息</summary>
            Message,
            /// <summary>日志类型</summary>
            Type,
            /// <summary>日志扩展名称,类型为None</summary>
            TypeName,
            /// <summary>当前用户</summary>
            User
        }

        public static void ToTson(ITsonSerializer serializer, RecordInfo info)
        {
            serializer.Write((byte)MyIndex.Local, info.Local);
            serializer.Write((byte)MyIndex.Time, info.Time);
            serializer.Write((byte)MyIndex.Index, info.Index);
            serializer.Write((byte)MyIndex.ThreadID, info.ThreadID);
            serializer.Write((byte)MyIndex.RequestID, info.RequestID);
            serializer.Write((byte)MyIndex.Name, info.Name);
            serializer.Write((byte)MyIndex.Machine, info.Machine);
            serializer.Write((byte)MyIndex.Message, info.Message);
            serializer.Write((byte)MyIndex.Type, (int)info.Type);
            serializer.Write((byte)MyIndex.TypeName, info.TypeName);
            serializer.Write((byte)MyIndex.User, info.User);
        }

        public static void FromTson(ITsonDeserializer serializer, RecordInfo info)
        {
            while (!serializer.IsEof)
            {
                switch ((MyIndex)serializer.ReadIndex())
                {
                    case MyIndex.Local:
                        info.Local = serializer.ReadBool();
                        break;
                    case MyIndex.Time:
                        info.Time = serializer.ReadDateTime();
                        break;
                    case MyIndex.Index:
                        info.Index = serializer.ReadULong();
                        break;
                    case MyIndex.ThreadID:
                        info.ThreadID = serializer.ReadInt();
                        break;
                    case MyIndex.RequestID:
                        info.RequestID = serializer.ReadString();
                        break;
                    case MyIndex.Name:
                        info.Name = serializer.ReadString();
                        break;
                    case MyIndex.Message:
                        info.Message = serializer.ReadString();
                        break;
                    case MyIndex.Type:
                        info.Type = (LogType)serializer.ReadInt();
                        break;
                    case MyIndex.TypeName:
                        info.TypeName = serializer.ReadString();
                        break;
                    case MyIndex.User:
                        info.User = serializer.ReadString();
                        break;
                    case MyIndex.Machine:
                        info.Machine = serializer.ReadString();
                        break;
                }
            }
        }
    }
}