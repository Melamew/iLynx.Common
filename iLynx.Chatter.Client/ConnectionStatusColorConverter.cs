using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace iLynx.Chatter.Client
{
    public class ConnectionStatusColorConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ConnectedBrushProperty = DependencyProperty.Register(
            "ConnectedBrush", typeof (Brush), typeof (ConnectionStatusColorConverter), new PropertyMetadata(new SolidColorBrush(Colors.Green)));

        public static readonly DependencyProperty DisconnectedBrushProperty = DependencyProperty.Register(
            "DisconnectedBrush", typeof (Brush), typeof (ConnectionStatusColorConverter), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public Brush DisconnectedBrush
        {
            get { return (Brush) GetValue(DisconnectedBrushProperty); }
            set { SetValue(DisconnectedBrushProperty, value); }
        }

        public Brush ConnectedBrush
        {
            get { return (Brush) GetValue(ConnectedBrushProperty); }
            set { SetValue(ConnectedBrushProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return null;
            var connected = (bool) value;
            return connected ? ConnectedBrush : DisconnectedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, ConnectedBrush) ? true : (Equals(value, DisconnectedBrush) ? new bool?(false) : null);
        }
    }
}