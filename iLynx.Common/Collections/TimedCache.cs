using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace iLynx.Common.Collections
{
    public class TimedCache<T> : INotifyCollectionChanged, ICache<T> where T : IIDentifiable
    {
        private readonly ITimerService timerService;
        private readonly Dictionary<int, ActivelyTimedCacheEntry<T>> storage = new Dictionary<int, ActivelyTimedCacheEntry<T>>();

        public TimedCache(ITimerService timerService)
        {
            this.timerService = timerService;
            Timeout = TimeSpan.FromMinutes(30d);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return storage.Select(x => x.Value.Item).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TimeSpan Timeout
        {
            get;
            set;
        }

        public T FindItem(int key)
        {
            ActivelyTimedCacheEntry<T> entry;
            return storage.TryGetValue(key, out entry) ? entry.Item : default(T);
        }

        public void AddOrUpdate(T item)
        {
            if (Equals(null, item)) throw new ArgumentNullException("item");
            var key = item.Key;
            ActivelyTimedCacheEntry<T> entry;
            if (storage.TryGetValue(key, out entry))
            {
                var oldItem = entry.Item;
                entry.Item = item;
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, item, oldItem);
            }
            else
            {
                entry = new ActivelyTimedCacheEntry<T>(timerService, Timeout, item);
                entry.Expired += EntryOnExpired;
                storage.Add(key, entry);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, default(T));
            }
        }

        private void EntryOnExpired(object sender, EventArgs eventArgs)
        {
            var entry = sender as ActivelyTimedCacheEntry<T>;
            if (null == entry) return;
            Remove(entry.Item);
        }

        public void Clear()
        {
            storage.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, default(T), default(T));
        }

        public bool Contains(T item)
        {
            if (Equals(null, item)) throw new ArgumentNullException("item");
            return storage.ContainsKey(item.Key);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var entries = storage.Select(x => x.Value.Item).ToArray();
            Array.Copy(entries, 0, array, arrayIndex, entries.Length);
        }

        public bool Remove(T item)
        {
            if (Equals(null, item)) throw new ArgumentNullException("item");
            var key = item.Key;
            try
            {
                return storage.Remove(key);
            }
            finally
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, default(T), item);
            }
        }

        public int Count { get { return storage.Count; } }
        
        public void Dispose()
        {

        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, T newItem, T oldItem)
        {
            var handler = CollectionChanged;
            if (null == handler) return;
            handler(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
