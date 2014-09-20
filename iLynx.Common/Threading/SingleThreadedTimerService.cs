using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace iLynx.Common.Threading
{
    /// <summary>
    /// A 'lightweight' class for timers that uses a single thread to manage multiple timers.
    /// </summary>
    public class SingleThreadedTimerService : ITimerService, IDisposable
    {
        private int nextId = int.MinValue;
        private readonly ReaderWriterLockSlim callbackLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly SortedList<DateTime, List<TimerDefinition>> timerCallbacks = new SortedList<DateTime, List<TimerDefinition>>();
        private readonly Timer mainTimer;

        /// <summary>
        /// 
        /// </summary>
        public SingleThreadedTimerService()
        {
            mainTimer = new Timer(Tick);
        }

        private void Tick(object state)
        {
            if (0 >= timerCallbacks.Count) return;
            callbackLock.EnterUpgradeableReadLock();
            try
            {
                var entries = timerCallbacks.First();
                callbackLock.EnterWriteLock();
                try
                {
                    var vals = new TimerDefinition[entries.Value.Count];
                    entries.Value.CopyTo(vals);
                    foreach (var entry in vals)
                    {
                        entry.Callback.Invoke();
                        AddDefinition(entry,
                            Timeout.Infinite == entry.Interval
                            ? Timeout.InfiniteTimeSpan
                            : TimeSpan.FromMilliseconds(entry.Interval));
                    }
                    timerCallbacks.Remove(entries.Key);
                    ResetMain();
                }
                finally
                {
                    callbackLock.ExitWriteLock();
                }
            }
            finally
            {
                callbackLock.ExitUpgradeableReadLock();
            }
        }

        private void ResetMain()
        {
            var nextDue = GetNextDue();
            if (DateTime.MaxValue == nextDue) return;
            var delta = nextDue - DateTime.Now;
            if (delta.TotalMilliseconds < 0)
            {
                RuntimeCommon.DefaultLogger.Log(LoggingType.Warning, this, string.Format("Next timer interval is < 0 milliseconds away ({0}), restarting at 0", delta));
                delta = TimeSpan.FromMilliseconds(0);
            }
            mainTimer.Change(delta, Timeout.InfiniteTimeSpan);
        }

        private DateTime GetNextDue()
        {
            return 0 >= timerCallbacks.Count ? DateTime.MaxValue : timerCallbacks.FirstOrDefault().Key;
        }

        /// <summary>
        /// Starts a new timer with the specified timeout and interval
        /// <para/>
        /// This method will return an identifier that can be used to modify the timer.
        /// </summary>
        /// <param name="callback">The delegate that is invoked each time the timer expires</param>
        /// <param name="timeout">The time (in milliseconds) it takes for the timer's first tick to occur (Use <see cref="Timeout.Infinite"/> to make this timer never fire)</param>
        /// <param name="interval">The time (in milliseconds) between all the following ticks of this timer (Use <see cref="Timeout.Infinite"/> to only fire once the timeout is reached)</param>
        /// <returns></returns>
        public int StartNew(Action callback, int timeout, int interval)
        {
            Guard.IsNull(() => callback);
            RuntimeCommon.DefaultLogger.Log(LoggingType.Information, this, string.Format("StartNew from {0}, timeout {1} ms, interval {2} ms", callback.Target, timeout, interval));
            TimeSpan dueIn;
            var definition = MakeDefinition(callback, timeout, interval, out dueIn);
            callbackLock.EnterWriteLock();
            AddDefinition(definition, dueIn);
            callbackLock.ExitWriteLock();
            ResetMain();
            return definition.Id;
        }

        private void AddDefinition(TimerDefinition definition, TimeSpan dueIn)
        {
            List<TimerDefinition> existing;
            var dueAt = dueIn == Timeout.InfiniteTimeSpan ? DateTime.MaxValue : DateTime.Now + dueIn;
            if (!timerCallbacks.TryGetValue(dueAt, out existing))
            {
                existing = new List<TimerDefinition>();
                timerCallbacks.Add(dueAt, existing);
            }
            existing.Add(definition);
        }

        private TimerDefinition MakeDefinition(Action callback, int timeout, int interval, out TimeSpan dueIn)
        {
            return MakeDefinition(callback, timeout, interval, NextId, out dueIn);
        }

        private TimerDefinition MakeDefinition(Action callback, int timeout, int interval, int id, out TimeSpan dueIn)
        {
            var definition = new TimerDefinition
            {
                Id = id,
                Callback = callback,
                Interval = interval
            };
            dueIn = timeout == Timeout.Infinite
                ? (interval == Timeout.Infinite ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(interval))
                : TimeSpan.FromMilliseconds(timeout);
            return definition;
        }

        /// <summary>
        /// Stops the timer with the specified id.
        /// </summary>
        /// <param name="id">The id of the timer to stop</param>
        public void Stop(int id)
        {
            callbackLock.EnterUpgradeableReadLock();
            try
            {
                var keyValuePair = timerCallbacks.FirstOrDefault(x => x.Value.Any(y => y.Id == id));
                if (null == keyValuePair.Value) return;
                callbackLock.EnterWriteLock();
                try
                {
                    var list = keyValuePair.Value;
                    list.RemoveAll(x => x.Id == id);
                }
                finally
                {
                    callbackLock.ExitWriteLock();
                }

            }
            finally
            {
                callbackLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Changes the properties of the timer with the specified id.
        /// </summary>
        /// <param name="id">The id of the timer to modify</param>
        /// <param name="newTimeout">The new timeout that is used for this timer</param>
        /// <param name="newInterval">The new interval that is used for this timer</param>
        public void Change(int id, int newTimeout, int newInterval)
        {
            callbackLock.EnterUpgradeableReadLock();
            try
            {
                var keyValuePair = timerCallbacks.FirstOrDefault(x => x.Value.Any(y => y.Id == id));
                var list = keyValuePair.Value;
                if (null == list) return;
                callbackLock.EnterWriteLock();
                try
                {
                    var definition = list.FirstOrDefault(x => x.Id == id);
                    if (null == definition) return;
                    list.Remove(definition);
                    TimeSpan dueIn;
                    var newDefinition = MakeDefinition(definition.Callback, newTimeout, newInterval, definition.Id, out dueIn);
                    AddDefinition(newDefinition, dueIn);
                }
                finally { callbackLock.ExitWriteLock(); }
            }
            finally { callbackLock.ExitUpgradeableReadLock(); }
        }

        private int NextId
        {
            get { return nextId++; }
        }

        private class TimerDefinition
        {
            public int Id { get; set; }
            public Action Callback { get; set; }
            public int Interval { get; set; }
        }

        public void Dispose()
        {
            mainTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}