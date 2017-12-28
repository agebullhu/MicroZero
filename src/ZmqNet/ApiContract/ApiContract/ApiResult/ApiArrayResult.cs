using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yizuan.Service.Api
{
    /// <summary>
    /// 表示API返回的列表
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class ApiList<TData> : List<TData>, IApiResultData
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="list"></param>
        public ApiList(IList<TData> list)
        {
            if (list == null)
                return;
            AddRange(list);
        }
    }

    /// <summary>
    /// API返回数组泛型类
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiArrayResult<TData> : IApiResult<ApiList<TData>>
    {
        /// <summary>
        /// 成功或失败标记
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Result { get; set; }

        /// <summary>
        /// API执行状态（为空表示状态正常）
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IApiStatusResult Status { get; set; }

        private ApiList<TData> _datas;

        /// <summary>
        /// 返回值
        /// </summary>
        [JsonIgnore]
        ApiList<TData> IApiResult<ApiList<TData>>.ResultData
        {
            get
            {
                if (_datas != null)
                    return _datas;
                _datas = ResultData as ApiList<TData>;
                if (_datas != null)
                    return _datas;
                return _datas = new ApiList<TData>(ResultData);
            }
            set { ResultData = value; }
        }

        /// <summary>
        /// 返回值
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TData> ResultData { get; set; }

    }
}