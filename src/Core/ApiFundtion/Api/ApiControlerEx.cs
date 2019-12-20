using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.Common.Context;
using Agebull.Common.OAuth;
using Agebull.EntityModel.Common;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// ApiController扩展
    /// </summary>
    public abstract class ApiControlerEx : ApiController
    {
        #region 状态

        /// <summary>
        ///     是否操作失败
        /// </summary>
        protected internal bool IsFailed => GlobalContext.Current.LastState != ErrorCode.Success;

        /// <summary>
        ///     设置当前操作失败
        /// </summary>
        /// <param name="message"></param>
        protected internal void SetFailed(string message)
        {
            GlobalContext.Current.LastState = ErrorCode.LogicalError;
            GlobalContext.Current.LastMessage = message;
        }

        #endregion

        #region 权限相关

        /*// <summary>
        ///     是否公开页面
        /// </summary>
        internal protected bool IsPublicPage => BusinessContext.Context.PageItem.IsPublic;

        /// <summary>
        ///     当前页面节点配置
        /// </summary>
        public IPageItem PageItem => BusinessContext.Context.PageItem;

        /// <summary>
        ///     当前页面权限配置
        /// </summary>
        public IRolePower PagePower => BusinessContext.Context.CurrentPagePower;*/

        /// <summary>
        ///     当前用户是否已登录成功
        /// </summary>
        protected internal bool UserIsLogin => GlobalContext.Current.LoginUserId > 0;

        /// <summary>
        ///     当前登录用户
        /// </summary>
        protected internal ILoginUserInfo LoginUser => GlobalContext.Current.User;

        #endregion

        #region 参数解析

        /// <summary>
        ///     参数
        /// </summary>
        private Dictionary<string, string> _arguments;

        /// <summary>
        ///     参数
        /// </summary>
        protected internal Dictionary<string, string> Arguments => _arguments ?? (_arguments = InitArguments());

        /// <summary>
        ///     初始化参数字典
        /// </summary>
        public Dictionary<string, string> InitArguments()
        {
            if (_arguments != null)
                return _arguments;

            _arguments = !string.IsNullOrWhiteSpace(ApiCallItem.Content) && ApiCallItem.Content[0] == '{'
                ? JsonHelper.DeserializeObject<Dictionary<string, string>>(ApiCallItem.Content)
                : new Dictionary<string, string>();//StringComparer.OrdinalIgnoreCase
            var context = GlobalContext.Current.DependencyObjects.Dependency<Dictionary<string, string>>();
            if (context == null)
                return _arguments;
            foreach (var kv in context)
            {
                var key = kv.Key.Trim();
                if (!_arguments.ContainsKey(key))
                    _arguments.Add(key, kv.Value?.Trim());
            }

            GlobalContext.Current.DependencyObjects.Annex(this);
            return _arguments;
        }

        /// <summary>
        /// 获取或新增(修改)参数
        /// </summary>
        /// <param name="arg">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>参数值</returns>
        protected internal string this[string arg]
        {
            get => arg == null ? null : Arguments.TryGetValue(arg, out var vl) ? vl : null;
            set
            {
                if (Arguments.ContainsKey(arg))
                    Arguments[arg] = value;
                else
                    Arguments.Add(arg, value);
            }
        }

        /// <summary>
        ///     当前请求是否包含这个参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>是否包含这个参数</returns>
        protected internal bool ContainsArgument(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && Arguments.ContainsKey(name);
        }

        /// <summary>
        ///     设置替代参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        protected internal void SetArg(string name, string value)
        {
            this[name] = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        ///     设置替代参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        protected internal void SetArg(string name, int value)
        {
            this[name] = value.ToString();
        }

        /// <summary>
        ///     设置替代参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        protected internal void SetArg(string name, object value)
        {
            this[name] = value?.ToString();
        }

        /// <summary>
        ///     获取参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <returns>参数值</returns>
        protected internal string GetArgValue(string name)
        {
            return this[name];
        }

        /// <summary>
        ///     转换面参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="action">转换方法</param>
        protected internal void ConvertQueryString(string name, Action<string> action)
        {
            if (Arguments.TryGetValue(name, out var val))
                action(val);
        }


        /// <summary>
        ///     获取参数(文本)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>文本</returns>
        protected internal string GetArg(string name)
        {
            if (!Arguments.TryGetValue(name, out var value))
                return null;
            var vl = value?.Trim();
            return string.IsNullOrWhiteSpace(vl) ? null : vl;
        }

        /// <summary>
        ///     读参数(泛型),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="convert">转换方法</param>
        /// <param name="def">默认值</param>
        /// <returns>值</returns>
        protected internal T GetArg<T>(string name, Func<string, T> convert, T def)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? def : convert(value);
        }


        /// <summary>
        ///     读参数(泛型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="convert">转换方法</param>
        /// <returns>参数为空或不存在,返回不成功,其它情况视convert返回值自行控制</returns>
        protected internal bool GetArg(string name, Func<string, bool> convert)
        {
            var value = GetArgValue(name);
            return !string.IsNullOrEmpty(value) && convert(value);
        }
        /// <summary>
        ///     读参数(文本),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>文本</returns>
        protected internal string GetArg(string name, string def)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? def : value;
        }

        /// <summary>
        ///     获取参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected internal int GetIntArg(string name)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? -1 : int.Parse(value);
        }

        /// <summary>
        ///     获取参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected internal double GetDoubleArg(string name)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? double.NaN : double.Parse(value);
        }

        /// <summary>
        ///     获取参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected internal float GetSingleArg(string name)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? float.NaN : float.Parse(value);
        }

        /// <summary>
        ///     获取参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected internal Guid GetGuidArg(string name)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? Guid.Empty : Guid.Parse(value);
        }
        /// <summary>
        ///     获取参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected internal byte GetByteArg(string name)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? (byte)0 : byte.Parse(value);
        }

        /// <summary>
        ///     获取参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected internal int[] GetIntArrayArg(string name)
        {
            var value = GetArgValue(name);

            return string.IsNullOrEmpty(value)
                ? (new int[0])
                : value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        }

        /// <summary>
        ///     获取参数(int类型),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>int类型,如果存在且不能转为int类型将出现异常</returns>
        protected internal int GetIntArg(string name, int def)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) || value == "NaN" ? def : int.Parse(value);
        }

        /// <summary>
        ///     获取参数(数字),模糊名称读取
        /// </summary>
        /// <param name="names">多个名称</param>
        /// <returns>名称解析到的第一个不为0的数字,如果有名称存在且不能转为int类型将出现异常</returns>
        protected internal int GetIntAnyArg(params string[] names)
        {
            return names.Select(p => GetIntArg(p, 0)).FirstOrDefault(re => re != 0);
        }

        /// <summary>
        ///     获取参数(日期类型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>日期类型,为空则为空,如果存在且不能转为日期类型将出现异常</returns>
        protected internal DateTime? GetDateArg(string name)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? null : (DateTime?)DateTime.Parse(value);
        }

        /// <summary>
        ///     获取参数(日期类型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>日期类型,为空则为DateTime.MinValue,如果存在且不能转为日期类型将出现异常</returns>
        protected internal DateTime GetDateArg2(string name)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? DateTime.MinValue : DateTime.Parse(value);
        }

        /// <summary>
        ///     获取参数(日期类型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def"></param>
        /// <returns>日期类型,为空则为空,如果存在且不能转为日期类型将出现异常</returns>
        protected internal DateTime GetDateArg(string name, DateTime def)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? def : DateTime.Parse(value);
        }


        /// <summary>
        ///     获取参数bool类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected internal bool GetBoolArg(string name)
        {
            var value = GetArgValue(name);
            return !string.IsNullOrEmpty(value) && (value != "0" && (value == "1" || value == "yes" || bool.Parse(value)));
        }


        /// <summary>
        ///     获取参数(decimal型数据)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>decimal型数据,如果未读取值则为-1,如果存在且不能转为decimal类型将出现异常</returns>
        protected internal decimal GetDecimalArg(string name)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? -1 : decimal.Parse(value);
        }

        /// <summary>
        ///     获取参数(decimal型数据),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>decimal型数据,如果存在且不能转为decimal类型将出现异常</returns>
        protected internal decimal GetDecimalArg(string name, decimal def)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? def : decimal.Parse(value);
        }

        /// <summary>
        ///     获取参数(long型数据),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>long型数据,如果存在且不能转为long类型将出现异常</returns>
        protected internal long GetLongArg(string name, long def = -1)
        {
            var value = GetArgValue(name);
            return string.IsNullOrEmpty(value) ? def : long.Parse(value);
        }

        /// <summary>
        ///     获取参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected internal long[] GetLongArrayArg(string name)
        {
            var value = GetArgValue(name);

            return string.IsNullOrEmpty(value)
                ? (new long[0])
                : value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
        }

        /// <summary>
        ///     获取参数(数字),模糊名称读取
        /// </summary>
        /// <param name="names">多个名称</param>
        /// <returns>名称解析到的第一个不为0的数字,如果有名称存在且不能转为int类型将出现异常</returns>
        protected internal long GetLongAnyArg(params string[] names)
        {
            return names.Select(p => GetLongArg(p, 0)).FirstOrDefault(re => re != 0);
        }

        #region TryGet

        /// <summary>
        ///     读参数(泛型),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="convert">转换方法</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGe<T>(string name, Func<string, T> convert, out T value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = default;
                return false;
            }

            try
            {
                value = convert(str);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }


        /// <summary>
        ///     尝试获取参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>值是否存在</returns>
        protected internal bool TryGet(string name, out string value)
        {
            if (!Arguments.TryGetValue(name, out value))
            {
                return false;
            }
            value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            return true;
        }


        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out bool value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = false;
                return false;
            }
            try
            {
                switch (str.ToUpper())
                {
                    case "1":
                    case "yes":
                        value = true;
                        return true;
                    case "0":
                    case "no":
                        value = false;
                        return true;
                    default:
                        return bool.TryParse(str, out value);
                }
            }
            catch
            {
                value = false;
                return false;
            }
        }

        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out DateTime value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = DateTime.MinValue;
                return false;
            }
            try
            {
                return DateTime.TryParse(str, out value);
            }
            catch
            {
                value = DateTime.MinValue;
                return false;
            }
        }
        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out int value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = 0;
                return false;
            }
            try
            {
                return int.TryParse(str, out value);
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out decimal value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = 0;
                return false;
            }
            try
            {
                return decimal.TryParse(str, out value);
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out float value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = float.NaN;
                return false;
            }
            try
            {
                return float.TryParse(str, out value) && !float.IsNaN(value);
            }
            catch
            {
                value = float.NaN;
                return false;
            }
        }

        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out double value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = double.NaN;
                return false;
            }
            try
            {
                return double.TryParse(str, out value) && !double.IsNaN(value);
            }
            catch
            {
                value = double.NaN;
                return false;
            }
        }
        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out short value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = 0;
                return false;
            }
            try
            {
                return short.TryParse(str, out value);
            }
            catch
            {
                value = 0;
                return false;
            }
        }
        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out long value)
        {
            if (!Arguments.TryGetValue(name, out var str))
            {
                value = 0;
                return false;
            }
            try
            {
                return long.TryParse(str, out value);
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out string[] value)
        {
            if (!Arguments.TryGetValue(name, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = new string[0];
                return false;
            }
            try
            {
                value = str.Split(new[] { ',' });
                return value.Length > 0;
            }
            catch
            {
                value = new string[0];
                return false;
            }
        }
        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out int[] value)
        {
            if (!Arguments.TryGetValue(name, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = new int[0];
                return false;
            }
            try
            {
                value = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                return value.Length > 0;
            }
            catch
            {
                value = new int[0];
                return false;
            }
        }

        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out List<int> value)
        {
            if (!Arguments.TryGetValue(name, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = new List<int>();
                return false;
            }
            try
            {
                value = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                return value.Count > 0;
            }
            catch
            {
                value = new List<int>();
                return false;
            }
        }
        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGetIDs(string name, out List<long> value)
        {
            if (!Arguments.TryGetValue(name, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = new List<long>();
                return false;
            }
            try
            {
                value = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();
                return value.Count > 0;
            }
            catch
            {
                value = new List<long>();
                return false;
            }
        }

        /// <summary>
        ///     读参数,如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>值</returns>
        protected internal bool TryGet(string name, out long[] value)
        {
            if (!Arguments.TryGetValue(name, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = new long[0];
                return false;
            }
            try
            {
                value = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
                return value.Length > 0;
            }
            catch
            {
                value = new long[0];
                return false;
            }
        }
        #endregion
        #endregion
    }
}