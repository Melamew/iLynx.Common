using System;
using System.Windows.Media;

namespace iLynx.Common.WPF.Imaging
{
    public struct HsvColor
    {
        public double Hue { get; set; }
        public double Saturation { get; set; }
        public double Value { get; set; }
        public double Alpha { get; set; }

        public HsvColor(double alpha,
                        double hue,
                        double sat,
                        double val)
            : this()
        {
            Alpha = alpha;
            Hue = hue;
            Saturation = sat;
            Value = val;
        }

        public static Color HsvToRgb(HsvColor hsv)
        {
            var h = hsv.Hue;
            if (h >= 360f)
                h = 0f;
            else
                h /= 60f;
            var s = hsv.Saturation;
            var v = hsv.Value;
            if (Math.Abs(s - 0) < float.Epsilon)
            {
                // achromatic (grey)
                return Color.FromScRgb(1f, (float)v, (float)v, (float)v);
            }
            var i = (int)Math.Floor(h);
            var f = h - i;			// factorial part of h
            var p = (v * (1f - s));
            var q = (v * (1f - s * f));
            var t = (v * (1f - s * (1f - f)));
            switch (i)
            {
                case 0:
                    return Color.FromScRgb((float)hsv.Alpha, (float)v, (float)t, (float)p);
                case 1:
                    return Color.FromScRgb((float)hsv.Alpha, (float)q, (float)v, (float)p);
                case 2:
                    return Color.FromScRgb((float)hsv.Alpha, (float)p, (float)v, (float)t);
                case 3:
                    return Color.FromScRgb((float)hsv.Alpha, (float)p, (float)q, (float)v);
                case 4:
                    return Color.FromScRgb((float)hsv.Alpha, (float)t, (float)p, (float)v);
                default:		// case 5:
                    return Color.FromScRgb((float)hsv.Alpha, (float)v, (float)p, (float)q);
            }
        }

        public static HsvColor RgbToHsv(Color color)
        {
            var min = Math.Min(Math.Min(color.ScR, color.ScG), color.ScB);
            var value = Math.Max(Math.Max(color.ScR, color.ScG), color.ScB);
            double saturation;
            if (Math.Abs(value) < double.Epsilon)
                saturation = value;
            else
                saturation = (value - min) / value;
            var delta = value - min;
            var cR = (value - color.ScR) / delta;
            var cG = (value - color.ScG) / delta;
            var cB = (value - color.ScB) / delta;
            double hue;
            if (Math.Abs(color.ScR - value) < double.Epsilon)
                hue = cB - cG;
            else if (Math.Abs(color.ScG - value) < double.Epsilon)
                hue = 2 + (cR - cB);
            else
                hue = 4 + (cG - cR);
            hue *= 60d;
            if (hue < 0) hue = hue + 360;
            return new HsvColor(color.ScA, hue, saturation, value);
        }

        public static implicit operator HsvColor(Color color)
        {
            return RgbToHsv(color);
        }

        public static implicit operator Color(HsvColor color)
        {
            return HsvToRgb(color);
        }

        public static int Binary(HsvColor color)
        {
            // Easiest way to do it currently
            return ((Color)color).Intify();
        }

        public static HsvColor ColorBetween(HsvColor first,
                                            HsvColor second,
                                            double factor)
        {
            return new HsvColor(
                first.Alpha + ((second.Alpha - first.Alpha) * factor),
                first.Hue + ((second.Hue - first.Hue) * factor),
                first.Saturation + ((second.Saturation - first.Saturation) * factor),
                first.Value + ((second.Value - first.Value) * factor)
                );
        }
    }
}