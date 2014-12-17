using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace iLynx.Common.WPF.Controls
{
    public class ColorPicker : Control
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(default(Color), OnSelectedColorChanged));

        public static readonly DependencyProperty BaseColorProperty =
            DependencyProperty.Register("BaseColor", typeof (Color), typeof (ColorPicker), new PropertyMetadata(default(Color)));

        public static readonly DependencyProperty RedProperty =
            DependencyProperty.Register("Red", typeof (byte), typeof (ColorPicker), new PropertyMetadata(default(byte), OnRgbChanged));

        public static readonly DependencyProperty GreenProperty =
            DependencyProperty.Register("Green", typeof(byte), typeof(ColorPicker), new PropertyMetadata(default(byte), OnRgbChanged));

        public static readonly DependencyProperty BlueProperty =
            DependencyProperty.Register("Blue", typeof(byte), typeof(ColorPicker), new PropertyMetadata(default(byte), OnRgbChanged));

        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register("Saturation", typeof(double), typeof(ColorPicker), new PropertyMetadata(default(double), OnHsvChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ColorPicker), new PropertyMetadata(default(double), OnHsvChanged));

        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(default(double), OnHsvChanged));

        public static readonly DependencyProperty IsManualInputEnabledProperty =
            DependencyProperty.Register("IsManualInputEnabled", typeof(bool), typeof(ColorPicker), new PropertyMetadata(default(bool)));

        public ColorPicker()
        {
            DefaultStyleKey = typeof (ColorPicker);
        }

        public bool IsManualInputEnabled
        {
            get { return (bool)GetValue(IsManualInputEnabledProperty); }
            set { SetValue(IsManualInputEnabledProperty, value); }
        }

        private static void OnRgbChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var picker = dependencyObject as ColorPicker;
            if (null == picker) return;
            if (picker.converting) return;
            picker.SelectedColor = Color.FromArgb(255, picker.Red, picker.Green, picker.Blue);
        }

        private static void OnSelectedColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var picker = dependencyObject as ColorPicker;
            if (null == picker) return;
            if (picker.converting)
            {
                picker.Red = picker.SelectedColor.R;
                picker.Green = picker.SelectedColor.G;
                picker.Blue = picker.SelectedColor.B;
                return;
            }
            picker.converting = true;
            var hsv = RgbToHsv(picker.SelectedColor);
            picker.Hue = hsv.Item1;
            picker.Saturation = hsv.Item2;
            picker.Value = hsv.Item3;
            picker.BaseColor = HsvToRgb(new Tuple<float, float, float>((float) picker.Hue, 1.0f, 1.0f));
            picker.converting = false;
        }

        private static void OnHsvChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var picker = dependencyObject as ColorPicker;
            if (null == picker) return;
            if (picker.converting) return;
            picker.converting = true;
            picker.SelectedColor= HsvToRgb(new Tuple<float, float, float>((float) picker.Hue, (float) picker.Saturation, (float) picker.Value));
            picker.BaseColor = HsvToRgb(new Tuple<float, float, float>((float)picker.Hue, 1.0f, 1.0f));
            picker.converting = false;
        }

        private bool converting;
        public byte Blue
        {
            get { return (byte)GetValue(BlueProperty); }
            set { SetValue(BlueProperty, value); }
        }

        public byte Green
        {
            get { return (byte)GetValue(GreenProperty); }
            set { SetValue(GreenProperty, value); }
        }

        public byte Red
        {
            get { return (byte)GetValue(RedProperty); }
            set { SetValue(RedProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        private static Color HsvToRgb(Tuple<float, float, float> hsv)
        {
            var h = hsv.Item1;
            if (h >= 360f)
                h = 0f;
            else
                h /= 60f;
            var s = hsv.Item2;
            var v = hsv.Item3;
            if (Math.Abs(s - 0) < float.Epsilon)
            {
                // achromatic (grey)
                return Color.FromScRgb(1f, v, v, v);
            }
            var i = (int)Math.Floor(h);
            var f = h - i;			// factorial part of h
            var p = (v * (1f - s));
            var q = (v * (1f - s * f));
            var t = (v * (1f - s * (1f - f)));
            switch (i)
            {
                case 0:
                    return Color.FromScRgb(1f, v, t, p);
                case 1:
                    return Color.FromScRgb(1f, q, v, p);
                case 2:
                    return Color.FromScRgb(1f, p, v, t);
                case 3:
                    return Color.FromScRgb(1f, p, q, v);
                case 4:
                    return Color.FromScRgb(1f, t, p, v);
                default:		// case 5:
                    return Color.FromScRgb(1f, v, p, q);
            }
        }

        private static Tuple<double, double, double> RgbToHsv(Color color)
        {
            var min = Math.Min(Math.Min(color.ScR, color.ScG), color.ScB);
            var value = Math.Max(Math.Max(color.ScR, color.ScG), color.ScB);
            var saturation = 0d;
            if (Math.Abs(value) < double.Epsilon)
                saturation = value;
            else
                saturation = (value - min)/value;
            var delta = value - min;
            var cR = (value - color.ScR)/delta;
            var cG = (value - color.ScG)/delta;
            var cB = (value - color.ScB)/delta;
            double hue;
            if (Math.Abs(color.ScR - value) < double.Epsilon)
                hue = cB - cG;
            else if (Math.Abs(color.ScG - value) < double.Epsilon)
                hue = 2 + (cR - cB);
            else
                hue = 4 + (cG - cR);
            hue *= 60d;
            if (hue < 0) hue = hue + 360;
            return new Tuple<double, double, double>(hue, saturation, value);
        }

        /*
         //  Value:
V = max(R,G,B);
//  Saturation:
temp = min(R,G,B);
if V = 0 then // achromatics case
    S = 0//          H = 0
else // chromatics case
    S = (V - temp)/V
//  Hue:
Cr = (V - R) / (V - temp)
Cg = (V - G) / (V - temp)
Cb = (V - B) / (V - temp)
if R = V then H = Cb - Cg
if G = V then H = 2 + Cr - Cb
if B = V then H = 4 + Cg - Cr
H = 60*H
if H < 0 then H = H + 360*/

        public Color BaseColor
        {
            get { return (Color)GetValue(BaseColorProperty); }
            set { SetValue(BaseColorProperty, value); }
        }

        /*float min, max, delta;
	min = MIN( r, g, b );
	max = MAX( r, g, b );
	*v = max;				// v
	delta = max - min;
	if( max != 0 )
		*s = delta / max;		// s
	else {
		// r = g = b = 0		// s = 0, v is undefined
		*s = 0;
		*h = -1;
		return;
	}
	if( r == max )
		*h = ( g - b ) / delta;		// between yellow & magenta
	else if( g == max )
		*h = 2 + ( b - r ) / delta;	// between cyan & yellow
	else
		*h = 4 + ( r - g ) / delta;	// between magenta & cyan
	*h *= 60;				// degrees
	if( *h < 0 )
		*h += 360;*/
    }
}
