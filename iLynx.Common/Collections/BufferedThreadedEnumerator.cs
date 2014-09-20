using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace iLynx.Common.Collections
{
    /// <summary>
    /// BufferedEnumerator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BufferedThreadedEnumerator<T> : IEnumerator<T>
    {
        /// <summary>
        /// The source
        /// </summary>
        private readonly IEnumerator<T> source;
        /// <summary>
        /// The max pre buffer count
        /// </summary>
        private readonly int maxPreBufferCount;
        /// <summary>
        /// The worker
        /// </summary>
        private readonly Thread worker;
        /// <summary>
        /// The disposed
        /// </summary>
        private volatile bool disposed;
        /// <summary>
        /// The reset
        /// </summary>
        private volatile bool reset;
        /// <summary>
        /// The pause
        /// </summary>
        private volatile bool pause;
        /// <summary>
        /// The thread paused
        /// </summary>
        private volatile bool threadPaused;
        /// <summary>
        /// The end
        /// </summary>
        private volatile bool end;
        /// <summary>
        /// The items
        /// </summary>
        private readonly ConcurrentQueue<BufferedItem> items = new ConcurrentQueue<BufferedItem>();

        /// <summary>
        /// BufferedItem
        /// </summary>
        private class BufferedItem
        {
            /// <summary>
            /// Gets or sets the item.
            /// </summary>
            /// <value>
            /// The item.
            /// </value>
            public T Item { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [new state].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [new state]; otherwise, <c>false</c>.
            /// </value>
            public bool NewState { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedThreadedEnumerator{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="maxPreBufferCount">The max pre buffer count.</param>
        public BufferedThreadedEnumerator(IEnumerator<T> source, int maxPreBufferCount)
        {
            this.source = source;
            this.maxPreBufferCount = maxPreBufferCount;
            worker = new Thread(Work);
            worker.Start();
        }

        /// <summary>
        /// Works this instance.
        /// </summary>
        private void Work()
        {
            var newState = source.MoveNext();
            while (!disposed || reset)
            {
                if (newState)
                {
                    var item = source.Current;
                    newState = source.MoveNext();
                    items.Enqueue(new BufferedItem { Item = item, NewState = newState });
                    end = !newState;
                }
                else if (reset)
                {
                    reset = false;
                    newState = source.MoveNext();
                    Thread.CurrentThread.Join(1);
                    continue;
                }
                else
                    return; // Hm...
                while (items.Count >= maxPreBufferCount)
                {
                    threadPaused = pause;
                    Thread.CurrentThread.Join(1);
                }
                while (pause)
                    threadPaused = pause;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposed = true;
            worker.Join();
        }

        /// <summary>
        /// The next state
        /// </summary>
        private bool nextState = true;
        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            BufferedItem item;
            var result = items.TryDequeue(out item);
            while (!end && !result)
            {
                Thread.CurrentThread.Join(1);
                result = items.TryDequeue(out item);
            }
            if (!result) return false; // Something probably went wrong...
            try
            {
                Current = item.Item;
                return nextState; // This is still "current state" at this point
            }
            finally
            {
                nextState = item.NewState;
            }
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            pause = true;
            while (!threadPaused)
                Thread.CurrentThread.Join(1);
            reset = true;
            source.Reset();
            pause = false;
        }

        /// <summary>
        /// The current
        /// </summary>
        private T current;
        /// <summary>
        /// The read lock
        /// </summary>
        private readonly object readLock = new object();
        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public T Current
        {
            get { lock (readLock) return current; }
            set { lock (readLock) current = value; }
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}