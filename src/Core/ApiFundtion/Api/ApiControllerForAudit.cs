// // /*****************************************************
// // (c)2016-2016 Copy right www.gboxt.com
// // 作者:
// // 工程:Agebull.DataModel
// // 建立:2016-06-12
// // 修改:2016-06-16
// // *****************************************************/

#region 引用

using System.Collections.Generic;

using Agebull.EntityModel.Common;

using Agebull.MicroZero.ZeroApi;
using Agebull.EntityModel.Common.Extends;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;
using Agebull.Common.Context;
using Agebull.EntityModel.MySql;
using Agebull.EntityModel.MySql.BusinessLogic;

#endregion

namespace Agebull.Common.WebApi
{
    /// <summary>
    ///     审核支持API页面的基类
    /// </summary>
    public abstract class ApiControllerForAudit<TData, TAccess, TDatabase, TBusinessLogic> :
        ApiControllerForDataState<TData, TAccess, TDatabase, TBusinessLogic>
        where TData : EditDataObject, IStateData, IHistoryData, IAuditData, IIdentityData, new()
        where TAccess : DataStateTable<TData, TDatabase>, new()
        where TBusinessLogic : BusinessLogicByAudit<TData, TAccess, TDatabase>, new()
        where TDatabase : MySqlDataBase
    {
        /// <summary>
        ///     提交审核
        /// </summary>
        protected virtual void OnSubmitAudit()
        {

            var ids = GetLongArrayArg("selects");
            if (!DoValidate(ids))
                return;
            if (!Business.Submit(ids))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;

        }

        /// <summary>
        ///     退回审核
        /// </summary>
        private void OnBackAudit()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.Back(ids))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        /// <summary>
        ///     审核
        /// </summary>
        private void OnUnAudit()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.UnAudit(ids))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        /// <summary>
        ///     审核
        /// </summary>
        protected virtual void OnAuditPass()
        {
            var ids = GetLongArrayArg("selects");
            if (!DoValidate(ids))
            {
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
                return;
            }
            var result = Business.AuditPass(ids);
            if (!result)
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        private bool DoValidate(IEnumerable<long> ids)
        {
            var message = new ValidateResultDictionary();
            var succeed = Business.Validate(ids, message.TryAdd);

            if (message.Result.Count > 0)
            {
                GlobalContext.Current.LastMessage = message.ToString();
            }
            return succeed;
        }

        /// <summary>
        ///     拉回
        /// </summary>
        private void OnPullback()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.Pullback(ids))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        /// <summary>
        ///     审核
        /// </summary>
        private void OnAuditDeny()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.AuditDeny(ids))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }


        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected override ApiPageData<TData> GetListData(LambdaItem<TData> lambda)
        {
            var audit = GetIntArg("audit", -1);
            if (audit == 0x100 || audit < 0)
                return base.GetListData(lambda);
            if (audit <= (int) AuditStateType.End)
            {
                lambda.AddRoot(p => p.AuditState == (AuditStateType)audit);
                return base.GetListData(lambda);
            }

            switch (audit)
            {
                case 0x10: //废弃
                case 0xFF: //删除
                    SetArg("dataState", audit);
                    break;
                case 0x13: //停用
                    SetArg("dataState", (int)DataStateType.Disable);
                    break;
                case 0x11: //未审核
                    lambda.AddRoot(p => p.AuditState <= AuditStateType.Again);
                    break;
                case 0x12: //未结束
                    lambda.AddRoot(p => p.AuditState < AuditStateType.End);
                    break;
            }

            return base.GetListData(lambda);
        }

        #region API

        /// <summary>
        ///     审核不通过
        /// </summary>

        [Route("audit/deny")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult AuditDeny()
        {
            
            OnAuditDeny();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///     拉回已提交的审核
        /// </summary>

        [Route("audit/pullback")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult Pullback()
        {
            
            OnPullback();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///     提交审核
        /// </summary>
        
        [Route("audit/submit")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult SubmitAudit()
        {
            
            OnSubmitAudit();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///     校验审核数据
        /// </summary>
        
        [Route("audit/validate")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult Validate()
        {
            
            DoValidate(GetLongArrayArg("selects"));
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }


        /// <summary>
        ///     审核通过
        /// </summary>
        
        [Route("audit/pass")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult AuditPass()
        {
            
            OnAuditPass();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///     重新审核
        /// </summary>
        
        [Route("audit/redo")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult UnAudit()
        {
            OnUnAudit();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///     退回(审核)
        /// </summary>

        [Route("audit/back")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult BackAudit()
        {
            
            OnBackAudit();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        #endregion
    }

    /// <summary>
    ///     审核支持API页面的基类
    /// </summary>
    public abstract class ApiControllerForAudit<TData, TAccess, TDatabase> :
        ApiControllerForDataState<TData, TAccess, TDatabase, BusinessLogicByAudit<TData, TAccess, TDatabase>>
        where TData : EditDataObject, IStateData, IHistoryData, IAuditData, IIdentityData, new()
        where TAccess : DataStateTable<TData, TDatabase>, new()
        where TDatabase : MySqlDataBase
    {
    }
}