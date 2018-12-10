using System;
using System.Runtime.Serialization;
using Agebull.Common.ApiDocuments;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>Api方法的信息</summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiActionInfoEx : ApiActionInfo
    {
        /// <summary>有参方法</summary>
        public Func<object, object> ArgumentAction2;
        /// <summary>是否有调用参数</summary>
        public int ArgumentType;
    }
}