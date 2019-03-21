using System.Collections.Generic;
using System.Runtime.Serialization;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace Xuhui.Internetpro.WzHealthCardService
{
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiArgument
    {
        /// <summary>
        ///     基础参数
        /// </summary>
        [JsonProperty("header")]
        public ApiArgumentHeader Header { get; set; }

        /// <summary>数据校验</summary>
        /// <param name="message">返回的消息</param>
        /// <returns>成功则返回真</returns>
        public virtual bool Validate(out string message)
        {
            if (Header == null)
                Header = new ApiArgumentHeader();
            var result = Header.Validate();
            message = result.succeed ? null : "参数错误";
            return true; //result.succeed;
        }
    }

    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiArgument<TData> : ApiArgument
        where TData : class, IApiArgument, new()
    {
        /// <summary>
        ///     扩展参数
        /// </summary>
        [JsonProperty("data")]
        public TData Data { get; set; }

        /// <summary>数据校验</summary>
        /// <param name="message">返回的消息</param>
        /// <returns>成功则返回真</returns>
        public override bool Validate(out string message)
        {
            if (!base.Validate(out message))
                return false;
            if (Data == null)
                Data = new TData();
            return Data.Validate(out message);
        }
    }

    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiValueArgument<TData> : ApiArgument
        where TData : struct
    {
        /// <summary>
        ///     扩展参数
        /// </summary>
        [JsonProperty("data")]
        public TData Data { get; set; }
    }

    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiArrayArgument<TData> : ApiArgument
        where TData : class, IApiArgument, new()
    {
        /// <summary>
        ///     扩展参数
        /// </summary>
        [JsonProperty("data")]
        public List<TData> Data { get; set; }

        /// <summary>数据校验</summary>
        /// <param name="message">返回的消息</param>
        /// <returns>成功则返回真</returns>
        public override bool Validate(out string message)
        {
            if (!base.Validate(out message))
                return false;
            if (Data == null)
                Data = new List<TData>();
            foreach (var data in Data)
                if (!data.Validate(out message))
                    return false;
            return true;
        }
    }
}
//#endif