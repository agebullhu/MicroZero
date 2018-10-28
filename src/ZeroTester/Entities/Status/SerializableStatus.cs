using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.Common.DataModel
{
    /// <summary>
    /// 序列化状态
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class SerializableStatus : ObjectStatusBase
    {

        /// <summary>
        ///     表示网路对象来源
        /// </summary>
        [DataMember(Name = "ObjectSource"), ReadOnly(true), DisplayName("对象来源"), Category("运行时")]
        public NetObjectSource ObjectSource
        {
            get;
            set;
        }

        /// <summary>
        ///     序列化中
        /// </summary>
        [IgnoreDataMember, ReadOnly(true), DisplayName("序列化中"), Category("运行时")]
        public bool IsSerializing
        {
            get;
            internal set;
        }

        /// <summary>
        ///     反序列化中
        /// </summary>
        [IgnoreDataMember, ReadOnly(true), DisplayName("反序列化中"), Category("运行时")]
        public bool IsDeserializing
        {
            get;
            internal set;
        }
    }
}