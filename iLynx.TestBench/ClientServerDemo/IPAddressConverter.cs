using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace iLynx.TestBench.ClientServerDemo
{
    public class IPAddressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return null == str ? null : IPAddress.Parse(str);
        }
    }
}
