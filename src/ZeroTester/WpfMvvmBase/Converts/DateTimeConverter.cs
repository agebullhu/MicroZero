// 整理人员：bull
// 整理日期：2012－01－08 22:45

using System;
using System.Globalization;
using System.Windows.Data;

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///   布尔到可视的转换
    /// </summary>
    public class EmptyNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal val;
            if(value is decimal)
                val = (decimal)value;
            else if (value is int)
                val = (int)value;
            else if (value is long)
                val = (long)value;
            else
                return value;
            return val==0 ? "" : val.ToString("F2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    /// <summary>
    ///   日期时间转换
    /// </summary>
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
                return value;
            var date = (DateTime) value;
            if (date.Year <= 2000)
            {
                return "";
            }
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DateTime.MinValue;
            if (value is DateTime)
                return value;
            if (value is string)
            {
                if (DateTime.TryParse((string)value, out DateTime date))
                    return date;
            }
            return DateTime.MinValue;
        }
    }
    /// <summary>
    ///   布尔到可视的转换
    /// </summary>
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
                return value;
            var date = (DateTime)value;
            if (date.Year <= 2000)
            {
                return "";
            }
            return date.ToString("yyyy-MM-dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DateTime.MinValue;
            if (value is DateTime)
                return value;
            if (value is string)
            {
                if (DateTime.TryParse((string)value, out DateTime date))
                    return date;
            }
            return DateTime.MinValue;
        }
    }
    /// <summary>
    ///   布尔到可视的转换
    /// </summary>
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
                return value;
            var date = (DateTime)value;
            if (date.Year <= 2000)
            {
                return "";
            }
            return date.ToString("HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DateTime.MinValue;
            if (value is DateTime)
                return value;
            if (value is string)
            {
                if (DateTime.TryParse((string)value, out DateTime date))
                    return date;
            }
            return DateTime.MinValue;
        }
    }
}
