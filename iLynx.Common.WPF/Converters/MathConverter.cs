using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace iLynx.Common.WPF.Converters
{
    public class MathConverter : DependencyObject, IValueConverter, IMultiValueConverter
    {
        public enum MathOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
        }

        private static readonly Dictionary<MathOperation, Func<double, double, double>> Aggregators = new Dictionary
            <MathOperation, Func<double, double, double>>()
        {
            {MathOperation.Add, (x, y) => x + y},
            {MathOperation.Subtract, (x, y) => x - y},
            {MathOperation.Multiply, (x, y) => x * y},
            {MathOperation.Divide, (x, y) => x / y},
        };

        public static readonly DependencyProperty OperationProperty = DependencyProperty.Register(
            "Operation", typeof(MathOperation), typeof(MathConverter), new PropertyMetadata(MathOperation.Add));

        public MathOperation Operation
        {
            get { return (MathOperation)GetValue(OperationProperty); }
            set { SetValue(OperationProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double))
                throw new InvalidCastException("The specified source type is not a double");
            if (!(parameter is double))
                throw new InvalidCastException("The specified parameter type is not a double");
            var val = (double)value;
            var other = (double)parameter;
            Func<double, double, double> aggregator;
            if (!Aggregators.TryGetValue(Operation, out aggregator)) throw new InvalidOperationException("Can't do math :'c");
            return aggregator(val, other);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var vals = values.OfType<double>().ToArray();
            if (2 > vals.Length) throw new InvalidOperationException("Cannot do math on less than two values at this time");
            var initial = vals[0];
            Func<double, double, double> aggregator;
            if (!Aggregators.TryGetValue(Operation, out aggregator)) throw new InvalidOperationException("Can't do math :'c");
            return vals
                .Skip(1)
                .Aggregate(initial, aggregator);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return value.Repeat(targetTypes.Length);
        }
    }
}
