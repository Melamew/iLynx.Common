using System.Collections;
using System.Collections.Generic;

namespace iLynx.Common.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsynchronousEnumerableWrapper<T> : IEnumerable<T>
    {
        /// <summary>
        /// The source
        /// </summary>
        private readonly IEnumerable<T> source;
        /// <summary>
        /// The max pre buffered
        /// </summary>
        private readonly int maxPreBuffered;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronousEnumerableWrapper{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public AsynchronousEnumerableWrapper(IEnumerable<T> source)
        {
            this.source = source;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronousEnumerableWrapper{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="maxPreBuffered">The max pre buffered.</param>
        public AsynchronousEnumerableWrapper(IEnumerable<T> source, int maxPreBuffered)
        {
            this.source = source;
            this.maxPreBuffered = maxPreBuffered;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (maxPreBuffered <= 1)
                return new DirectThreadedEnumerator<T>(source.GetEnumerator());
            return new BufferedThreadedEnumerator<T>(source.GetEnumerator(), maxPreBuffered);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
