// // /*****************************************************
// // (c)2016-2016 Copy right www.gboxt.com
// // 作者:
// // 工程:Agebull.DataModel
// // 建立:2016-06-12
// // 修改:2016-06-16
// // *****************************************************/

#region 引用

using Agebull.EntityModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     FORM对象转化辅助类
    /// </summary>
    public class FormConvert
    {
        #region 基本属性

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="controler"></param>
        /// <param name="data"></param>
        public FormConvert(ApiControlerEx controler, EditDataObject data)
        {
            Controler = controler;
            Data = data;
        }
        /// <summary>
        ///     读取过程的错误消息记录
        /// </summary>
        public ApiControlerEx Controler { get; }

        /// <summary>
        ///     读取过程的错误消息记录
        /// </summary>
        public EditDataObject Data { get; }

        /// <summary>
        ///     是否更新状态
        /// </summary>
        public bool IsUpdata { get; set; }

        /// <summary>
        ///     是否发生解析错误
        /// </summary>
        public bool Failed { get; set; }


        /// <summary>
        ///     是否发生解析错误
        /// </summary>
        public string Message
        {
            get
            {
                StringBuilder msg = new StringBuilder();
                foreach (var kv in _messages)
                {
                    var field = Data.__Struct.Properties.Values.FirstOrDefault(p => p.Name == kv.Key)?.Caption ?? kv.Key;
                    msg.AppendLine($"{field} : {kv.Value}<br/>");
                }
                return msg.ToString();
            }
        }

        /// <summary>
        ///     字段
        /// </summary>
        private readonly Dictionary<string, string> _messages = new Dictionary<string, string>();

        /// <summary>
        /// 设置错误字段
        /// </summary>
        /// <param name="field"></param>
        /// <param name="msg"></param>
        private void AddMessage(string field, string msg)
        {
            if (_messages.TryGetValue(field, out var val))
                _messages[field] = $"{val};{msg}";
            else
                _messages.Add(field, msg);
        }
        #endregion

        #region 新方法 

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out string value)
        {
            if (!Controler.Arguments.TryGetValue(field, out value))
            {
                return false;
            }
            value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            return true;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out byte value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (byte.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = 0;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out byte? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (byte.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out sbyte value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (sbyte.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = 0;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out sbyte? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (sbyte.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out short value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (short.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = 0;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out short? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (short.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out ushort value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (ushort.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = 0;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out ushort? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (ushort.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }
        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out bool value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = false;
                return false;
            }

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
            }
            if (!bool.TryParse(str, out var vl))
            {
                AddMessage(field, "参数值转换出错");
                Failed = true;
                value = false;
                return false;
            }
            value = vl;
            return true;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out bool? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }
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
            }
            if (bool.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = null;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out int value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (int.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = 0;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out int? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (int.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = null;
            return false;
        }
        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out uint value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (uint.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = 0;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out uint? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (uint.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = null;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out long value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (long.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = 0;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out long? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (long.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out ulong value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (ulong.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = 0;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out ulong? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (ulong.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out float value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (float.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = 0;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out float? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (float.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out double value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0;
                return false;
            }
            if (double.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = 0;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out double? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (double.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out DateTime value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = DateTime.MinValue;
                return false;
            }
            if (DateTime.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = DateTime.MinValue;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out DateTime? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (DateTime.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out decimal value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = 0M;
                return false;
            }
            if (decimal.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = 0M;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out decimal? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }

            if (decimal.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = null;
            Failed = true;
            AddMessage(field, "参数值转换出错");
            return false;
        }
        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out Guid value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = Guid.Empty;
                return false;
            }
            if (Guid.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            value = Guid.Empty;
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out Guid? value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (str == "null")
            {
                value = null;
                return true;
            }
            if (Guid.TryParse(str, out var vl))
            {
                value = vl;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = null;
            return false;
        }


        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out List<int> value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (Controler.TryGet(field, out List<int> ids))
            {
                value = ids;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = null;
            return false;
        }
        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out List<long> value)
        {
            if (!Controler.Arguments.TryGetValue(field, out var str) || string.IsNullOrWhiteSpace(str))
            {
                value = null;
                return false;
            }
            if (Controler.TryGetIDs(field, out var ids))
            {
                value = ids;
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            value = null;
            return false;
        }

        #endregion
    }
}

#region 旧方法
/*
private string GetValue(string field)
{
    return !Controler.Arguments.TryGetValue(field, out var val) ? null : val?.Trim();
}

/// <summary>
/// 到文本
/// </summary>
/// <param name="field"></param>
/// <param name="canNull"></param>
/// <returns></returns>
public string ToString(string field, bool canNull)
{
    if (!string.IsNullOrWhiteSpace(GetValue(field)))
        return GetValue(field).Trim();
    //if (!canNull)
    //{
    //    AddMessage(field, "值不能为空");
    //    this.Failed = true;
    //}
    return null;
}
/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">找不到或为空时的默认值</param>
/// <returns>值</returns>
public string ToString(string field, string def = null)
{
    return string.IsNullOrWhiteSpace(GetValue(field)) ? def : GetValue(field).Trim();
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>

public byte ToByte(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return 0;
    }

    if (byte.TryParse(GetValue(field), out var vl)) return vl;
    AddMessage(field, "参数值转换出错");
    Failed = true;
    return 0;

}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public byte? ToNullByte(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return null;
    }

    if (!byte.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return null;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public sbyte ToSByte(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return 0;
    }

    if (!sbyte.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public sbyte? ToNullSByte(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return null;
    }

    if (!sbyte.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return null;
    }

    return vl;
}
/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public long ToLong(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return 0;
    }

    if (!long.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">为空时的默认值</param>
/// <returns>值</returns>
public long ToLong(string field, long def)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return def;
    if (!long.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public long? ToNullLong(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return null;
    if (!long.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return null;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="canNull">能否为空</param>
/// <returns>值</returns>
public uint ToUInteger(string field, bool canNull = true)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        if (!canNull)
        {
            AddMessage(field, "值不能为空");
            Failed = true;
        }

        return 0;
    }

    if (!uint.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="canNull">能否为空</param>
/// <returns>值</returns>
public int ToInteger(string field, bool canNull = true)
{
    var val = GetValue(field);
    if (string.IsNullOrWhiteSpace(val))
    {
        if (!canNull)
        {
            AddMessage(field, "值不能为空");
            Failed = true;
        }

        return 0;
    }

    if (!int.TryParse(val, out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="procFunc">转换方法</param>
/// <returns>值</returns>
public int ToInteger(string field, Func<string, string> procFunc)
{
    var str = GetValue(field);
    if (string.IsNullOrWhiteSpace(str)) return 0;
    str = procFunc(str);
    if (!int.TryParse(str, out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">为空时的缺省值</param>
/// <returns>值</returns>
public int ToInteger(string field, int def)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return def;
    if (!int.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public int? ToNullInteger(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return null;
    if (!int.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "参数值转换出错");
        Failed = true;
        return null;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public bool ToBoolean(string field)
{
    var str = GetValue(field);
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
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">为空时的缺省值</param>
/// <returns>值</returns>
public bool ToBoolean(string field, bool def)
{
    var str = GetValue(field);
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
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public bool? ToNullBoolean(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return null;
    return ToBoolean(field);
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="canNull">是否可为空时</param>
/// <returns>值</returns>
public decimal ToDecimal(string field, bool canNull = true)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        if (!canNull)
        {
            AddMessage(field, "值不能为空");
            Failed = true;
        }

        return 0;
    }

    if (!decimal.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">为空时的缺省值</param>
/// <returns>值</returns>
public decimal ToDecimal(string field, decimal def)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return def;
    if (!decimal.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public decimal? ToNullDecimal(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return null;
    if (!decimal.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return null;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public Guid ToGuid(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return Guid.Empty;
    }

    if (!Guid.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为GUID");
        Failed = true;
        return Guid.Empty;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">为空时的缺省值</param>
/// <returns>值</returns>
public Guid ToGuid(string field, Guid def)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return def;
    if (!Guid.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为GUID");
        Failed = true;
        return Guid.Empty;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public Guid? ToNullGuid(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return null;
    if (!Guid.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为GUID");
        Failed = true;
        return null;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public double ToDouble(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return 0;
    }

    if (!double.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">为空时的缺省值</param>
/// <returns>值</returns>
public double ToDouble(string field, double def)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return def;
    if (!double.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public double? ToNullDouble(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return null;
    if (!double.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return null;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public float ToSingle(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return 0;
    }

    if (!float.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">为空时的缺省值</param>
/// <returns>值</returns>
public float ToSingle(string field, float def)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return def;
    if (!float.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return 0;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public float? ToNullSingle(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return null;
    if (!float.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为小数");
        Failed = true;
        return null;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public DateTime ToDateTime(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field)))
    {
        AddMessage(field, "值不能为空");
        Failed = true;
        return DateTime.MinValue;
    }

    if (!DateTime.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为日期");
        Failed = true;
        return DateTime.MinValue;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="def">为空时的缺省值</param>
/// <returns>值</returns>
public DateTime ToDateTime(string field, DateTime def)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return def;
    if (!DateTime.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为日期");
        Failed = true;
        return def;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public DateTime? ToNullDateTime(string field)
{
    if (string.IsNullOrWhiteSpace(GetValue(field))) return null;
    if (!DateTime.TryParse(GetValue(field), out var vl))
    {
        AddMessage(field, "值无法转为日期");
        Failed = true;
        return null;
    }

    return vl;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <returns>值</returns>
public List<int> ToArray(string field)
{
    var cs = GetValue(field);
    if (string.IsNullOrWhiteSpace(cs)) return null;
    var css = cs.Trim('[', ']').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    return css.Length > 0 ? css.Select(int.Parse).ToList() : null;
}

/// <summary>
/// 字段值转换
/// </summary>
/// <param name="field">字段名称</param>
/// <param name="parse">转换方法</param>
/// <returns>值</returns>
public List<T> ToArray<T>(string field, Func<string, T> parse)
{
    var cs = GetValue(field);
    if (string.IsNullOrWhiteSpace(cs)) return null;
    var css = cs.Trim('[', ']').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    return css.Length > 0 ? css.Select(parse).ToList() : null;
}*/
#endregion