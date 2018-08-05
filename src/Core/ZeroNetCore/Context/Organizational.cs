using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace Agebull.Common.OAuth
{
    /// <summary>
    /// 当前发生业务的组织
    /// </summary>
    public interface IOrganizational : IApiResultData
    {
        /// <summary>
        /// 组织的唯一标识(数字)
        /// </summary>
        long OrgId { get; set; }

        /// <summary>
        /// 组织的唯一标识(文字)
        /// </summary>
        string OrgKey { get; set; }

        /// <summary>
        /// 组织的路由名称
        /// </summary>
        string RouteName { get; set; }

    }

    /// <summary>
    /// 当前发生业务的组织
    /// </summary>
    [DataContract, Category("上下文")]
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganizationalInfo : IOrganizational
    {
        /// <summary>
        /// 组织的唯一标识(数字)
        /// </summary>
        public long OrgId { get; set; }

        /// <summary>
        /// 组织的唯一标识(文字)
        /// </summary>
        public string OrgKey { get; set; }

        /// <summary>
        /// 组织的路由名称
        /// </summary>
        public string RouteName { get; set; }


        #region 预定义

        /// <summary>
        /// 系统用户
        /// </summary>
        public static OrganizationalInfo System { get; } = new OrganizationalInfo
        {
            OrgId  = 0,
            OrgKey = "zero_center",
            RouteName = "*"
        };

        #endregion
    }
}