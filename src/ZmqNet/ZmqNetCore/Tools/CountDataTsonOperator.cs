using Agebull.ZeroNet.ZeroApi;

namespace Agebull.Common.Tson
{
    public class CountDataTsonOperator: ITsonOperator<CountData>
    {
        private const byte Index_Machine = 1;
        private const byte Index_Station = 2;
        private const byte Index_User = 3;
        private const byte Index_RequestId = 4;
        private const byte Index_ToId = 5;
        private const byte Index_FromId = 6;
        private const byte Index_Requester = 7;
        private const byte Index_HostName = 8;
        private const byte Index_ApiName = 9;
        private const byte Index_Status = 10;
        private const byte Index_IsInner = 11;
        private const byte Index_Title = 12;
        private const byte Index_Start = 13;
        private const byte Index_End = 14;
        
        /// <summary>
        /// 序列化
        /// </summary>
        public void ToTson(ITsonSerializer serializer, CountData data)
        {
            serializer.Write(Index_Machine, data.Machine);
            serializer.Write(Index_Station, data.Station);
            serializer.Write(Index_User, data.User);
            serializer.Write(Index_RequestId, data.RequestId);
            serializer.Write(Index_IsInner, data.IsInner);
            serializer.Write(Index_Title, data.Title);
            serializer.Write(Index_Start, data.Start);
            serializer.Write(Index_End, data.End);
            serializer.Write(Index_ToId, data.ToId);
            serializer.Write(Index_FromId, data.FromId);
            serializer.Write(Index_Requester, data.Requester);
            serializer.Write(Index_HostName, data.HostName);
            serializer.Write(Index_ApiName, data.ApiName);
            serializer.Write(Index_Status, (int)data.Status);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public void FromTson(ITsonDeserializer serializer, CountData data)
        {
            while (!serializer.IsEof)
            {
                int idx = serializer.ReadIndex();
                switch (idx)
                {
                    case Index_IsInner:
                        data.IsInner = serializer.ReadBool();
                        break;
                    case Index_Title:
                        data.Title = serializer.ReadString();
                        break;
                    case Index_Start:
                        data.Start = serializer.ReadLong();
                        break;
                    case Index_End:
                        data.End = serializer.ReadLong();
                        break;
                    case Index_ToId:
                        data.ToId = serializer.ReadString();
                        break;
                    case Index_FromId:
                        data.FromId = serializer.ReadString();
                        break;
                    case Index_Requester:
                        data.Requester = serializer.ReadString();
                        break;
                    case Index_HostName:
                        data.HostName = serializer.ReadString();
                        break;
                    case Index_ApiName:
                        data.ApiName = serializer.ReadString();
                        break;
                    case Index_Status:
                        data.Status = (OperatorStatus)serializer.ReadInt();
                        break;
                    case Index_Machine:
                        data.Machine = serializer.ReadString();
                        break;
                    case Index_Station:
                        data.Station = serializer.ReadString();
                        break;
                    case Index_User:
                        data.User = serializer.ReadString();
                        break;
                    case Index_RequestId:
                        data.RequestId = serializer.ReadString();
                        break;
                }
            }
        }
    }
}