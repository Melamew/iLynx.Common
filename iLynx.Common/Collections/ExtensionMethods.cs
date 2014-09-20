using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using iLynx.Common;

namespace iLynx.Common.Collections
{
    /// <summary>
    /// ExtensionMethods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Wraps an asynchronous enumerable source around this IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="maxPreBuffered">The max pre buffered.</param>
        /// <returns></returns>
        public static IEnumerable<T> WrapEnumerable<T>(this IEnumerable<T> source, int maxPreBuffered = 1)
        {
            return new AsynchronousEnumerableWrapper<T>(source, maxPreBuffered);
        }

        /// <summary>
        /// Wraps an asynchronous queryable source around this IQueryable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="maxPrebuffered">The max prebuffered.</param>
        /// <returns></returns>
        public static IQueryable<T> WrapQueryableAsync<T>(this IQueryable<T> source, int maxPrebuffered)
        {
            return new AsynchronourQueryableWrapper<T>(source, maxPrebuffered);
        }
    }
}

namespace System.Collections.Generic
{
    public static class ExtensionMethods
    {
        public static void AddRange<T>(this ObservableCollection<T> target, IEnumerable<T> items)
        {
            target.Guard("target");
            items.Guard("items");
            foreach (var item in items)
                target.Add(item);
        }

        public static T[,] SwapPlanes<T>(this T[,] input)
        {
            var yDim = input.GetLength(1);
            var xDim = input.GetLength(0);
            var output = new T[input.GetLength(1), input.GetLength(0)];
            for (var y = 0; y < yDim; ++y)
            {
                for (var x = 0; x < xDim; ++x)
                {
                    output[y, x] = input[x, y];
                }
            }
            return output;
        }
    }
}
