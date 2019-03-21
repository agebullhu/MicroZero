using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Xuhui.Internetpro.WzHealthCardService
{
    /// <summary>
    /// 参数头
    /// </summary>
    [DataContract,JsonObject(MemberSerialization.OptIn)]
    public partial class ApiArgumentHeader
    {

        /// <summary>
        /// 机构代码
        /// </summary>
        /// <remarks>
        /// 发起交易的机构代码,按AppId申请时数据填写
        /// </remarks>
        /// <value>
        /// 不能为空.可存储64个字符.合理长度应不大于64.
        /// </value>
        [DataMember , JsonProperty("organizationId", NullValueHandling = NullValueHandling.Ignore)]
        public string OrganizationId
        {
            get;
            set;
        }
        /// <summary>
        /// 应用的编码
        /// </summary>
        /// <remarks>
        /// 接入App应用的编码,按AppId申请时数据填写
        /// </remarks>
        /// <value>
        /// 不能为空.可存储16个字符.合理长度应不大于16.
        /// </value>
        [DataMember , JsonProperty("appId", NullValueHandling = NullValueHandling.Ignore)]
        public string AppId
        {
            get;
            set;
        }
        /// <summary>
        /// 否
        /// </summary>
        /// <remarks>
        /// 数据来源,按AppId申请时数据填写
        /// </remarks>
        /// <value>
        /// 不能为空.可存储2个字符.合理长度应不大于2.
        /// </value>
        [DataMember , JsonProperty("dataSources", NullValueHandling = NullValueHandling.Ignore)]
        public string DataSources
        {
            get;
            set;
        }
        /// <summary>
        /// 是
        /// </summary>
        /// <remarks>
        /// 交易码,由各业务接口指定。
        /// </remarks>
        /// <value>
        /// 不能为空.可存储5个字符.合理长度应不大于5.
        /// </value>
        [DataMember , JsonProperty("tradeCode", NullValueHandling = NullValueHandling.Ignore)]
        public string TradeCode
        {
            get;
            set;
        }
        /// <summary>
        /// 是
        /// </summary>
        /// <remarks>
        /// 动态访问令牌,非特殊说明的接口,使用通过动态令牌接口取得的accessToken,否则按接口说明处理。
        /// </remarks>
        /// <value>
        /// 不能为空.可存储32个字符.合理长度应不大于32.
        /// </value>
        [DataMember , JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token
        {
            get;
            set;
        }
        /// <summary>
        /// 是
        /// </summary>
        /// <remarks>
        /// 本机构内唯一的请求标识字符串,每次请求不相同,正式环境重复请求将会被限流处理,建议使用自动生成的GUID/UUID。
        /// </remarks>
        /// <value>
        /// 不能为空.可存储32个字符.合理长度应不大于32.
        /// </value>
        [DataMember , JsonProperty("requestId", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId
        {
            get;
            set;
        }
        /// <summary>
        /// 请求时间
        /// </summary>
        /// <remarks>
        /// 请求时间
        /// </remarks>
        [DataMember , JsonProperty("requestTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime RequestTime
        {
            get;
            set;
        }
        /// <summary>
        /// 操作员工号
        /// </summary>
        /// <remarks>
        /// 操作员工号(医院必填)
        /// </remarks>
        /// <value>
        /// 可存储20个字符.合理长度应不大于20.
        /// </value>
        [DataMember , JsonProperty("operatorCode", NullValueHandling = NullValueHandling.Ignore)]
        public string OperatorCode
        {
            get;
            set;
        }
        /// <summary>
        /// 操作员姓名
        /// </summary>
        /// <remarks>
        /// 操作员姓名(医院必填)
        /// </remarks>
        /// <value>
        /// 可存储50个字符.合理长度应不大于50.
        /// </value>
        [DataMember , JsonProperty("operatorName", NullValueHandling = NullValueHandling.Ignore)]
        public string OperatorName
        {
            get;
            set;
        }
        /// <summary>
        /// 请求终端的IPv4地址
        /// </summary>
        /// <remarks>
        /// 请求终端的IPv4地址(医院必填),如:192.168.10.2
        /// </remarks>
        /// <value>
        /// 可存储15个字符.合理长度应不大于15.
        /// </value>
        [DataMember , JsonProperty("clientIp", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientIp
        {
            get;
            set;
        }
        /// <summary>
        /// 请求终端的MAC地址
        /// </summary>
        /// <remarks>
        /// 请求终端的MAC地址(大写) (医院必填)如：01-FE-23-49-28-D0
        /// </remarks>
        /// <value>
        /// 可存储17个字符.合理长度应不大于17.
        /// </value>
        [DataMember , JsonProperty("clientMacAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientMacAddress
        {
            get;
            set;
        }
        /// <summary>
        /// 请求参数签名
        /// </summary>
        /// <remarks>
        /// 请求参数签名
        /// </remarks>
        /// <value>
        /// 不能为空.可存储344个字符.合理长度应不大于344.
        /// </value>
        [DataMember , JsonProperty("sign", NullValueHandling = NullValueHandling.Ignore)]
        public string Sign
        {
            get;
            set;
        }
        /// <summary>
        /// 否
        /// </summary>
        /// <remarks>
        /// 请求方自定义的透传参数（原样返回）,小于100字符,如有大于100字符,请内部做一个键值查询,内容内部保存,查询的Key作为透传参数。
        /// </remarks>
        /// <value>
        /// 可存储100个字符.合理长度应不大于100.
        /// </value>
        [DataMember , JsonProperty("extend", NullValueHandling = NullValueHandling.Ignore)]
        public string Extend
        {
            get;
            set;
        }
    }
}