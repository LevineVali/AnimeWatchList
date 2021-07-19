using System;
using System.Windows;
using System.Windows.Data;

namespace AnimeWatchList
{
    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value - (double)60;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class EnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !String.IsNullOrWhiteSpace((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public static class Extensions
    {
        public static T SetVisualTree<T>(this T template, FrameworkElementFactory factory) where T : FrameworkTemplate
        {
            template.VisualTree = factory;

            return template;
        }
    }
}
