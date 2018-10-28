using System.Collections.Generic;

namespace Agebull.ZeroNet.ZeroApi
{

    /// <summary>
    /// Api处理器
    /// </summary>
    public interface IApiHandler
    {
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="station"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        void Prepare(ApiStation station, ApiCallItem item);

        /// <summary>
        /// 结束处理
        /// </summary>
        /// <param name="station"></param>
        /// <param name="item"></param>
        void End(ApiStation station, ApiCallItem item);
    }

    /// <summary>
    /// Api调用节点
    /// </summary>
    public class ApiCallItem
    {
        /// <summary>
        /// 全局ID
        /// </summary>
        internal List<IApiHandler> Handlers { get; set; }
        /// <summary>
        /// 全局ID
        /// </summary>
        public string GlobalId { get; set; }

        /// <summary>
        /// 调用方的全局ID
        /// </summary>
        public string CallerGlobalId { get; set; }

        /// <summary>
        /// 请求者
        /// </summary>
        public byte[] Caller { get; set; }
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
        /// 请求参数字典
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 返回
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public ZeroOperatorStatus Status { get; set; }

    }
}