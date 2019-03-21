using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.Common.Rpc;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace Xuhui.Internetpro.WzHealthCardService
{
    /// <summary>
    /// 返回值基础
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public partial class ApiResultEx : IApiResult
    {

        /// <summary>
        /// 交易码
        /// </summary>
        /// <remarks>
        /// 与请求相同
        /// </remarks>
        /// <value>
        /// 不能为空.可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("tradeCode", NullValueHandling = NullValueHandling.Ignore)]
        public string TradeCode
        {
            get;
            set;
        }

        /// <summary>
        /// 请求唯一标识符
        /// </summary>
        /// <remarks>
        /// 与请求相同
        /// </remarks>
        /// <value>
        /// 不能为空.可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("requestId", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId
        {
            get;
            set;
        }

        /// <summary>
        /// 透传参数
        /// </summary>
        /// <remarks>
        /// 与请求相同,请求不为空则返回,请求为空则不返回
        /// </remarks>
        /// <value>
        /// 不能为空.可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("extend", NullValueHandling = NullValueHandling.Ignore)]
        public string Extend
        {
            get;
            set;
        }

        /// <summary>
        /// 状态码
        /// </summary>
        /// <remarks>
        /// 0表示成功,其它表示失败。参见标准错误码与各接口的特定错误码
        /// </remarks>
        [DataMember, JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public int Code
        {
            get;
            set;
        }

        /// <summary>
        /// 状态消息
        /// </summary>
        [DataMember, JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
        public string Msg
        {
            get;
            set;
        }


        /// <summary>
        /// 构造
        /// </summary>
        public ApiResultEx() : this(WzHealthCardContext.Context.Argument)
        {
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="argument"></param>
        public ApiResultEx(ApiArgument argument)
        {
            if (argument?.Header == null)
                return;
            RequestId = argument.Header.RequestId;
            TradeCode = argument.Header.TradeCode;
            Extend = argument.Header.Extend;
        }

        #region IApiResult


        /// <inheritdoc />
        /// <summary>成功或失败标记</summary>
        bool IApiResult.Success { get => Code == 0; set => Code = value ? 0 : -1; }

        /// <inheritdoc />
        /// <summary>API执行状态（为空表示状态正常）</summary>
        IOperatorStatus IApiResult.Status
        {
            get => new OperatorStatus
            {
                ClientMessage = Msg,
                ErrorCode = Code
            };
            set
            {

                if (value == null)
                {
                    Msg = null;
                }
                else
                {
                    Msg = value.ClientMessage;
                    Code = value.ErrorCode;
                }
            }

        }

        /// <summary>API操作标识</summary>
        string IApiResult.OperatorId { get => TradeCode; set => TradeCode = value; }
        string IApiResult.RequestId { get => GlobalContext.RequestInfo.RequestId; set => GlobalContext.RequestInfo.RequestId = value; }

        int IApiResult.ErrorCode => Code;

        string IApiResult.Message => Msg;


        #endregion
    }


    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class ApiResultEx<TData> : ApiResultEx, IApiResult<TData>
    {
        /// <summary>
        /// 扩展参数
        /// </summary>
        [JsonProperty("data")]
        public TData Data { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public ApiResultEx()
        {
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="argument"></param>
        public ApiResultEx(ApiArgument argument) : base(argument)
        {
        }


        /// <summary>返回值</summary>
        TData IApiResult<TData>.ResultData => Data;

    }
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class ApiValueResultEx<TData> : ApiResultEx, IApiResult<TData>
        where TData : struct
    {
        /// <summary>
        /// 扩展参数
        /// </summary>
        [JsonProperty("data")]

        public TData Data { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public ApiValueResultEx()
        {
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="argument"></param>
        public ApiValueResultEx(ApiArgument argument) : base(argument)
        {
        }

        /// <summary>返回值</summary>
        TData IApiResult<TData>.ResultData => Data;

    }

    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class ApiArrayResultEx<TData> : ApiResultEx, IApiResult<List<TData>>
        where TData : class, IApiArgument, new()
    {
        /// <summary>
        /// 扩展参数
        /// </summary>
        [JsonProperty("data")]

        public List<TData> Data { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public ApiArrayResultEx()
        {
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="argument"></param>
        public ApiArrayResultEx(ApiArgument argument) : base(argument)
        {
        }

        /// <summary>返回值</summary>
        List<TData> IApiResult<List<TData>>.ResultData => Data;

    }
}