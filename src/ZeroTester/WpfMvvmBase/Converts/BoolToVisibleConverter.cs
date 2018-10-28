// 整理人员：bull
// 整理日期：2012－01－08 22:45

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///   布尔到可视的转换
    /// </summary>
    public class NullVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }
    }
    /// <summary>
    ///   布尔到可视的转换
    /// </summary>
    public class BoolAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(bool)value)
            {
                return HorizontalAlignment.Left;
            }
            return HorizontalAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
    
    /// <summary>
    ///   布尔到可视的转换
    /// </summary>
    public class BoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(bool) value)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (Visibility) value == Visibility.Visible;
        }
    }
    /// <summary>
    ///   布尔到可视的转换
    /// </summary>
    public class ReversionVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof (Visibility))
            {
                return value != null && (Visibility) value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (Visibility) value == Visibility.Visible;
        }
    }

    
    /// <summary>
    ///   布尔到可视的转换
    /// </summary>
    public class ReversionBoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (Visibility)value == Visibility.Collapsed;
        }
    }

    /// <summary>
    ///   布尔的反转(真为假假为真)
    /// </summary>
    public class BoolReversalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || !(bool) value;
        }
    }
}
