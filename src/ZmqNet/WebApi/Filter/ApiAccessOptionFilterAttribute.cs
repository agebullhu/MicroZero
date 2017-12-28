using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using GoodLin.Common.Configuration;

namespace Yizuan.Service.Api.WebApi
{

    /// <summary>
    /// API访问配置
    /// </summary>
    [Flags]
    public enum ApiAccessOption
    {
        /// <summary>
        /// 不可访问
        /// </summary>
        None,
        /// <summary>
        /// 公开访问
        /// </summary>
        Public = 0x1,
        /// <summary>
        /// 内部访问
        /// </summary>
        Internal = 0x2,
        /// <summary>
        /// 游客
        /// </summary>
        Anymouse = 0x4,
        /// <summary>
        /// 客户
        /// </summary>
        Customer = 0x8,
        /// <summary>
        /// 商家用户
        /// </summary>
        Business = 0x10,
        /// <summary>
        /// 内部员工
        /// </summary>
        Employe = 0x20,
    }

    /// <summary>
    /// API配置过滤器
    /// </summary>
    public class ApiAccessOptionFilterAttribute : ActionFilterAttribute
    {
        private readonly ApiAccessOption _option;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="option"></param>
        public ApiAccessOptionFilterAttribute(ApiAccessOption option)
        {
            _option = option;
        }

        /// <summary>
        /// 动作运行前
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            //没有配置访问范围与访问者类型的API是不能访问的
            if (_option == ApiAccessOption.None && _option < ApiAccessOption.Anymouse)
            {
                throw new HttpResponseException(filterContext.Request.ToResponse(ApiResult.Error(ErrorCode.DenyAccess)));
            }

            //公开、内部必须有其一，否则不能访问
            if (!_option.HasFlag(ApiAccessOption.Public))
            {
                if (!_option.HasFlag(ApiAccessOption.Internal))
                {
                    throw new HttpResponseException(filterContext.Request.ToResponse(ApiResult.Error(ErrorCode.DenyAccess)));
                }

                if (ApiContext.RequestContext == null || ApiContext.RequestContext.ServiceKey == GlobalVariable.ServiceKey)
                {
                    throw new HttpResponseException(filterContext.Request.ToResponse(ApiResult.Error(ErrorCode.DenyAccess)));
                }
            }
            //非匿名用户时用户类型校验（员工、商家类型未校验
            if (!_option.HasFlag(ApiAccessOption.Anymouse))
            {
                if (_option.HasFlag(ApiAccessOption.Customer) && (ApiContext.Customer == null || ApiContext.Customer.UserId <= 0))
                {
                    throw new HttpResponseException(filterContext.Request.ToResponse(ApiResult.Error(ErrorCode.DenyAccess)));
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
