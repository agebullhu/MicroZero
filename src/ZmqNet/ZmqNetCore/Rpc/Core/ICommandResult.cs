using Newtonsoft.Json;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 命令的标准返回
    /// </summary>
    public interface ICommandResult
    {
        /// <summary>
        /// 状态，0表示成功，其它表示错误代码
        /// </summary>
        int Status { get; }

        /// <summary>
        /// 错误消息，如果有的话
        /// </summary>
        string Message { get; }
    }

    /// <summary>
    /// 基本命令返回
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CommandResult : ICommandResult
    {
        /// <summary>
        /// 成功标识
        /// </summary>
        public bool Success
        {
            get { return Status == 0; }
            set { Status = Success ? 0 : -1; }
        }
        /// <summary>
        /// 状态，0表示成功，其它表示错误代码,-1表示未知错误
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }
        /// <summary>
        /// 网络命令
        /// </summary>
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}