using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PdfBookmarkToolModern.Converters
{
    /// <summary>
    /// BooleanをVisibilityに変換するコンバーター
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    /// <summary>
    /// レベルからフォントウェイトに変換するコンバーター
    /// </summary>
    public class LevelToFontWeightConverter : IValueConverter
    {
        public static readonly LevelToFontWeightConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int level)
            {
                return level == 1 ? FontWeights.Bold : FontWeights.Normal;
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// nullチェック用のコンバーター
    /// </summary>
    public class NotNullToBooleanConverter : IValueConverter
    {
        public static readonly NotNullToBooleanConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 