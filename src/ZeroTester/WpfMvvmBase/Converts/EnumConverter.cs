// 整理人员：bull
// 整理日期：2012－01－08 22:45

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Agebull.Common.Reflection;

#if !CLIENT
#endif

#endregion

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///   表示一个枚举的值(转换为long)文本对应表节点
    /// </summary>
    public class EnumInfomation
    {
        /// <summary>
        ///   文本
        /// </summary>
        public string Caption
        {
            get;
            set;
        }

        /// <summary>
        ///   内容
        /// </summary>
        public long Value
        {
            get;
            set;
        }
    }

    /// <summary>
    ///   枚举的转换器
    /// </summary>
    public class EnumToStringConverter<TE> : IValueConverter
            where TE : struct
    {
        /// <summary>
        ///   枚举对应值
        /// </summary>
        private static IEnumerable<EnumInfomation<TE>> _enumValues;

        /// <summary>
        ///   枚举对应值
        /// </summary>
        private static IEnumerable<EnumInfomation<TE>> EnumValues => _enumValues ?? (_enumValues = EnumHelper.KeyValue<TE>());

        #region IValueConverter Members

        /// <summary>
        ///   到文本
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="targetType"> </param>
        /// <param name="parameter"> </param>
        /// <param name="culture"> </param>
        /// <returns> </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "无";
            }
            string sValue = value.ToString().Trim();

            if (string.IsNullOrWhiteSpace(sValue))
                return "无";

            string[] sWords = sValue.Split(',', ' ', ';', '.', '\r', '\n', '\t');

            StringBuilder sb = new StringBuilder();

            foreach (var word in sWords.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                EnumInfomation<TE> ei = EnumValues.FirstOrDefault(p => p.Value.ToString() == word);
                if (ei == null)
                    continue;
                if (sb.Length > 0)
                {
                    sb.Append(" , ");
                }
                sb.Append(ei.Caption);
            }
            return sb.ToString();
        }

        /// <summary>
        ///   到值
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="targetType"> </param>
        /// <param name="parameter"> </param>
        /// <param name="culture"> </param>
        /// <returns> </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return default(TE);
            }
            string sValue = value.ToString().Trim();

            if (string.IsNullOrWhiteSpace(sValue) || sValue == "无")
                return default(TE);

            string[] sWords = sValue.Split(',', ' ', ';', '.', '\r', '\n', '\t');

            StringBuilder sb = new StringBuilder();
            foreach (var word in sWords.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                EnumInfomation<TE> ei = EnumValues.FirstOrDefault(p => p.Caption == word);
                if (ei == null)
                    continue;
                if (sb.Length > 0)
                {
                    sb.Append(" , ");
                }
                sb.Append(ei.Value);
            }
            try
            {
                return Enum.Parse(typeof(TE), sb.ToString(), false);
            }
            catch
            {
                return default(TE);
            }
        }

        #endregion
    }

    /// <summary>
    ///  从真转到可视
    /// </summary>
    public class TrueToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        ///   到文本
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="targetType"> </param>
        /// <param name="parameter"> </param>
        /// <param name="culture"> </param>
        /// <returns> </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool v = false;
            if (value != null)
            {
                bool.TryParse(value.ToString(), out v);
            }
            return v ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        ///   到值
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="targetType"> </param>
        /// <param name="parameter"> </param>
        /// <param name="culture"> </param>
        /// <returns> </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }

        #endregion
    }
    /// <summary>
    ///   从真转到不可视
    /// </summary>
    public class FalseToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        ///   到文本
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="targetType"> </param>
        /// <param name="parameter"> </param>
        /// <param name="culture"> </param>
        /// <returns> </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool v = false;
            if (value != null)
            {
                bool.TryParse(value.ToString(), out v);
            }
            return v ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        ///   到值
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="targetType"> </param>
        /// <param name="parameter"> </param>
        /// <param name="culture"> </param>
        /// <returns> </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is Visibility) || (Visibility)value == Visibility.Collapsed;
        }

        #endregion
    }
    /// <summary>
    ///   枚举的转换器
    /// </summary>
    public class EnumToLongConverter<TE> : IValueConverter
            where TE : struct
    {
        /// <summary>
        ///   枚举对应值
        /// </summary>
        private static IEnumerable<EnumInfomation<TE>> _enumValues;

        /// <summary>
        ///   枚举对应值
        /// </summary>
        public static IEnumerable<EnumInfomation<TE>> EnumValues => _enumValues ?? (_enumValues = EnumHelper.KeyValue<TE>());

        #region IValueConverter Members

        /// <summary>
        ///   到文本
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="targetType"> </param>
        /// <param name="parameter"> </param>
        /// <param name="culture"> </param>
        /// <returns> </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt64(value);
        }

        /// <summary>
        ///   到值
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="targetType"> </param>
        /// <param name="parameter"> </param>
        /// <param name="culture"> </param>
        /// <returns> </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return default( TE );
            }
            long l = (long)value;
            if (l <= 0)
                return default( TE );

            StringBuilder sb = new StringBuilder();
            foreach (var ev in EnumValues)
            {
                if(( l & ev.LValue) != ev.LValue)
                    continue;
                if (sb.Length > 0)
                {
                    sb.Append(" , ");
                }
                sb.Append(ev.Value);
            }
            try
            {
                return Enum.Parse(typeof(TE), sb.ToString(), false);
            }
            catch
            {
                return default(TE);
            }
        }

        #endregion
    }

    public class FileTypeToStringConverter : EnumToStringConverter<FileType>
    {
    }

    /// <summary>
    ///   文本类型到整数的转换
    /// </summary>
    public class FileTypeToLongConverter : EnumToLongConverter<FileType>
    {
    }

    
}
