using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace iLynx.Common.Pixels
{
    /// <summary>
    /// InterpolatingPalette
    /// </summary>
    public class LinearGradientPalette : NotificationBase, IPalette
    {
        private readonly SortedList<double, int> colourMap = new SortedList<double, int>();
        private double[] sortedKeys;
        private readonly bool isFrozen;
        private readonly object writeLock = new object();

        public LinearGradientPalette()
        {
            MaxValue = int.MinValue;
            MinValue = int.MaxValue;
        }

        protected LinearGradientPalette(SortedList<double, int> values, bool isFrozen = true)
        {
            this.isFrozen = isFrozen;
            colourMap = values;
            MinValue = colourMap.Keys.Min();
            MaxValue = colourMap.Keys.Max();
            sortedKeys = colourMap.Keys.ToArray();
        }

        public void RemoveValue(double sampleValue)
        {
            if (isFrozen) throw new InvalidOperationException("This instance is frozen, it cannot be modified");
            lock (writeLock)
            {
                colourMap.Remove(sampleValue);
                if (colourMap.Count != 0)
                {
                    if (Math.Abs(sampleValue - MaxValue) <= double.Epsilon)
                        MaxValue = colourMap.Keys.Max();
                    if (Math.Abs(sampleValue - MinValue) <= double.Epsilon)
                        MinValue = colourMap.Keys.Min();
                }
                sortedKeys = colourMap.Keys.ToArray();
            }
        }

        public void RemapValue(double oldValue, double newValue)
        {
            if (isFrozen) throw new InvalidOperationException("This instance is frozen, it cannot be modified");
            lock (writeLock)
            {
                if (!colourMap.ContainsKey(oldValue)) return;
                var colour = colourMap[oldValue];
                colourMap.Remove(oldValue);
                MinValue = Math.Min(newValue, MinValue);
                MaxValue = Math.Max(newValue, MaxValue);

                colourMap.Add(newValue, colour);
                if (Math.Abs(oldValue - MaxValue) <= double.Epsilon)
                    MaxValue = colourMap.Keys.Max();
                if (Math.Abs(oldValue - MinValue) <= double.Epsilon)
                    MinValue = colourMap.Keys.Min();
                sortedKeys = colourMap.Keys.ToArray();
            }
        }

        public unsafe void MapValue(double sampleValue, byte[] colour)
        {
            fixed (byte* col = colour)
                MapValue(sampleValue, *((int*)col)); // Direct conversion
        }

        private double minValue;
        public double MinValue
        {
            get { return minValue; }
            private set
            {
                if (Math.Abs(value - minValue) <= double.Epsilon) return;
                minValue = value;
                RaisePropertyChanged(() => MinValue);
            }
        }

        private double maxValue;
        public double MaxValue
        {
            get { return maxValue; }
            private set
            {
                if (Math.Abs(value - maxValue) <= double.Epsilon) return;
                maxValue = value;
                RaisePropertyChanged(() => MaxValue);
            }
        }

        public void MapValue(double sampleValue, byte a, byte r, byte g, byte b)
        {
            MapValue(sampleValue, new[] { b, g, r, a });
        }

        public void MapValue(double sampleValue, int colour)
        {
            if (isFrozen) throw new InvalidOperationException("This instance is frozen, it cannot be modified");
            lock (writeLock)
            {
                MaxValue = Math.Max(MaxValue, sampleValue);
                MinValue = Math.Min(MinValue, sampleValue);
                if (colourMap.ContainsKey(sampleValue))
                    colourMap[sampleValue] = colour;
                else
                    colourMap.Add(sampleValue, colour);
                sortedKeys = colourMap.Keys.ToArray();
            }
        }

        public unsafe int GetColour(double sampleValue)
        {
            double min, max;

            FindSamples(sampleValue, out min, out max);

            if (Math.Abs(min - max) <= double.Epsilon)
            {
                var val = colourMap[min];
                return val;
            }
            var f = colourMap[min];
            var s = colourMap[max];
            var first = (byte*)&f;
            var second = (byte*)&s;
            var res = new byte[4];
            res[0] = (byte)InterpolateLinear(sampleValue, min, max, first[0], second[0]);
            res[1] = (byte)InterpolateLinear(sampleValue, min, max, first[1], second[1]);
            res[2] = (byte)InterpolateLinear(sampleValue, min, max, first[2], second[2]);
            res[3] = (byte)InterpolateLinear(sampleValue, min, max, first[3], second[3]);

            fixed (byte* p = res)
            {
                var colour = *((int*)p); // As if by magic.
                return colour;
            }
        }

        [TargetedPatchingOptOut("")]
        public static double InterpolateLinear(double x, double x0, double x1, double y0, double y1)
        {
            return y0 + ((x - x0) * (y1 - y0)) / (x1 - x0);
        }

        public void FindSamples(double mean, out double min, out double max)
        {
            min = 0;
            max = min;
            if (null == sortedKeys) return;
            FindSamples(sortedKeys, mean, out min, out max);
        }

        private static void FindSamples(double[] samples, double mean, out double min, out double max)
        {
            var index = Array.BinarySearch(samples, mean);
            if (index < 0)
            {
                index = ~index;
                if (index >= samples.Length)
                {
                    min = samples[samples.Length - 1];
                    max = min;
                }
                else
                {
                    max = samples[index];
                    min = index <= 0 ? max : samples[index - 1];
                }
            }
            else
            {
                min = samples[index];
                max = min;
            }
        }
    }
}