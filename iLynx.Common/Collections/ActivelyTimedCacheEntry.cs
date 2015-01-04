using System;
using System.Threading;

namespace iLynx.Common.Collections
{
    public class ActivelyTimedCacheEntry<T> : PassivelyTimedCacheEntry<T>
    {
        private TimeSpan maxAge;
        private readonly ITimerService timerService;
        private int timerId = -1;

        public ActivelyTimedCacheEntry(ITimerService timerService, TimeSpan maxAge, T item)
            : this(timerService, item, maxAge, DateTime.Now)
        {
        }

        protected ActivelyTimedCacheEntry(ITimerService timerService, T item, TimeSpan maxAge, DateTime lastUpdated)
            : base(item, lastUpdated)
        {
            var nextTimeout = lastUpdated + maxAge - DateTime.Now;
            if (nextTimeout.TotalMilliseconds < 0d) throw new ArgumentOutOfRangeException("The timeout for this entry has already passed");
            this.timerService = timerService;
            this.maxAge = maxAge;
            timerId = this.timerService.StartNew(OnTimeout, (int)nextTimeout.TotalMilliseconds, Timeout.Infinite);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            if (-1 == timerId) return;
            timerService.Stop(timerId);
            timerId = -1;
        }

        private void ResetTimer()
        {
            timerService.Change(timerId, (int)maxAge.TotalMilliseconds, Timeout.Infinite);
        }

        private void OnTimeout()
        {
            OnExpired();
        }

        protected virtual void OnExpired()
        {
            var handler = Expired;
            if (null == handler) return;
            handler(this, EventArgs.Empty);
        }

        public override T Item
        {
            get
            {
                return base.Item;
            }
            set
            {
                base.Item = value;
                ResetTimer();
            }
        }

        public event EventHandler Expired;
    }
}