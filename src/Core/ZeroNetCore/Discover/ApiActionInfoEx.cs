using System;
using System.Runtime.Serialization;
using Agebull.MicroZero.ApiDocuments;
using Newtonsoft.Json;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>Api方法的信息</summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiActionInfoEx : ApiActionInfo
    {
        /// <summary>有参方法</summary>
        public Func<object, object> ArgumentAction2;

        /// <summary>有参方法</summary>
        public Func<object> Action2;

        /// <summary>有参方法</summary>
        public Func<string> Action3;

        /// <summary>有参方法</summary>
        public Func<object, string> Action4;

        /// <summary>是否有调用参数</summary>
        public int ArgumentType;

        /// <summary>是否有调用参数</summary>
        public Type ResultType;
    }
}