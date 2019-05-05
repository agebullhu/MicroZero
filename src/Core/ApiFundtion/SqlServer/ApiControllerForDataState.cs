// // /*****************************************************
// // (c)2016-2016 Copy right www.gboxt.com
// // 作者:
// // 工程:Agebull.DataModel
// // 建立:2016-06-12
// // 修改:2016-06-16
// // *****************************************************/

#region 引用

using System;
using System.Linq.Expressions;
using Agebull.Common.Context;
using Agebull.EntityModel.Common;
using Agebull.EntityModel.SqlServer;
using Agebull.EntityModel.BusinessLogic.SqlServer;
using Agebull.MicroZero.ZeroApis;

#endregion

namespace Agebull.MicroZero.SqlServer
{
    /// <summary>
    ///     支持数据状态的启用禁用方法的页面的基类
    /// </summary>
    public abstract class ApiControllerForDataState<TData, TAccess, TDatabase, TBusinessLogic> :
        ApiController<TData, TAccess, TDatabase, TBusinessLogic>
        where TData : EditDataObject, IStateData, IIdentityData, new()
        where TAccess : DataStateTable<TData, TDatabase>, new()
        where TBusinessLogic : BusinessLogicByStateData<TData, TAccess, TDatabase>, new()
        where TDatabase : SqlServerDataBase
    {
        #region 数据校验支持

        /// <summary>
        ///     检查值的唯一性
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="name"></param>
        /// <param name="field"></param>
        protected override void CheckUnique<TValue>(string name, Expression<Func<TData, TValue>> field)
        {
            var no = GetArg("No");
            if (string.IsNullOrEmpty(no))
            {
                SetFailed(name + "为空");
                return;
            }

            var id = GetIntArg("id", 0);
            Expression<Func<TData, bool>> condition;
            if (id == 0)
                condition = p => p.DataState < DataStateType.Delete;
            else
                condition = p => p.Id != id && p.DataState < DataStateType.Delete;
            if (Business.Access.IsUnique(field, no, condition))
                SetFailed(name + "[" + no + "]不唯一");
            else
                GlobalContext.Current.LastMessage = name + "[" + no + "]唯一";
        }

        #endregion

        #region API

        /// <summary>
        ///      重置数据状态
        /// </summary>
        
        [Route("state/reset")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult Reset(IdsArguent arg)
        {
            
            OnReset();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///      锁定数据
        /// </summary>
        
        [Route("state/lock")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult Lock(IdsArguent arg)
        {
            
            OnLock();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///      废弃数据
        /// </summary>
        
        [Route("state/discard")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult Discard(IdsArguent arg)
        {
            
            OnDiscard();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///      禁用数据
        /// </summary>
        
        [Route("state/disable")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult Disable(IdsArguent arg)
        {
            
            OnDisable();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        /// <summary>
        ///      启用数据
        /// </summary>
        
        [Route("state/enable")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult Enable(IdsArguent arg)
        {
            
            OnEnable();
            return IsFailed
                ? (new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                })
                : ApiResult.Ok;
        }

        #endregion

        #region 操作

        /// <summary>
        ///     锁定对象
        /// </summary>
        protected virtual void OnLock()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.LoopIds(ids, Business.Lock))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        /// <summary>
        ///     恢复对象
        /// </summary>
        private void OnReset()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.LoopIds(ids, Business.Reset))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        /// <summary>
        ///     废弃对象
        /// </summary>
        private void OnDiscard()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.LoopIds(ids, Business.Discard))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        /// <summary>
        ///     启用对象
        /// </summary>
        private void OnEnable()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.LoopIds(ids, Business.Enable))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        /// <summary>
        ///     禁用对象
        /// </summary>
        private void OnDisable()
        {
            var ids = GetLongArrayArg("selects");
            if (!Business.LoopIds(ids, Business.Disable))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        #endregion


        #region 列表数据

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected override ApiPageData<TData> GetListData()
        {
            var root = new LambdaItem<TData>();
            return GetListData(root);
        }

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected override ApiPageData<TData> GetListData(LambdaItem<TData> lambda)
        {
            var state = GetIntArg("dataState", 0x100);
            if (state >= 0 && state < 0x100)
            {
                lambda.AddRoot(p => p.DataState == (DataStateType)state);
            }
            else
            {
                lambda.AddRoot(p => p.DataState < DataStateType.Delete);
            }

            return DoGetListData(lambda);
        }

        #endregion
    }

    /// <summary>
    ///     支持数据状态的启用禁用方法的页面的基类
    /// </summary>
    public abstract class ApiControllerForDataState<TData, TAccess, TDatabase> :
        ApiController<TData, TAccess, TDatabase, BusinessLogicByStateData<TData, TAccess, TDatabase>>
        where TData : EditDataObject, IStateData, IIdentityData, new()
        where TAccess : DataStateTable<TData, TDatabase>, new()
        where TDatabase : SqlServerDataBase
    {
    }
}