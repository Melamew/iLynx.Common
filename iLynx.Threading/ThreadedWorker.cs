using System;
using System.Threading;
using iLynx.Common;

namespace iLynx.Threading
{
    /// <summary>
    /// ThreadedWorker
    /// </summary>
    public class ThreadedWorker : ThreadedWorkerBase
    {
        private readonly Action target;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadedWorker" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="apartmentState">State of the apartment.</param>
        /// <param name="isBackgroundThread"></param>
        public ThreadedWorker(Action target, ILogger logger, ApartmentState apartmentState = ApartmentState.MTA, bool isBackgroundThread = true)
            : base(logger, apartmentState, isBackgroundThread)
        {
            target.Guard("target");
            this.target = target;
        }

        /// <summary>
        /// Executes the internal.
        /// </summary>
        /// <param name="args">The args.</param>
        protected override void ExecuteInternal(object args)
        {
            target();
        }
    }
}
