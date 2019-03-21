using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.Common.AppManage;
using Agebull.Common.Context;
using Agebull.Common.OAuth;

using Agebull.Common.WebApi.Auth;
using Agebull.MicroZero.ZeroApi;
using Agebull.EntityModel.Common;

namespace Agebull.Common.WebApi
{
    /// <summary>
    /// ApiController扩展
    /// </summary>
    public abstract class ApiControlerEx : ApiController
    {
        /// <summary>
        /// 构造
        /// </summary>
        protected ApiControlerEx()
        {
            GlobalContext.Current.IsManageMode = true;
        }

        #region 状态

        /// <summary>
        ///     是否操作失败
        /// </summary>
        protected bool IsFailed => GlobalContext.Current.LastState != ErrorCode.Success;

        /// <summary>
        ///     设置当前操作失败
        /// </summary>
        /// <param name="message"></param>
        protected void SetFailed(string message)
        {
            GlobalContext.Current.LastState = ErrorCode.LogicalError;
            GlobalContext.Current.LastMessage = message;
        }

        #endregion

        #region 权限相关

        /// <summary>
        ///     是否公开页面
        /// </summary>
        protected bool IsPublicPage => BusinessContext.Context.PageItem.IsPublic;

        /// <summary>
        ///     当前页面节点配置
        /// </summary>
        public IPageItem PageItem => BusinessContext.Context.PageItem;

        /// <summary>
        ///     当前页面权限配置
        /// </summary>
        public IRolePower PagePower => BusinessContext.Context.CurrentPagePower;

        /// <summary>
        ///     当前用户是否已登录成功
        /// </summary>
        protected bool UserIsLogin => BusinessContext.Context.LoginUserId > 0;

        /// <summary>
        ///     当前登录用户
        /// </summary>
        protected ILoginUserInfo LoginUser => GlobalContext.Current.User;

        #endregion

        #region 参数解析

        /// <summary>
        ///     参数
        /// </summary>
        private Dictionary<string, string> _arguments;

        /// <summary>
        ///     参数
        /// </summary>
        protected Dictionary<string, string> Arguments => _arguments ?? (_arguments = InitArguments());

        /// <summary>
        ///     初始化查询字符串
        /// </summary>
        public Dictionary<string, string> InitArguments()
        {
            if (_arguments != null)
                return _arguments;
            _arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var context = GlobalContext.Current.DependencyObjects.Dependency<Dictionary<string, string>>();
            if (context != null)
            {
                foreach (var kv in context)
                    if (!_arguments.ContainsKey(kv.Key))
                        _arguments.Add(kv.Key, kv.Value);
            }
            return _arguments;
        }


        /// <summary>
        ///     转换页面参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="action">转换方法</param>
        protected void ConvertQueryString(string name, Action<string> action)
        {
            var val = Arguments[name];
            if (!string.IsNullOrEmpty(val)) action(val);
        }

        /// <summary>
        ///     当前请求是否包含这个参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>是否包含这个参数</returns>
        protected bool ContainsArgument(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return Arguments[name] != null;
        }

        /// <summary>
        ///     设置替代参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void SetArg(string name, string value)
        {
            if (Arguments.ContainsKey(name))
                Arguments[name] = value;
            else Arguments.Add(name, value);
        }

        /// <summary>
        ///     设置替代参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void SetArg(string name, int value)
        {
            if (Arguments.ContainsKey(name))
                Arguments[name] = value.ToString();
            else Arguments.Add(name, value.ToString());
        }

        /// <summary>
        ///     设置替代参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void SetArg(string name, object value)
        {
            if (Arguments.ContainsKey(name))
                Arguments[name] = value?.ToString();
            else Arguments.Add(name, value?.ToString());
        }

        /// <summary>
        ///     设置替代参数
        /// </summary>
        /// <param name="name"></param>
        protected string GetArgValue(string name)
        {
            if (Arguments.ContainsKey(name))
                return Arguments[name];
            return null;
        }

        /// <summary>
        ///     读取页面参数(文本)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>文本</returns>
        protected string GetArg(string name)
        {
            var value = GetArgValue(name);
            if (value == null) return null;
            var vl = value.Trim();
            if (vl == "null") return null;
            if (value == "undefined") return null;
            return string.IsNullOrWhiteSpace(vl) ? null : vl;
        }

        /// <summary>
        ///     读参数(文本),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="convert">转换方法</param>
        /// <param name="def">默认值</param>
        /// <returns>文本</returns>
        protected T GetArg<T>(string name, Func<string, T> convert, T def)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return def;
            return convert(value);
        }


        /// <summary>
        ///     读参数(泛型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="convert">转换方法</param>
        /// <returns>参数为空或不存在,返回不成功,其它情况视convert返回值自行控制</returns>
        protected bool GetArg(string name, Func<string, bool> convert)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return false;
            return convert(value);
        }

        /// <summary>
        ///     读参数(文本),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>文本</returns>
        protected string GetArg(string name, string def)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return def;
            return value;
        }

        /// <summary>
        ///     读取页面参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected int GetIntArg(string name)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return -1;
            return int.Parse(value);
        }

        /// <summary>
        ///     读取页面参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected int[] GetIntArrayArg(string name)
        {
            var value = GetArgValue(name);

            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return new int[0];
            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        }

        /// <summary>
        ///     读取页面参数(int类型),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>int类型,如果存在且不能转为int类型将出现异常</returns>
        protected int GetIntArg(string name, int def)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "NaN" || value == "undefined" || value == "null")
                return def;
            return int.Parse(value);
        }

        /// <summary>
        ///     读取页面参数(数字),模糊名称读取
        /// </summary>
        /// <param name="names">多个名称</param>
        /// <returns>名称解析到的第一个不为0的数字,如果有名称存在且不能转为int类型将出现异常</returns>
        protected int GetIntAnyArg(params string[] names)
        {
            return names.Select(p => GetIntArg(p, 0)).FirstOrDefault(re => re != 0);
        }

        /// <summary>
        ///     读取页面参数(日期类型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>日期类型,为空则为空,如果存在且不能转为日期类型将出现异常</returns>
        protected DateTime? GetDateArg(string name)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return null;
            return DateTime.Parse(value);
        }

        /// <summary>
        ///     读取页面参数(日期类型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>日期类型,为空则为DateTime.MinValue,如果存在且不能转为日期类型将出现异常</returns>
        protected DateTime GetDateArg2(string name)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return DateTime.MinValue;
            return DateTime.Parse(value);
        }

        /// <summary>
        ///     读取页面参数(日期类型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def"></param>
        /// <returns>日期类型,为空则为空,如果存在且不能转为日期类型将出现异常</returns>
        protected DateTime GetDateArg(string name, DateTime def)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return def;
            return DateTime.Parse(value);
        }


        /// <summary>
        ///     读取页面参数bool类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected bool GetBoolArg(string name)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return false;
            return value != "0" && (value == "1" || value == "yes" || bool.Parse(value));
        }


        /// <summary>
        ///     读取页面参数(decimal型数据)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>decimal型数据,如果未读取值则为-1,如果存在且不能转为decimal类型将出现异常</returns>
        protected decimal GetDecimalArg(string name)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return -1;
            return decimal.Parse(value);
        }

        /// <summary>
        ///     读取页面参数(decimal型数据),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>decimal型数据,如果存在且不能转为decimal类型将出现异常</returns>
        protected decimal GetDecimalArg(string name, decimal def)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return def;
            return decimal.Parse(value);
        }

        /// <summary>
        ///     读取页面参数(long型数据),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>long型数据,如果存在且不能转为long类型将出现异常</returns>
        protected long GetLongArg(string name, long def = -1)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return def;
            return long.Parse(value);
        }

        /// <summary>
        ///     读取页面参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected long[] GetLongArrayArg(string name)
        {
            var value = GetArgValue(name);

            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null") return new long[0];
            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
        }

        /// <summary>
        ///     读取页面参数(数字),模糊名称读取
        /// </summary>
        /// <param name="names">多个名称</param>
        /// <returns>名称解析到的第一个不为0的数字,如果有名称存在且不能转为int类型将出现异常</returns>
        protected long GetLongAnyArg(params string[] names)
        {
            return names.Select(p => GetLongArg(p, 0)).FirstOrDefault(re => re != 0);
        }

        #endregion
    }
}