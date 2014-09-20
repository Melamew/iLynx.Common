using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace iLynx.Common.WPF.Controls
{
    public class ValuePicker : ContentControl
    {
        public static readonly DependencyProperty XMinimumProperty =
            DependencyProperty.Register("XMinimum", typeof(double), typeof(ValuePicker), new PropertyMetadata(default(double), OnValueChanged));

        public static readonly DependencyProperty XMaximumProperty =
            DependencyProperty.Register("XMaximum", typeof(double), typeof(ValuePicker), new PropertyMetadata(default(double), OnValueChanged));

        public static readonly DependencyProperty YMinimumProperty =
            DependencyProperty.Register("YMinimum", typeof(double), typeof(ValuePicker), new PropertyMetadata(default(double), OnValueChanged));

        public static readonly DependencyProperty YMaximumProperty =
            DependencyProperty.Register("YMaximum", typeof(double), typeof(ValuePicker), new PropertyMetadata(default(double), OnValueChanged));

        public static readonly DependencyPropertyKey XPositionPropertyKey = DependencyProperty.RegisterReadOnly("XPosition", typeof(double), typeof(ValuePicker), new PropertyMetadata(0d, OnPositionChanged));

        public static readonly DependencyPropertyKey YPositionPropertyKey = DependencyProperty.RegisterReadOnly("YPosition", typeof(double), typeof(ValuePicker), new PropertyMetadata(0d, OnPositionChanged));

        public static readonly DependencyProperty IsXReversedProperty =
            DependencyProperty.Register("IsXReversed", typeof(bool), typeof(ValuePicker), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsYReversedProperty =
            DependencyProperty.Register("IsYReversed", typeof(bool), typeof(ValuePicker), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsYEnabledProperty =
            DependencyProperty.Register("IsYEnabled", typeof(bool), typeof(ValuePicker), new PropertyMetadata(true));

        public static readonly DependencyProperty IsXEnabledProperty =
            DependencyProperty.Register("IsXEnabled", typeof(bool), typeof(ValuePicker), new PropertyMetadata(true));

        public static readonly DependencyProperty PointerTemplateProperty =
            DependencyProperty.Register("PointerTemplate", typeof(ControlTemplate), typeof(ValuePicker), new PropertyMetadata(default(ControlTemplate)));

        public static readonly DependencyProperty XValueProperty =
            DependencyProperty.Register("XValue", typeof(double), typeof(ValuePicker), new PropertyMetadata(default(double), OnValueChanged));

        public static readonly DependencyProperty YValueProperty =
            DependencyProperty.Register("YValue", typeof(double), typeof(ValuePicker), new PropertyMetadata(default(double), OnValueChanged));

        public ValuePicker()
        {
            DefaultStyleKey = typeof (ValuePicker);
        }

        public ControlTemplate PointerTemplate
        {
            get { return (ControlTemplate) GetValue(PointerTemplateProperty); }
            set { SetValue(PointerTemplateProperty, value); }
        }

        public bool IsXEnabled
        {
            get { return (bool)GetValue(IsXEnabledProperty); }
            set { SetValue(IsXEnabledProperty, value); }
        }

        public bool IsYEnabled
        {
            get { return (bool)GetValue(IsYEnabledProperty); }
            set { SetValue(IsYEnabledProperty, value); }
        }

        public bool IsYReversed
        {
            get { return (bool)GetValue(IsYReversedProperty); }
            set { SetValue(IsYReversedProperty, value); }
        }

        public bool IsXReversed
        {
            get { return (bool)GetValue(IsXReversedProperty); }
            set { SetValue(IsXReversedProperty, value); }
        }

        private bool isDragging;

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            isDragging = true;
            SetCurrentPosition(e);
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (!isDragging) return;
            SetCurrentPosition(e);
            e.Handled = true;
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            isDragging = false;
            ReleaseMouseCapture();
            e.Handled = true;
        }

        private void SetCurrentPosition(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (pos.X > ActualWidth) pos.X = ActualWidth;
            if (pos.Y > ActualHeight) pos.Y = ActualHeight;
            if (pos.X < 0) pos.X = 0;
            if (pos.Y < 0) pos.Y = 0;
            XPosition = IsXEnabled ? pos.X : 0d;
            YPosition = IsYEnabled ? pos.Y : 0d;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            isDragging = true;
            var pos = ConvertValueToPoint(Value, this);
            XPosition = pos.X;
            YPosition = pos.Y;
            isDragging = false;
        }

        private static void OnPositionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var slider = dependencyObject as ValuePicker;
            if (null == slider) return;
            if (!slider.isDragging) return;
            var val = ConvertPointToValue(slider.Position, slider);
            slider.YValue = val.Y;
            slider.XValue = val.X;
        }

        private static Point ConvertPointToValue(Point p, ValuePicker slider)
        {
            var xRange = Math.Max(slider.XMaximum, slider.XMinimum) - Math.Min(slider.XMaximum, slider.XMinimum);
            var yRange = Math.Max(slider.YMaximum, slider.YMinimum) - Math.Min(slider.YMaximum, slider.YMinimum);
            var xValue = (slider.IsXReversed ? slider.ActualWidth - p.X : p.X) * (xRange / slider.ActualWidth);
            var yValue = (slider.IsYReversed ? slider.ActualHeight - p.Y : p.Y) * (yRange / slider.ActualHeight);
            return new Point(slider.IsXEnabled ? xValue : 0d, slider.IsYEnabled ? yValue : 0d);
        }

        private static Point ConvertValueToPoint(Point v, ValuePicker slider)
        {
            var xRange = Math.Max(slider.XMaximum, slider.XMinimum) - Math.Min(slider.XMaximum, slider.XMinimum);
            var yRange = Math.Max(slider.YMaximum, slider.YMinimum) - Math.Min(slider.YMaximum, slider.YMinimum);
            var xPoint = v.X / (xRange / slider.ActualWidth);
            var yPoint = v.Y / (yRange / slider.ActualHeight);
            xPoint = slider.IsXReversed ? slider.ActualWidth - xPoint : xPoint;
            yPoint = slider.IsYReversed ? slider.ActualHeight - yPoint : yPoint;
            return new Point(
                slider.IsXEnabled ? xPoint : 0d,
                slider.IsYEnabled ? yPoint : 0d);
        }

        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var slider = dependencyObject as ValuePicker;
            if (null == slider) return;
            Clamp(slider);
            if (slider.isDragging)
                return;
            var point = ConvertValueToPoint(slider.Value, slider);
            slider.XPosition = point.X;
            slider.YPosition = point.Y;
        }

        private static void Clamp(ValuePicker slider)
        {
            var xMin = Math.Min(slider.XMinimum, slider.XMinimum);
            var xMax = Math.Max(slider.XMaximum, slider.XMinimum);
            if (slider.XValue > xMax)
                slider.XValue = xMax;
            if (slider.XValue < xMin)
                slider.XValue = xMin;
            var yMin = Math.Min(slider.YMaximum, slider.YMinimum);
            var yMax = Math.Max(slider.YMaximum, slider.YMinimum);
            if (slider.YValue > yMax)
                slider.YValue = yMax;
            if (slider.YValue < yMin)
                slider.YValue = yMin;
        }

        public Point Position
        {
            get { return new Point(XPosition, YPosition); }
        }

        public Point Value
        {
            get { return new Point(XValue, YValue); }
        }

        public double XPosition
        {
            get { return (double)GetValue(XPositionPropertyKey.DependencyProperty); }
            private set { SetValue(XPositionPropertyKey, value); }
        }

        public double YPosition
        {
            get { return (double)GetValue(YPositionPropertyKey.DependencyProperty); }
            private set { SetValue(YPositionPropertyKey, value); }
        }

        public double YMaximum
        {
            get { return (double)GetValue(YMaximumProperty); }
            set { SetValue(YMaximumProperty, value); }
        }

        public double YMinimum
        {
            get { return (double)GetValue(YMinimumProperty); }
            set { SetValue(YMinimumProperty, value); }
        }

        public double XMaximum
        {
            get { return (double)GetValue(XMaximumProperty); }
            set { SetValue(XMaximumProperty, value); }
        }

        public double XMinimum
        {
            get { return (double)GetValue(XMinimumProperty); }
            set { SetValue(XMinimumProperty, value); }
        }

        public double YValue
        {
            get { return (double)GetValue(YValueProperty); }
            set { SetValue(YValueProperty, value); }
        }

        public double XValue
        {
            get { return (double)GetValue(XValueProperty); }
            set { SetValue(XValueProperty, value); }
        }
    }
}
