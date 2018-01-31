using System.Collections.Generic;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{

    /// <summary>
    /// API返回数组泛型类
    /// </summary>
    public class ApiPageResult<TData> : ApiResult<ApiPageData<TData>>
    {
    }

    /// <summary>
    /// API返回分页信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiPage : IApiResultData
    {
        /// <summary>
        /// 当前页号（从1开始）
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PageIndex { get; set; }

        /// <summary>
        /// 页行数
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PageCount { get; set; }

        /// <summary>
        /// 总行数
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int RowCount { get; set; }
    }

    /// <summary>
    /// API返回分布页数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiPageData<TData> : ApiPage
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [JsonProperty("Rows", NullValueHandling = NullValueHandling.Ignore)]
        public List<TData> Rows { get; set; }
    }
    /// <summary>
    /// API返回数组泛型类
    /// </summary>
    public class ApiPageResult : ApiResult<ApiPage>
    {
    }
}