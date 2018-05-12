namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Api调用节点
    /// </summary>
    public class ApiCallItem
    {
        /// <summary>
        /// 请求者
        /// </summary>
        public string Caller { get; set; }
        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestId { get; set; }
        /// <summary>
        /// 请求者
        /// </summary>
        public string Requester { get; set; }
        /// <summary>
        /// API名称
        /// </summary>
        public string ApiName { get; set; }
        /// <summary>
        ///  原始上下文的JSO内容
        /// </summary>
        public string ContextJson { get; set; }
        /// <summary>
        /// 请求参数
        /// </summary>
        public string Argument { get; set; }
        /// <summary>
        /// 返回
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public int Status { get; set; }

    }
}