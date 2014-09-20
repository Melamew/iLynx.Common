using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace iLynx.Common.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsynchronourQueryableWrapper<T> : IQueryable<T>
    {
        /// <summary>
        /// The source
        /// </summary>
        private readonly IQueryable<T> source;
        /// <summary>
        /// The max pre buffered
        /// </summary>
        private readonly int maxPreBuffered;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronourQueryableWrapper{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="maxPreBuffered">The max pre buffered.</param>
        public AsynchronourQueryableWrapper(IQueryable<T> source, int maxPreBuffered = 0)
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
            if (maxPreBuffered <= 1) return new DirectThreadedEnumerator<T>(source.GetEnumerator());
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

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of <see cref="T:System.Linq.IQueryable" />.</returns>
        public Expression Expression { get { return source.Expression; } }
        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable" /> is executed.
        /// </summary>
        /// <returns>A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.</returns>
        public Type ElementType { get { return source.ElementType; } }
        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.</returns>
        public IQueryProvider Provider { get { return source.Provider; } }
    }
}