using System.Collections.Generic;
using System.Linq;

namespace System
{
    /// <summary>
    /// A few general Extension Methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Gets a string representation of the specified <see name="IEnumerable{byte}"/> using the specified <paramref name="splitter"/> as a "splitter"
        /// </summary>
        /// <param name="val">The <see cref="IEnumerable{T}"/> to stringify</param>
        /// <param name="splitter">The splitter to use between bytes</param>
        /// <returns></returns>
        public static string ToString(this IEnumerable<byte> val, string splitter)
        {
            var ret = val.Aggregate(string.Empty, (current, v) => current + (v.ToString("X2") + splitter));
            // Remove the superflous splitter that was added during the aggregate.
            ret = ret.Remove(ret.Length - splitter.Length, splitter.Length);
            return ret;
        }

        /// <summary>
        /// Combines to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public static string CombineToString<T>(this IEnumerable<T> val)
        {
            return val.Aggregate(string.Empty, (s, arg2) => s + arg2.ToString());
        }

        /// <summary>
        /// Determines whether the specified value is within the specified range (Inclusively!).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>
        ///   <c>true</c> if [is in range] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRange(this short value, short min, short max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Determines whether the specified value is within the specified range (Inclusively!).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>
        ///   <c>true</c> if [is in range] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRange(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Determines whether the specified value is within the specified range (Inclusively!).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>
        ///   <c>true</c> if [is in range] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRange(this long value, long min, long max)
        {
            return value >= min && value <= max;
        }
        /// <summary>
        /// Determines whether the specified value is within the specified range (Inclusively!).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>
        ///   <c>true</c> if [is in range] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRange(this uint value, uint min, uint max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Determines whether the specified value is within the specified range (Inclusively!).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>
        ///   <c>true</c> if [is in range] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRange(this ushort value, ushort min, ushort max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Determines whether the specified value is within the specified range (Inclusively!).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>
        ///   <c>true</c> if [is in range] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRange(this ulong value, ulong min, ulong max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Determines whether [is power of two] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is power of two] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPowerOfTwo(this uint value)
        {
            return (value != 0) && ((value & (value - 1)) == 0);
        }

        /// <summary>
        /// Determines whether [is power of two] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is power of two] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPowerOfTwo(this ulong value)
        {
            return (value != 0) && ((value & (value - 1)) == 0);
        }

        /// <summary>
        /// Determines whether [is power of two] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is power of two] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPowerOfTwo(this ushort value)
        {
            return (value != 0) && ((value & (value - 1)) == 0);
        }

        /// <summary>
        /// Determines whether [is power of two] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is power of two] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPowerOfTwo(this int value)
        {
            return (value > 0) && ((value & (value - 1)) == 0);
        }

        /// <summary>
        /// Determines whether [is power of two] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is power of two] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPowerOfTwo(this long value)
        {
            return (value > 0) && ((value & (value - 1)) == 0);
        }

        /// <summary>
        /// Determines whether [is power of two] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is power of two] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPowerOfTwo(this short value)
        {
            return (value > 0) && ((value & (value - 1)) == 0);
        }

        /// <summary>
        /// Normalizes the specified arr (In place, and returns it).
        /// </summary>
        /// <param name="arr">The arr.</param>
        public static float[] Normalize(this float[] arr, float level = float.NaN)
        {
            if (null == arr) return null;
            // ReSharper disable LoopCanBeConvertedToQuery // Faster like this, ffs
            // ReSharper disable ForCanBeConvertedToForeach // Faster like this, ffs
            // TODO: Maybe find a way to make this go faster?
            if (float.IsNaN(level))
            {
                level = float.MinValue;
                for (var i = 0; i < arr.Length; ++i)
                    level = arr[i] > level ? arr[i] : level;
            }
            for (var i = 0; i < arr.Length; ++i)
                arr[i] /= level;
            return arr;
            // ReSharper restore ForCanBeConvertedToForeach
            // ReSharper restore LoopCanBeConvertedToQuery
        }

        /// <summary>
        /// Transforms the specified arr (In place) and returns it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">The arr.</param>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        public static T[] Transform<T>(this T[] arr, Func<T, T> func)
        {
            for (var i = 0; i < arr.Length; ++i)
                arr[i] = func(arr[i]);
            return arr;
        }

        /// <summary>
        /// Transforms the specified arr.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK">The type of the K.</typeparam>
        /// <param name="arr">The arr.</param>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        public static TK[] Transform<T, TK>(this T[] arr, Func<T, TK> func)
        {
            var res = new TK[arr.Length];
            for (var i = 0; i < arr.Length; ++i)
                res[i] = func(arr[i]);
            return res;
        }

        public static T[] Slice<T>(this T[] arr,
                                   int offset,
                                   int length)
        {
            if (offset + length > arr.Length) throw new ArgumentOutOfRangeException("length");
            var result = new T[length];
            for (var i = offset; i < offset + length; ++i)
                result[i - offset] = arr[i];
            return result;
        }

        /// <summary>
        /// Normalizes the specified arr.
        /// </summary>
        /// <param name="arr">The arr.</param>
        /// <param name="level"></param>
        public static void Normalize(this double[] arr, double level = double.NaN)
        {
            // ReSharper disable LoopCanBeConvertedToQuery // Faster like this, ffs
            // ReSharper disable ForCanBeConvertedToForeach // Faster like this, ffs
            // TODO: Maybe find a way to make this go faster?
            if (double.IsNaN(level))
            {
                level = double.MinValue;
                for (var i = 0; i < arr.Length; ++i)
                    level = arr[i] > level ? arr[i] : level;
            }
            for (var i = 0; i < arr.Length; ++i)
                arr[i] /= level;
            // ReSharper restore ForCanBeConvertedToForeach
            // ReSharper restore LoopCanBeConvertedToQuery
        }

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="range">The range.</param>
        public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> range)
        {
            foreach (var t in range)
                source.Remove(t);
        }

        #region Source: http://stackoverflow.com/questions/609501/generating-a-random-decimal-in-c-sharp
        public static int NextInt32(this Random rng)
        {
            unchecked
            {
                var firstBits = rng.Next(0, 1 << 4) << 28;
                var lastBits = rng.Next(0, 1 << 28);
                return firstBits | lastBits;
            }
        }

        public static decimal NextDecimal(this Random rng)
        {
            var scale = (byte)rng.Next(29);
            var sign = rng.Next(2) == 1;
            return new decimal(rng.NextInt32(),
                               rng.NextInt32(),
                               rng.NextInt32(),
                               sign,
                               scale);
        }
        #endregion
    }
}