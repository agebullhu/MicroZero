using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Agebull.Common.Context;
using Agebull.EntityModel.Common;
using Agebull.EntityModel.MySql;
using Agebull.EntityModel.BusinessLogic.MySql;
using MySql.Data.MySqlClient;
using Agebull.MicroZero.WebApi;
using Newtonsoft.Json;
using Agebull.MicroZero.ZeroApis.Common;
using Agebull.MicroZero.ZeroApis;

namespace Agebull.MicroZero.MySql
{
    /// <summary>
    ///     自动实现基本增删改查API页面的基类
    /// </summary>
    public abstract class ApiController<TData, TAccess, TDatabase, TBusinessLogic> : ApiControlerEx
        where TData : EditDataObject, IIdentityData, new()
        where TAccess : MySqlTable<TData, TDatabase>, new()
        where TBusinessLogic : UiBusinessLogicBase<TData, TAccess, TDatabase>, new()
        where TDatabase : MySqlDataBase
    {
        #region 数据校验支持

        /// <summary>
        ///     检查值的唯一性
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="name"></param>
        /// <param name="field"></param>
        protected virtual void CheckUnique<TValue>(string name, Expression<Func<TData, TValue>> field)
        {
            var no = GetArg("No");
            if (string.IsNullOrEmpty(no))
            {
                SetFailed(name + "为空");
                return;
            }

            var id = GetIntArg("id", 0);
            var result = id == 0
                ? Business.Access.IsUnique(field, no)
                : Business.Access.IsUnique(field, no, id);
            if (result)
                SetFailed(name + "[" + no + "]不唯一");
            else
                GlobalContext.Current.LastMessage = name + "[" + no + "]唯一";
        }

        #endregion

        #region 基础变量

        private TBusinessLogic _business;

        /// <summary>
        ///     业务逻辑对象
        /// </summary>
        protected TBusinessLogic Business
        {
            get => _business ?? (_business = new TBusinessLogic());
            set => _business = value;
        }

        /// <summary>
        ///     基本查询条件(SQL表述方式)
        /// </summary>
        protected virtual string BaseQueryCondition => null;

        #endregion

        #region API

        /// <summary>
        ///     实体类型
        /// </summary>
        /// <returns></returns>
        [Route("edit/eid")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult<EntityInfo> EntityType()
        {
            return ApiResult.Succees(new EntityInfo
            {
                EntityType = Business.EntityType,
                PageId = PageItem?.Id ?? 0
            });
        }

        /// <summary>
        ///     列表数据
        /// </summary>
        /// <returns></returns>
        [Route("edit/list")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiPageResult<TData> List(QueryArgument args)
        {
            return List2();
        }

        private ApiPageResult<TData> List2()
        {
            GlobalContext.Current.Feature = 1;
            var data = GetListData();
            GlobalContext.Current.Feature = 0;
            return IsFailed
                ? new ApiPageResult<TData>
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                }
                : new ApiPageResult<TData>
                {
                    Success = true,
                    ResultData = data
                };
        }

        /// <summary>
        ///     单条详细数据
        /// </summary>
        [Route("edit/details")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult<TData> Details(IdArguent arguent)
        {
            var data = DoDetails();
            return IsFailed
                ? new ApiResult<TData>
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                }
                : ApiResult.Succees(data);
        }

        /// <summary>
        ///     新增数据
        /// </summary>
        [Route("edit/addnew")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult<TData> AddNew(TData arg)
        {
            var data = DoAddNew();
            return IsFailed
                ? new ApiResult<TData>
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                }
                : ApiResult.Succees(data);
        }

        /// <summary>
        ///     更新数据
        /// </summary>
        [Route("edit/update")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult<TData> Update(TData arg)
        {
            var data = DoUpdate();
            return IsFailed
                ? new ApiResult<TData>
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                }
                : ApiResult.Succees(data);
        }

        /// <summary>
        ///     删除多条数据
        /// </summary>
        [Route("edit/delete")]
        [ApiAccessOptionFilter(ApiAccessOption.Public | ApiAccessOption.Internal | ApiAccessOption.Customer)]
        public ApiResult Delete(IdsArguent arg)
        {
            DoDelete();
            return IsFailed
                ? new ApiResult
                {
                    Success = false,
                    Status = GlobalContext.Current.LastStatus
                }
                : ApiResult.Ok;
        }

        #endregion

        #region 列表读取支持

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected virtual ApiPageData<TData> GetListData()
        {
            return LoadListData(null, null);
        }

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected ApiPageData<TData> GetListData(Expression<Func<TData, bool>> lambda)
        {
            return GetListData(new[] { lambda });
        }

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected ApiPageData<TData> GetListData(IEnumerable<Expression<Func<TData, bool>>> lambdas)
        {
            var lg = new TAccess();
            var condition = new ConditionItem(Business.Access.DataBase);
            foreach (var lambda in lambdas) PredicateConvert.Convert(lg.FieldDictionary, lambda, condition);
            return GetListData(condition);
        }

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected virtual ApiPageData<TData> GetListData(LambdaItem<TData> lambda)
        {
            return DoGetListData(lambda);
        }


        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected ApiPageData<TData> DoGetListData(LambdaItem<TData> lambda)
        {
            var lg = new TAccess();
            var condition = PredicateConvert.Convert(lg.FieldDictionary, lambda);
            return GetListData(condition);
        }

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected ApiPageData<TData> GetListData(ConditionItem item)
        {
            return GetListData(new[] { item });
        }

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected ApiPageData<TData> GetListData(IEnumerable<ConditionItem> items)
        {
            var parameters = new List<MySqlParameter>();
            var sb = new StringBuilder();
            var isFirst = true;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.ConditionSql))
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(" AND ");
                    sb.Append("(" + item.ConditionSql + ")");
                }

                parameters.AddRange(item.Parameters.Cast<MySqlParameter>());
            }

            return LoadListData(sb.ToString(), parameters.ToArray());
        }

        /// <summary>
        ///     取得列表数据
        /// </summary>
        protected ApiPageData<TData> LoadListData(string condition, MySqlParameter[] args)
        {
            var page = GetIntArg("page", 1);
            var rows = GetIntArg("rows", 20);
            var sort = GetArg("sort");
            bool desc;
            var adesc = GetArg("order", "asc").ToLower();
            if (sort == null)
            {
                sort = Business.Access.KeyField;
                desc = true;
            }
            else
            {
                desc = adesc == "desc";
            }

            SaveQueryArguments(page, sort, adesc, rows);

            if (!string.IsNullOrEmpty(BaseQueryCondition))
            {
                if (string.IsNullOrEmpty(condition))
                    condition = BaseQueryCondition;
                else if (condition != BaseQueryCondition && !condition.Contains(BaseQueryCondition))
                    condition = $"({BaseQueryCondition}) AND ({condition})";
            }

            var data = Business.PageData(page, rows, sort, desc, condition, args);
            if (OnListLoaded(data.Rows, data.RowCount))
            {
                CheckListResult(data, condition, args);
                return data;
            }

            return data;
        }

        /// <summary>
        ///     是否保存查询条件
        /// </summary>
        protected virtual bool CanSaveQueryArguments => true;

        private void SaveQueryArguments(int page, string sort, string adesc, int rows)
        {
            if (CanSaveQueryArguments)
                BusinessContext.Context?.PowerChecker?.SaveQueryHistory(LoginUser, PageItem, Arguments);
        }

        /// <summary>
        ///     数据准备返回的处理
        /// </summary>
        /// <param name="result">当前的查询结果</param>
        /// <param name="condition">当前的查询条件</param>
        /// <param name="args">当前的查询参数</param>
        protected virtual bool CheckListResult(ApiPageData<TData> result, string condition,
            params MySqlParameter[] args)
        {
            return true;
        }

        /// <summary>
        ///     数据载入的处理
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="count"></param>
        protected virtual bool OnListLoaded(IList<TData> datas, int count)
        {
            OnListLoaded(datas);
            return true;
        }

        /// <summary>
        ///     数据载入的处理
        /// </summary>
        /// <param name="datas"></param>
        protected virtual void OnListLoaded(IList<TData> datas)
        {
        }

        #endregion

        #region 基本增删改查

        /// <summary>
        ///     读取Form传过来的数据
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="convert">转化器</param>
        protected abstract void ReadFormData(TData entity, FormConvert convert);


        private long _dataId = -1;

        /// <summary>
        ///     当前上下文数据ID
        /// </summary>
        public long ContextDataId
        {
            get => _dataId < 0 ? (_dataId = GetLongArg("id")) : _dataId;
            protected set => _dataId = value;
        }

        /// <summary>
        ///     载入当前操作的数据
        /// </summary>
        protected virtual TData DoDetails()
        {
            TData data;
            if (ContextDataId <= 0)
            {
                data = CreateData();
                OnDetailsLoaded(data, true);
            }
            else
            {
                data = Business.Details(ContextDataId);
                if (data == null)
                {
                    SetFailed("数据不存在");
                    return null;
                }
                OnDetailsLoaded(data, false);
            }

            return data;
        }

        /// <summary>
        ///     详细数据载入
        /// </summary>
        protected virtual void OnDetailsLoaded(TData data, bool isNew)
        {
        }

        /// <summary>
        ///     新增一条带默认值的数据
        /// </summary>
        protected virtual TData CreateData()
        {
            return new TData();
        }

        /// <summary>
        ///     新增
        /// </summary>
        protected virtual TData DoAddNew()
        {
            var data = new TData();
            //数据校验

            var convert = new FormConvert(Arguments);
            ReadFormData(data, convert);
            data.__IsFromUser = true;
            if (!Business.AddNew(data))
            {
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
                return null;
            }
            return data;
        }

        /// <summary>
        ///     更新对象
        /// </summary>
        protected virtual TData DoUpdate()
        {
            var data = Business.Details(ContextDataId);
            if(data == null)
            {
                GlobalContext.Current.LastState = ErrorCode.ArgumentError;
                GlobalContext.Current.LastMessage = "参数错误";
                return null;
            }
            //数据校验
            var convert = new FormConvert(Arguments)
            {
                IsUpdata = true
            };
            ReadFormData(data, convert);
            data.__IsFromUser = true;
            if (!Business.Update(data))
            {
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
                return null;
            }
            return data;
        }

        /// <summary>
        ///     删除对象
        /// </summary>
        private void DoDelete()
        {
            var ids = GetArg("selects");
            if (string.IsNullOrEmpty(ids))
            {
                SetFailed("没有数据");
                return;
            }

            var lid = ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
            if (lid.Length == 0)
            {
                SetFailed("没有数据");
                return;
            }

            if (!Business.Delete(lid))
                GlobalContext.Current.LastState = ErrorCode.LogicalError;
        }

        #endregion
    }

}