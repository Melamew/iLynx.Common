using System;
using System.Threading;
using iLynx.Common.Threading;

namespace iLynx.Common.Collections
{
    public interface ICacheEntry<T>
    {
        T Item { get; set; }
    }

    public abstract class CacheEntryBase<T> : IDisposable, ICacheEntry<T>
    {
        public abstract T Item { get; set; }

        ~CacheEntryBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing){}

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public class PassivelyTimedCacheEntry<T> : CacheEntryBase<T>
    {
        private T item;
        public DateTime Updated { get; private set; }

        public PassivelyTimedCacheEntry(T item)
        {
            this.item = item;
            Updated = DateTime.Now;
        }

        public PassivelyTimedCacheEntry(T item, DateTime lastUpdated)
        {
            this.item = item;
            Updated = lastUpdated;
        } 

        public override T Item
        {
            get { return item; }
            set
            {
                item = value;
                Updated = DateTime.Now;
            }
        }
    }

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