using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace iLynx.Common.Collections
{
    /// <summary>
    /// DirectAsynchronousEnumerator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DirectThreadedEnumerator<T> : IEnumerator<T>
    {
        /// <summary>
        /// The source
        /// </summary>
        private readonly IEnumerator<T> source;
        /// <summary>
        /// The worker thread
        /// </summary>
        private readonly Thread workerThread;
        /// <summary>
        /// The movement event
        /// </summary>
        private readonly AutoResetEvent movementEvent = new AutoResetEvent(true);
        /// <summary>
        /// The output event
        /// </summary>
        private readonly AutoResetEvent outputEvent = new AutoResetEvent(false);
        /// <summary>
        /// The last state
        /// </summary>
        private volatile bool lastState = true;
        /// <summary>
        /// The disposed
        /// </summary>
        private volatile bool disposed;
        /// <summary>
        /// The reset
        /// </summary>
        private volatile bool reset;
        /// <summary>
        /// The last item
        /// </summary>
        private T lastItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectThreadedEnumerator{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public DirectThreadedEnumerator(IEnumerator<T> source)
        {
            this.source = source;
            workerThread = new Thread(Work);
            workerThread.Start();
        }

        /// <summary>
        /// Works this instance.
        /// </summary>
        private void Work()
        {
            while (!disposed && lastState || reset)
            {
                if (reset) reset = false;
                movementEvent.WaitOne();
                lastState = source.MoveNext();
                if (lastState)
                    lastItem = source.Current;
                outputEvent.Set();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposed = true;
            movementEvent.Set();
            workerThread.Join();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            outputEvent.WaitOne();
            return lastState;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            outputEvent.WaitOne();
            reset = true;
            source.Reset();
            movementEvent.Set();
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public T Current
        {
            get
            {
                try
                {
                    return lastItem;
                }
                finally
                {
                    movementEvent.Set();
                }
            }
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