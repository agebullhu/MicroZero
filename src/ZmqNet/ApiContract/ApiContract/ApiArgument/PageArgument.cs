using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 分页请求的参数
    /// </summary>
    public class PageArgument : IApiArgument
    {
        /// <summary>
        /// 页号
        /// </summary>
        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public int Page { get; set; }

        /// <summary>
        /// 每页行数
        /// </summary>
        [JsonProperty("pageSize", NullValueHandling = NullValueHandling.Ignore)]
        public int PageSize { get; set; }

        /// <summary>
        /// 每页行数
        /// </summary>
        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public string Order { get; set; }


        /// <summary>
        /// 反序
        /// </summary>
        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public bool Desc { get; set; }

        
        string IApiArgument.ToFormString()
        {
            return $"Page={Page}&PageSize={PageSize}&Order={Order}&Desc={Desc}";
        }

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="message">返回的消息</param>
        /// <returns>成功则返回真</returns>
        public bool Validate(out string message)
        {
            message = null;
            return true;
        }
    }
}