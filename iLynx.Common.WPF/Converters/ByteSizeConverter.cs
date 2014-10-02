using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Conv = System.Convert;

namespace iLynx.Common.WPF.Converters
{
    public class ByteSizeConverter : IValueConverter
    {
        private static readonly string[] Sizes =
        {
            "B",
            "KiB",
            "MiB",
            "GiB",
            "TiB",
            "PiB",
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var bytes = Conv.ToDouble(value);
                var iterations = 0;
                var size = Sizes[iterations];
                while (bytes > 1024d && ++iterations < Sizes.Length)
                    bytes /= 1024d;
                return string.Format("{0:F2} {1}", bytes, size);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val;
            if (null == (val = value as string)) return null;
            var split = val.Split(' ');
            var stringValue = split.FirstOrDefault();
            if (null == stringValue) return null;
            double doubleValue;
            if (!double.TryParse(stringValue, out doubleValue)) return null;
            var sizeSpecifier = split.Skip(1).FirstOrDefault();
            if (null == sizeSpecifier) return null;
            var specifier = Array.IndexOf(Sizes, sizeSpecifier);
            if (-1 == specifier) return null;
            for (var i = 0; i < specifier; ++i)
                doubleValue *= 1024d;
            return doubleValue;
        }
    }
}
