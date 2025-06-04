using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PdfBookmarkToolModern.Converters
{
    /// <summary>
    /// アクションタイプに基づいてVisibilityを制御するコンバーター
    /// </summary>
    public class ActionTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string actionType && parameter is string targetActionType)
            {
                return string.Equals(actionType, targetActionType, StringComparison.OrdinalIgnoreCase) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// アクションタイプがGoToRの場合にVisibleにするコンバーター
    /// </summary>
    public class GoToRVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(value?.ToString(), Constants.PDF_ACTION_GOTOR, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// null または空文字列の場合にCollapsedにするコンバーター
    /// </summary>
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value?.ToString()) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 