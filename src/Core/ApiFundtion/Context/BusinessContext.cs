// // /*****************************************************
// // (c)2016-2016 Copy right www.gboxt.com
// // 作者:
// // 工程:Agebull.DataModel
// // 建立:2016-06-12
// // 修改:2016-06-24
// // *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Agebull.Common.AppManage;
using Agebull.Common.DataModel.Redis;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Redis;
using Agebull.Common.Rpc;
using Agebull.Common.WebApi.Auth;
using Gboxt.Common.DataModel.MySql;
using Newtonsoft.Json;

#endregion

namespace Agebull.Common.WebApi
{
    /// <summary>
    ///     为业务处理上下文对象
    /// </summary>
    [DataContract]
    [Category("业务上下文")]
    [JsonObject(MemberSerialization.OptIn)]
    public class BusinessContext : GlobalContext
    {

        #region 线程单例
        
        /// <summary>
        ///     取得或设置线程单例对象，当前对象不存在时，会自动构架一个
        /// </summary>
        public static BusinessContext Context => Current as BusinessContext;

        /// <summary>
        /// 缓存当前上下文
        /// </summary>
        public void Cache()
        {
            LogRecorder.MonitorTrace(JsonConvert.SerializeObject(this));
            using (RedisProxy proxy = new RedisProxy())
            {
                proxy.Set(GetCacheKey(RequestInfo.RequestId), this);
            }
        }

        /// <summary>
        /// 得到缓存的键
        /// </summary>
        public static string GetCacheKey(Guid requestId)
        {
            return RedisKeyBuilder.ToSystemKey("api", "ctx", requestId.ToString().ToUpper());
        }

        /// <summary>
        /// 得到缓存的键
        /// </summary>
        public static string GetCacheKey(string requestId)
        {
            return RedisKeyBuilder.ToSystemKey("api", "ctx", requestId.Trim('$').ToUpper());
        }
        #endregion

        #region 构造与析构

        /// <summary>
        ///     构造
        /// </summary>
        public BusinessContext()
        {
            LogRecorder.MonitorTrace("BusinessContext.ctor");
        }

        /// <summary>
        ///     析构
        /// </summary>
        ~BusinessContext()
        {
            Dispose();
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            GC.ReRegisterForFinalize(this);
            TransactionScope.EndAll();
            LogRecorder.MonitorTrace("BusinessContext.DoDispose");
        }

        #endregion


        #region 权限对象

        /// <summary>
        ///     当前页面节点配置
        /// </summary>
        public IPageItem PageItem { get; set; }

        /// <summary>
        ///     权限校验对象
        /// </summary>
        private IPowerChecker _powerChecker;


        /// <summary>
        ///     权限校验对象
        /// </summary>
        public IPowerChecker PowerChecker => _powerChecker ?? (_powerChecker = IocHelper.Create<IPowerChecker>());

        /// <summary>
        ///     用户的角色权限
        /// </summary>
        private List<IRolePower> _powers;

        /// <summary>
        ///     用户的角色权限
        /// </summary>
        public List<IRolePower> Powers => _powers ?? (_powers = PowerChecker.LoadUserPowers(Customer));

        /// <summary>
        /// 当前页面权限设置
        /// </summary>
        public IRolePower CurrentPagePower
        {
            get;
            set;
        }

        /// <summary>
        /// 在当前页面检查是否可以执行操作
        /// </summary>
        /// <param name="action">操作</param>
        /// <returns></returns>
        public bool CanDoCurrentPageAction(string action)
        {
            return PowerChecker == null || PowerChecker.CanDoAction(Customer, PageItem, action);
        }

        #endregion
    }
}