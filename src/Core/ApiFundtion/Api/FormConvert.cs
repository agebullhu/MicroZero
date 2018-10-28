// // /*****************************************************
// // (c)2016-2016 Copy right www.gboxt.com
// // 作者:
// // 工程:Agebull.DataModel
// // 建立:2016-06-12
// // 修改:2016-06-16
// // *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Agebull.Common.WebApi
{
    /// <summary>
    ///     FORM对象转化辅助类
    /// </summary>
    public class FormConvert
    {
        /// <summary>
        ///     读取过程的错误消息记录
        /// </summary>
        public readonly Dictionary<string, string> Messages = new Dictionary<string, string>();

        /// <summary>
        ///     是否发生解析错误
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        ///     参数
        /// </summary>
        private readonly Dictionary<string, string> _arguments;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="arguments"></param>
        public FormConvert(Dictionary<string, string> arguments)
        {
            _arguments = arguments;
        }

        private string GetValue(string name)
        {
            return !_arguments.TryGetValue(name, out var val) ? null : val?.Trim();
        }

        /// <summary>
        /// 到文本
        /// </summary>
        /// <param name="name"></param>
        /// <param name="canNull"></param>
        /// <returns></returns>
        public string ToString(string name, bool canNull)
        {
            if (!string.IsNullOrWhiteSpace(GetValue(name))) return GetValue(name).Trim();
            //if (!canNull)
            //{
            //    AddMessage(name, "值不能为空");
            //    this.Failed = true;
            //}
            return null;
        }
        /// <summary>
        /// 设置错误参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="msg"></param>
        private void AddMessage(string name, string msg)
        {
            if (Messages.ContainsKey(name))
                Messages[name] = msg;
            else
                Messages.Add(name, msg);
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">找不到或为空时的默认值</param>
        /// <returns>值</returns>
        public string ToString(string name, string def = null)
        {
            return string.IsNullOrWhiteSpace(GetValue(name)) ? def : GetValue(name).Trim();
        }
        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>

        public byte ToByte(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return 0;
            }

            if (byte.TryParse(GetValue(name), out var vl)) return vl;
            AddMessage(name, "值无法转为数字");
            Failed = true;
            return 0;

        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public byte? ToNullByte(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return null;
            }

            if (!byte.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return null;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public sbyte ToSByte(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return 0;
            }

            if (!sbyte.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public sbyte? ToNullSByte(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return null;
            }

            if (!sbyte.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return null;
            }

            return vl;
        }
        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public long ToLong(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return 0;
            }

            if (!long.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">为空时的默认值</param>
        /// <returns>值</returns>
        public long ToLong(string name, long def)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return def;
            if (!long.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public long? ToNullLong(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return null;
            if (!long.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return null;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="canNull">能否为空</param>
        /// <returns>值</returns>
        public uint ToUInteger(string name, bool canNull = true)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                if (!canNull)
                {
                    AddMessage(name, "值不能为空");
                    Failed = true;
                }

                return 0;
            }

            if (!uint.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="canNull">能否为空</param>
        /// <returns>值</returns>
        public int ToInteger(string name, bool canNull = true)
        {
            var val = GetValue(name);
            if (string.IsNullOrWhiteSpace(val))
            {
                if (!canNull)
                {
                    AddMessage(name, "值不能为空");
                    Failed = true;
                }

                return 0;
            }

            if (!int.TryParse(val, out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="procFunc">转换方法</param>
        /// <returns>值</returns>
        public int ToInteger(string name, Func<string, string> procFunc)
        {
            var str = GetValue(name);
            if (string.IsNullOrWhiteSpace(str)) return 0;
            str = procFunc(str);
            if (!int.TryParse(str, out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">为空时的缺省值</param>
        /// <returns>值</returns>
        public int ToInteger(string name, int def)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return def;
            if (!int.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public int? ToNullInteger(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return null;
            if (!int.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为数字");
                Failed = true;
                return null;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public bool ToBoolean(string name)
        {
            var str = GetValue(name);
            if (string.IsNullOrWhiteSpace(str)) return false;
            switch (str.ToLower())
            {
                case "0":
                case "off":
                case "no":
                case "false":
                    return false;
                case "1":
                case "on":
                case "yes":
                case "true":
                    return true;
            }

            Failed = true;
            return false;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">为空时的缺省值</param>
        /// <returns>值</returns>
        public bool ToBoolean(string name, bool def)
        {
            var str = GetValue(name);
            if (string.IsNullOrWhiteSpace(str)) return def;
            switch (str.ToLower())
            {
                case "0":
                case "off":
                case "no":
                case "false":
                    return false;
                case "1":
                case "on":
                case "yes":
                case "true":
                    return true;
            }

            Failed = true;
            return false;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public bool? ToNullBoolean(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return null;
            return ToBoolean(name);
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="canNull">是否可为空时</param>
        /// <returns>值</returns>
        public decimal ToDecimal(string name, bool canNull = true)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                if (!canNull)
                {
                    AddMessage(name, "值不能为空");
                    Failed = true;
                }

                return 0;
            }

            if (!decimal.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">为空时的缺省值</param>
        /// <returns>值</returns>
        public decimal ToDecimal(string name, decimal def)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return def;
            if (!decimal.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public decimal? ToNullDecimal(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return null;
            if (!decimal.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return null;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public Guid ToGuid(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return Guid.Empty;
            }

            if (!Guid.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为GUID");
                Failed = true;
                return Guid.Empty;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">为空时的缺省值</param>
        /// <returns>值</returns>
        public Guid ToGuid(string name, Guid def)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return def;
            if (!Guid.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为GUID");
                Failed = true;
                return Guid.Empty;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public Guid? ToNullGuid(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return null;
            if (!Guid.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为GUID");
                Failed = true;
                return null;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public double ToDouble(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return 0;
            }

            if (!double.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">为空时的缺省值</param>
        /// <returns>值</returns>
        public double ToDouble(string name, double def)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return def;
            if (!double.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public double? ToNullDouble(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return null;
            if (!double.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return null;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public float ToSingle(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return 0;
            }

            if (!float.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">为空时的缺省值</param>
        /// <returns>值</returns>
        public float ToSingle(string name, float def)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return def;
            if (!float.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return 0;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public float? ToNullSingle(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return null;
            if (!float.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为小数");
                Failed = true;
                return null;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public DateTime ToDateTime(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name)))
            {
                AddMessage(name, "值不能为空");
                Failed = true;
                return DateTime.MinValue;
            }

            if (!DateTime.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为日期");
                Failed = true;
                return DateTime.MinValue;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="def">为空时的缺省值</param>
        /// <returns>值</returns>
        public DateTime ToDateTime(string name, DateTime def)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return def;
            if (!DateTime.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为日期");
                Failed = true;
                return def;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public DateTime? ToNullDateTime(string name)
        {
            if (string.IsNullOrWhiteSpace(GetValue(name))) return null;
            if (!DateTime.TryParse(GetValue(name), out var vl))
            {
                AddMessage(name, "值无法转为日期");
                Failed = true;
                return null;
            }

            return vl;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>值</returns>
        public List<int> ToArray(string name)
        {
            var cs = GetValue(name);
            if (string.IsNullOrWhiteSpace(cs)) return null;
            var css = cs.Trim('[', ']').Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            return css.Length > 0 ? css.Select(int.Parse).ToList() : null;
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="parse">转换方法</param>
        /// <returns>值</returns>
        public List<T> ToArray<T>(string name, Func<string, T> parse)
        {
            var cs = GetValue(name);
            if (string.IsNullOrWhiteSpace(cs)) return null;
            var css = cs.Trim('[', ']').Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            return css.Length > 0 ? css.Select(parse).ToList() : null;
        }
    }
}