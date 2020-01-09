using System;
using System.Linq;
using Agebull.Common.Ioc;
using Agebull.MicroZero;
using Agebull.EntityModel.Common;
using Agebull.MicroZero.ZeroApis;
using Microsoft.AspNetCore.Mvc;
using MicroZero.Http.Route;

namespace WebMonitor.Controllers
{
    public class PlanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Active()
        {
            var result = IocHelper.Create<PlanManage>().Active();
            return new JsonResult(result);
        }
        [HttpGet]
        public IActionResult History()
        {
            var result = IocHelper.Create<PlanManage>().History();
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Station(string id)
        {
            var result = IocHelper.Create<PlanManage>().Station(id);
            return new JsonResult(result);
        }
        [HttpGet]
        public IActionResult Filter(string id)
        {
            var result = IocHelper.Create<PlanManage>().Filter(id);
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Clear(string id)
        {
            var result = IocHelper.Create<PlanManage>().Clear();
            return new JsonResult(result);
        }
        [HttpGet]
        public IActionResult Test(string id)
        {
            var result = IocHelper.Create<PlanManage>().Test();
            return new JsonResult(result);
        }
        
        [HttpPost]
        public IActionResult Add()
        {
            try
            {
                var result = IocHelper.Create<PlanManage>().NewPlan(new ClientPlan
                {
                    plan_type = (plan_date_type)GetIntArg("plan_type"),
                    plan_value = GetShortArg("plan_value"),
                    plan_repet = GetIntArg("plan_repet"),
                    description = GetArg("description"),
                    command = GetArg("command"),
                    station = GetArg("station"),
                    context = GetArg("context"),
                    argument = GetArg("argument"),
                    no_skip = GetBoolArg("no_skip"),
                    plan_time = GetDateArg2("plan_time1"),
                    skip_set = GetIntArg("skip_set")
                });
                return new JsonResult(result);

            }
            catch (Exception ex)
            {
                return new JsonResult(ApiResult.Error(ErrorCode.LocalException, ex.Message));
            }
        }

        [HttpGet]
        public IActionResult Flush()
        {
            var result = IocHelper.Create<PlanManage>().FlushList();
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Pause(string id)
        {
            var result = IocHelper.Create<PlanManage>().Pause(id);
            return new JsonResult(result);
        }
        [HttpGet]
        public IActionResult Reset(string id)
        {
            var result = IocHelper.Create<PlanManage>().Reset(id);
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Remove(string id)
        {
            var result = IocHelper.Create<PlanManage>().Remove(id);
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Close(string id)
        {
            var result =IocHelper.Create<PlanManage>().Close(id);
            return new JsonResult(result);
        }


        #region 参数解析


        /// <summary>
        ///     转换页面参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="action">转换方法</param>
        protected void ConvertQueryString(string name, Action<string> action)
        {
            var val = Request.Form[name];
            if (!string.IsNullOrEmpty(val))
            {
                action(val);
            }
        }

        /// <summary>
        ///     当前请求是否包含这个参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>是否包含这个参数</returns>
        protected bool ContainsArgument(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !Request.Form.ContainsKey(name) || string.IsNullOrWhiteSpace(Request.Form[name]))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 设置替代参数
        /// </summary>
        /// <param name="name"></param>
        protected string GetArgValue(string name)
        {
            return !Request.Form.ContainsKey(name) ? null : (string)Request.Form[name];
        }

        /// <summary>
        ///     读取页面参数(文本)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>文本</returns>
        protected string GetArg(string name)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            var vl = value.Trim();
            if (vl == "null")
            {
                return null;
            }
            if (vl == "undefined")
            {
                return null;
            }
            return vl;
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
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return def;
            }
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
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return false;
            }
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
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return def;
            }
            return value;
        }

        /// <summary>
        ///     读取页面参数(日期类型)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>日期类型,为空则为空,如果存在且不能转为日期类型将出现异常</returns>
        protected DateTime? GetDateArg(string name)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "-" || value == "undefined" || value == "null" || !DateTime.TryParse(value, out _))
            {
                return null;
            }
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
            if (string.IsNullOrEmpty(value) || value == "-" || value == "undefined" || value == "null" || !DateTime.TryParse(value, out var date))
            {
                return DateTime.MinValue;
            }
            return date;
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
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return def;
            }
            return DateTime.Parse(value);
        }

        /// <summary>
        ///     读取页面参数int类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected int[] GetIntArrayArg(string name)
        {
            var value = GetArgValue(name);

            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return new int[0];
            }
            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        }

        /// <summary>
        ///     读取页面参数bool类型
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>int类型,为空则为-1,如果存在且不能转为int类型将出现异常</returns>
        protected bool GetBoolArg(string name)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return false;
            }
            return value != "0" && (value == "1" || value == "yes" || (bool.TryParse(value, out var bl) && bl));
        }

        /// <summary>
        ///     读取页面参数(int类型),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>int类型,如果存在且不能转为int类型将出现异常</returns>
        protected int GetIntArg(string name, int def = 0)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "NaN" || value == "undefined" || value == "null")
            {
                return def;
            }
            return int.TryParse(value, out var num) ? num : def;
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
        ///     读取页面参数(decimal型数据)
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>decimal型数据,如果未读取值则为-1,如果存在且不能转为decimal类型将出现异常</returns>
        protected decimal GetDecimalArg(string name, decimal def)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return def;
            }
            return decimal.TryParse(value, out var num) ? num : def;
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
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return def;
            }
            return long.TryParse(value, out var num) ? num : def;
        }

        /// <summary>
        ///     读取页面参数(short),如果参数为空或不存在,用默认值填充
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">默认值</param>
        /// <returns>long型数据,如果存在且不能转为long类型将出现异常</returns>
        protected short GetShortArg(string name, short def = -1)
        {
            var value = GetArgValue(name);
            if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
            {
                return def;
            }
            return short.TryParse(value, out var num) ? num : def;
        }

        #endregion
    }
}