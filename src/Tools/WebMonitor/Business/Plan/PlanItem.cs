using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     计划广播节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PlanItem : PublishItem
    {
        /// <summary>
        /// 调用返回值
        /// </summary>
        public string CallResultJson { get; set; }

        /// <summary>
        /// 原始请求者
        /// </summary>
        public string Requester { get; set; }
        /// <summary>
        /// 计划
        /// </summary>
        public ZeroPlan Plan { get; set; }
    }
}