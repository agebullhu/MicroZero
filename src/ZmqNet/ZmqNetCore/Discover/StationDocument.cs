using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class StationDocument : DocumentItem
    {
        /// <summary>
        ///     Api方法
        /// </summary>
        [DataMember, JsonProperty("aips", NullValueHandling = NullValueHandling.Ignore)]
        private Dictionary<string, ApiDocument> aips;

        /// <summary>
        ///     Api方法
        /// </summary>
        public Dictionary<string, ApiDocument> Aips => aips ?? (aips = new Dictionary<string, ApiDocument>(StringComparer.OrdinalIgnoreCase));
    }
}