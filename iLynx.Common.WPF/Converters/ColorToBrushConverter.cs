using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace iLynx.Common.WPF.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var solid = value as SolidColorBrush;
            if (null != solid)
                return solid.Color;
            throw new NotSupportedException("Cannot get color for the specified brush.");
        }
    }
}
